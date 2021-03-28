// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
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
        private static Type GetNonNullableType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        class PropertySerializer
        {
            public DicomPropertyInfo DicomProperty { get; }

            public Type NonNullablePropertyType { get; }

            public ValueRepresentation VR { get; }

            public MethodInfo ReadValueMethod { get; }

            public MethodInfo WriteValueMethod { get; }

            public Func<DicomStreamReader, object> DeserializePropertyValue { get; }

            public Action<DicomStreamWriter, object> SerializePropertyValue { get; }

            public Func<object, DicomVRCoding, long> GetSerializedPropertyLength { get; }

            public PropertySerializer(DicomPropertyInfo dicomProperty, ValueRepresentation vr, MethodInfo readValueMethod, MethodInfo writeValueMethod)
            {
                DicomProperty = dicomProperty;
                NonNullablePropertyType = GetNonNullableType(dicomProperty.PropertyType);
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
                        throw new Exception($"Error deserializing {dicomProperty} using VR {VR}: {rootCause.Message}", rootCause);
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
                        throw new Exception($"Error serializing {dicomProperty} using VR {VR}: {rootCause.Message}", rootCause);
                    }
                };

                var hasLightWeightValueLengthCalculationInterface = typeof(IHasLightWeightValueLengthCalculation<>).MakeGenericType(
                    NonNullablePropertyType);

                if (hasLightWeightValueLengthCalculationInterface.IsAssignableFrom(VR.GetType()))
                {
                    var getUnpaddedValueLengthMethod = hasLightWeightValueLengthCalculationInterface.GetMethod(
                        nameof(IHasLightWeightValueLengthCalculation<object>.GetUnpaddedValueLength),
                        new[] { NonNullablePropertyType });

                    GetSerializedPropertyLength = (object obj, DicomVRCoding vrCoding) =>
                    {
                        var value = dicomProperty.GetValue(obj);
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

        public Type DicomObjectType { get; }

        private readonly SortedList<DicomTag, PropertySerializer> _propertySerializers = new();

        private static bool TryMakePropertySerializer(DicomPropertyInfo dicomProperty, ValueRepresentation vr, 
            out PropertySerializer propertySerializer)
        {
            var nonNullablePropertyType = GetNonNullableType(dicomProperty.PropertyType);
            
            if (vr is Sequence<object>)
            {
                if (nonNullablePropertyType.IsGenericType && nonNullablePropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var itemType = nonNullablePropertyType.GetGenericArguments()[0];
                    vr = (ValueRepresentation)Activator.CreateInstance(typeof(Sequence<>).MakeGenericType(itemType));
                }
            }

            var vrType = vr.GetType();

            var singleValueInterface = typeof(ISingleValue<>).MakeGenericType(nonNullablePropertyType);
            if (singleValueInterface.IsAssignableFrom(vrType))
            {
                var readValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.ReadValue));
                var writeValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.WriteValue));
                propertySerializer = new PropertySerializer(dicomProperty, vr, readValueMethod, writeValueMethod);
                return true;
            }

            if (nonNullablePropertyType.IsArray)
            {
                var elementType = nonNullablePropertyType.GetElementType();
                var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(elementType);
                if (multiValueInterface.IsAssignableFrom(vrType))
                {
                    var readValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadValues));
                    var writeValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteValues));
                    propertySerializer = new PropertySerializer(dicomProperty, vr, readValueMethod, writeValueMethod);
                    return true;
                }
            }
            else
            {
                var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(nonNullablePropertyType);
                if (multiValueInterface.IsAssignableFrom(vrType))
                {
                    var readValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadSingleValue));
                    var writeValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteSingleValue));
                    propertySerializer = new PropertySerializer(dicomProperty, vr, readValueMethod, writeValueMethod);
                    return true;
                }
            }

            propertySerializer = null;
            return false;
        }

        private PropertySerializer MakeEnumPropertySerializer(DicomPropertyInfo dicomProperty, Type enumType)
        {
            Type genericEnumVRType = null;

            var dicomAttribute = dicomProperty.DicomAttribute;

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
                throw new NotSupportedException($"Enum property {dicomProperty} must map to single-VR DICOM attribute with VR US or CS");
            }

            var enumVRType = genericEnumVRType.MakeGenericType(enumType);
            var enumVR = (ValueRepresentation)Activator.CreateInstance(enumVRType);

            if (!TryMakePropertySerializer(dicomProperty, enumVR, out PropertySerializer propertySerializer))
            {
                throw new Exception($"Unexpected failure creating enum property serializer for {dicomProperty}");
            }

            return propertySerializer;
        }

        private PropertySerializer MakePropertySerializer(DicomPropertyInfo dicomProperty)
        {
            var nonNullablePropertyType = GetNonNullableType(dicomProperty.PropertyType);
            
            if (nonNullablePropertyType.IsEnum)
            {
                return MakeEnumPropertySerializer(dicomProperty, nonNullablePropertyType);
            }

            if (nonNullablePropertyType.IsArray)
            {
                var elementType = nonNullablePropertyType.GetElementType();
                if (elementType.IsEnum)
                {
                    return MakeEnumPropertySerializer(dicomProperty, elementType);
                }
            }

            var dicomAttribute = dicomProperty.DicomAttribute;

            foreach (var standardDefinedVR in dicomAttribute.VRs)
            {
                if (TryMakePropertySerializer(dicomProperty, standardDefinedVR, out PropertySerializer propertySerializer))
                {
                    return propertySerializer;
                }
            }

            if (dicomAttribute.TryGetExtendedVR(out ValueRepresentation extendedVR))
            {
                if (TryMakePropertySerializer(dicomProperty, extendedVR, out PropertySerializer propertySerializer))
                {
                    return propertySerializer;
                }
            }

            throw new Exception($"No VR found for serializing {dicomProperty}");
        }

        internal DicomSerializer(Type dicomObjectType)
        {
            DicomObjectType = dicomObjectType;
        }

        internal void Initialize()
        {
            foreach (var dicomProperty in DicomPropertyInfo.GetDicomProperties(null, DicomObjectType))
            {
                var propertySerializer = MakePropertySerializer(dicomProperty);
                _propertySerializers.Add(dicomProperty.DicomAttribute.Tag, propertySerializer);
            }
        }

        internal void DeserializeProperties(DicomStreamReader reader, object obj)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                if (reader.TrySeek(tag))
                {
                    var propertyValue = propertySerializer.DeserializePropertyValue.Invoke(reader);
                    propertySerializer.DicomProperty.SetValue(obj, propertyValue);
                }
                else
                {
                    propertySerializer.DicomProperty.SetValue(obj, null); // assigns default value if property type is a value type
                }
            }
        }

        internal void SerializeProperties(DicomStreamWriter writer, object obj)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                var propertyValue = propertySerializer.DicomProperty.GetValue(obj);
                if (propertyValue != null)
                {
                    writer.WriteTag(tag);
                    propertySerializer.SerializePropertyValue.Invoke(writer, propertyValue);
                }
            }
        }

        public object Deserialize(DicomStreamReader reader)
        {
            var dicomObject = Activator.CreateInstance(DicomObjectType);
            DeserializeProperties(reader, dicomObject);
            return dicomObject;
        }

        public void Serialize(DicomStreamWriter writer, object dicomObject)
        {
            SerializeProperties(writer, dicomObject);
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

        public static DicomSerializer<T> GetSerializer<T>()
        {
            lock (s_serializers)
            {
                if (!s_serializers.TryGetValue(typeof(T), out DicomSerializer serializer))
                {
                    serializer = new DicomSerializer<T>();
                    s_serializers[typeof(T)] = serializer;
                    serializer.Initialize();
                }
                return (DicomSerializer<T>)serializer;
            }
        }

        public static T Deserialize<T>(DicomTransferSyntax transferSyntax, Stream stream) where T : new()
        {
            var serializer = GetSerializer<T>();
            var input = new BinaryStreamReader(transferSyntax.ByteOrder, stream);
            var reader = new DicomStreamReader(transferSyntax.VRCoding, input);
            var obj = new T();
            serializer.DeserializeProperties(reader, obj);
            return obj;
        }

        public static void Serialize<T>(DicomTransferSyntax transferSyntax, Stream stream, T obj)
        {
            var serializer = GetSerializer<T>();
            var output = new BinaryStreamWriter(transferSyntax.ByteOrder, stream);
            var writer = new DicomStreamWriter(transferSyntax.VRCoding, output);
            serializer.SerializeProperties(writer, obj);
            output.Flush(FlushMode.Shallow);
        }
    }

    public class DicomSerializer<T> : DicomSerializer
    {
        internal DicomSerializer()
            : base(typeof(T))
        {
        }

        public new T Deserialize(DicomStreamReader reader)
        {
            var obj = Activator.CreateInstance<T>();
            DeserializeProperties(reader, obj);
            return obj;
        }

        public void Serialize(DicomStreamWriter writer, T obj)
        {
            SerializeProperties(writer, obj);
        }
    }
}
