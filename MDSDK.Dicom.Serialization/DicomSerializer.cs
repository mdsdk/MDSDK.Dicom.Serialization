// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using MDSDK.Dicom.Serialization.TransferSyntaxes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using MDSDK.Dicom.Serialization.ValueRepresentations.Extensions;

namespace MDSDK.Dicom.Serialization
{
    public class DicomSerializer
    {
        class PropertySerializer
        {
            public PropertyInfo Property { get; }

            public ValueRepresentation VR { get; }

            public MethodInfo ReadValueMethod { get; }

            public MethodInfo WriteValueMethod { get; }

            public Func<DicomStreamReader, object> DeserializePropertyValue { get; }

            public Action<DicomStreamWriter, object> SerializePropertyValue { get; }

            public Func<object, DicomVRCoding, long> GetSerializedPropertyLength { get; }

            public PropertySerializer(PropertyInfo property, ValueRepresentation vr, MethodInfo readValueMethod, MethodInfo writeValueMethod)
            {
                Property = property;
                VR = vr;
                ReadValueMethod = readValueMethod;
                WriteValueMethod = writeValueMethod;

                DeserializePropertyValue = (DicomStreamReader reader) =>
                {
                    try
                    {
                        return ReadValueMethod.Invoke(VR, new object[] { reader });
                    }
                    catch (Exception error)
                    {
                        var rootCause = error.GetBaseException();
                        throw new Exception($"Error deserializing {property} using VR {VR}: {rootCause.Message}", rootCause);
                    }
                };

                SerializePropertyValue = (DicomStreamWriter writer, object value) =>
                {
                    try
                    {
                        WriteValueMethod.Invoke(VR, new object[] { writer, value });
                    }
                    catch (Exception error)
                    {
                        var rootCause = error.GetBaseException();
                        throw new Exception($"Error serializing {property} using VR {VR}: {rootCause.Message}", rootCause);
                    }
                };

                var hasLightWeightValueLengthCalculationInterface = typeof(IHasLightWeightValueLengthCalculation<>).MakeGenericType(
                    property.PropertyType);

                if (hasLightWeightValueLengthCalculationInterface.IsAssignableFrom(VR.GetType()))
                {
                    var getUnpaddedValueLengthMethod = hasLightWeightValueLengthCalculationInterface.GetMethod(
                        nameof(IHasLightWeightValueLengthCalculation<object>.GetUnpaddedValueLength),
                        new[] { property.PropertyType });

                    GetSerializedPropertyLength = (object obj, DicomVRCoding vrCoding) =>
                    {
                        var value = property.GetValue(obj);
                        if (value == null)
                        {
                            return 0;
                        }
                        else
                        {
                            var unpaddedValueLength = (long)getUnpaddedValueLengthMethod.Invoke(VR, new[] { value });
                            return DicomStreamWriter.GetDataElementLength(VR, vrCoding, unpaddedValueLength);
                        }
                    };
                }
            }
        }

        private readonly Type _dicomObjectType;

        private readonly SortedList<DicomTag, PropertySerializer> _propertySerializers = new();

        private static bool TryMakePropertySerializer(PropertyInfo property, ValueRepresentation vr, out PropertySerializer propertySerializer)
        {
            var vrType = vr.GetType();

            var singleValueInterface = typeof(ISingleValue<>).MakeGenericType(property.PropertyType);
            if (singleValueInterface.IsAssignableFrom(vrType))
            {
                var readValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.ReadValue));
                var writeValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.WriteValue));
                propertySerializer = new PropertySerializer(property, vr, readValueMethod, writeValueMethod);
                return true;
            }

            var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(property.PropertyType);
            if (multiValueInterface.IsAssignableFrom(vrType))
            {
                var readValueMethod = property.PropertyType.IsArray
                    ? multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadValues))
                    : multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadSingleValue));
                var writeValueMethod = property.PropertyType.IsArray
                    ? multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteValues))
                    : multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteSingleValue));
                propertySerializer = new PropertySerializer(property, vr, readValueMethod, writeValueMethod);
                return true;
            }

            propertySerializer = null;
            return false;
        }

        private PropertySerializer MakeEnumPropertySerializer(PropertyInfo property, Type enumType, DicomAttribute dicomAttribute)
        {
            Type genericEnumVRType = null;

            if (dicomAttribute.VRs.Length == 1)
            {
                if (dicomAttribute.VRs[0] == DicomVR.US)
                {
                    if (enumType.GetEnumUnderlyingType() != typeof(ushort))
                    {
                        throw new Exception($"Underlying type of {enumType} must be ushort");
                    }
                    genericEnumVRType = typeof(EnumUnsignedShort<>);
                }
                else if (dicomAttribute.VRs[0] == DicomVR.CS)
                {
                    genericEnumVRType = typeof(EnumCodeString<>);
                }
            }

            if (genericEnumVRType == null)
            {
                throw new NotSupportedException($"Enum property {property} must map to single-VR DICOM attribute with VR US or CS");
            }

            var enumVRType = genericEnumVRType.MakeGenericType(enumType);
            var enumVR = (ValueRepresentation)Activator.CreateInstance(enumVRType);

            if (!TryMakePropertySerializer(property, enumVR, out PropertySerializer propertySerializer))
            {
                throw new Exception($"Unexpected failure creating enum property serializer for {property}");
            }

            return propertySerializer;
        }

        private PropertySerializer MakePropertySerializer(PropertyInfo property, DicomAttribute dicomAttribute)
        {
            if (property.PropertyType.IsEnum)
            {
                return MakeEnumPropertySerializer(property, property.PropertyType, dicomAttribute);
            }

            if (property.PropertyType.IsArray)
            {
                var elementType = property.PropertyType.GetElementType();
                if (elementType.IsEnum)
                {
                    return MakeEnumPropertySerializer(property, elementType, dicomAttribute);
                }
            }

            foreach (var standardDefinedVR in dicomAttribute.VRs)
            {
                if (TryMakePropertySerializer(property, standardDefinedVR, out PropertySerializer propertySerializer))
                {
                    return propertySerializer;
                }
            }

            if (dicomAttribute.TryGetExtendedVR(out ValueRepresentation extendedVR))
            {
                if (TryMakePropertySerializer(property, extendedVR, out PropertySerializer propertySerializer))
                {
                    return propertySerializer;
                }
            }

            throw new Exception($"No VR found for serializing {property}");
        }

        internal DicomSerializer(Type dicomObjectType)
        {
            _dicomObjectType = dicomObjectType;

            foreach (var property in dicomObjectType.GetProperties())
            {
                var dicomAttributeField = typeof(DicomAttribute).GetField(property.Name, BindingFlags.Public | BindingFlags.Static);
                if (dicomAttributeField == null)
                {
                    throw new NotSupportedException($"Property {property.Name} in {dicomObjectType.Name} is not the keyword of a known DICOM attribute");
                }
                var dicomAttribute = (DicomAttribute)dicomAttributeField.GetValue(null);
                var propertySerializer = MakePropertySerializer(property, dicomAttribute);
                _propertySerializers.Add(dicomAttribute.Tag, propertySerializer);
            }
        }

        internal void DeserializeProperties(object obj, DicomStreamReader reader)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                if (reader.TrySeek(tag))
                {
                    var propertyValue = propertySerializer.DeserializePropertyValue.Invoke(reader);
                    propertySerializer.Property.SetValue(obj, propertyValue);
                }
            }
        }

        internal void SerializeProperties(object obj, DicomStreamWriter writer)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                var propertyValue = propertySerializer.Property.GetValue(obj);
                if (propertyValue != null)
                {
                    writer.WriteTag(tag);
                    propertySerializer.SerializePropertyValue.Invoke(writer, propertyValue);
                }
            }
        }

        public object Deserialize(DicomStreamReader reader)
        {
            var dicomObject = Activator.CreateInstance(_dicomObjectType);
            DeserializeProperties(dicomObject, reader);
            return dicomObject;
        }

        public void Serialize(object obj, DicomStreamWriter writer)
        {
            SerializeProperties(obj, writer);
        }

        private bool TryGetSerializedLength(object obj, DicomVRCoding vrCoding, out long serializedLength)
        {
            serializedLength = 0;
            if (_propertySerializers.Any(kv => kv.Value.GetSerializedPropertyLength == null))
            {
                return false;
            }
            else
            {
                foreach (var (tag, propertySerializer) in _propertySerializers)
                {
                    serializedLength += propertySerializer.GetSerializedPropertyLength.Invoke(obj, vrCoding);
                }
                return true;
            }
        }

        public bool TryGetSerializedLength(object obj, DicomVRCoding vrCoding, out uint serializedLength)
        {
            if (TryGetSerializedLength(obj, vrCoding, out long length) && (length < uint.MaxValue))
            {
                serializedLength = (uint)length;
                return true;
            }
            else
            {
                serializedLength = 0;
                return false;
            }
        }

        private static readonly Dictionary<Type, DicomSerializer> s_serializers = new();

        public static DicomSerializer<T> GetSerializer<T>() where T : new()
        {
            lock (s_serializers)
            {
                if (!s_serializers.TryGetValue(typeof(T), out DicomSerializer serializer))
                {
                    serializer = new DicomSerializer<T>();
                    s_serializers[typeof(T)] = serializer;
                }
                return (DicomSerializer<T>)serializer;
            }
        }

        public static T Deserialize<T>(Stream stream, TransferSyntax transferSyntax) where T : new()
        {
            var serializer = GetSerializer<T>();
            var input = new BinaryStreamReader(stream, transferSyntax.ByteOrder);
            var reader = new DicomStreamReader(input, transferSyntax.VRCoding);
            var obj = new T();
            serializer.DeserializeProperties(obj, reader);
            return obj;
        }

        public static void Serialize<T>(Stream stream, TransferSyntax transferSyntax, T obj) where T : new()
        {
            var serializer = GetSerializer<T>();
            var output = new BinaryStreamWriter(stream, transferSyntax.ByteOrder);
            var writer = new DicomStreamWriter(output, transferSyntax.VRCoding);
            serializer.SerializeProperties(obj, writer);
            output.Flush(FlushMode.Shallow);
        }
    }

    public class DicomSerializer<T> : DicomSerializer where T : new()
    {
        internal DicomSerializer()
            : base(typeof(T))
        {
        }

        public new T Deserialize(DicomStreamReader reader)
        {
            var obj = new T();
            DeserializeProperties(obj, reader);
            return obj;
        }

        public void Serialize(T obj, DicomStreamWriter writer)
        {
            SerializeProperties(obj, writer);
        }
    }
}
