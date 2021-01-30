// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using MDSDK.Dicom.Serialization.TransferSyntaxes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

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

            public Action<object, DicomStreamReader> DeserializePropertyValue { get; }

            public Action<object, DicomStreamWriter> SerializePropertyValue { get; }

            public Func<object, DicomVRCoding, long> GetSerializedPropertyLength { get; }

            public PropertySerializer(PropertyInfo property, ValueRepresentation vr, MethodInfo readValueMethod, MethodInfo writeValueMethod)
            {
                Property = property;
                VR = vr;
                ReadValueMethod = readValueMethod;
                WriteValueMethod = writeValueMethod;

                DeserializePropertyValue = (object obj, DicomStreamReader reader) =>
                {
                    try
                    {
                        var value = ReadValueMethod.Invoke(VR, new object[] { reader });
                        property.SetValue(obj, value);
                    }
                    catch (Exception error)
                    {
                        var rootCause = error.GetBaseException();
                        throw new Exception($"Error deserializing {property} using VR {VR}: {rootCause.Message}", rootCause);
                    }
                };

                SerializePropertyValue = (object obj, DicomStreamWriter writer) =>
                {
                    try
                    {
                        var value = property.GetValue(obj);
                        if (value != null)
                        {
                            WriteValueMethod.Invoke(VR, new object[] { writer, value });
                        }
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
                        nameof(IHasLightWeightValueLengthCalculation<object>.GetUnpaddedValueLength));

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

        private readonly SortedList<DicomTag, PropertySerializer> _propertySerializers = new();

        private static PropertySerializer MakePropertySerializer(PropertyInfo property, IEnumerable<ValueRepresentation> candidateVRs)
        {
            ValueRepresentation theVR = null;

            MethodInfo readValueMethod = null;
            MethodInfo writeValueMethod = null;

            foreach (var vr in candidateVRs)
            {
                var vrType = vr.GetType();

                var singleValueInterface = typeof(ISingleValue<>).MakeGenericType(property.PropertyType);
                var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(property.PropertyType);

                if (singleValueInterface.IsAssignableFrom(vrType))
                {
                    readValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.ReadValue));
                    writeValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.WriteValue));
                }
                else if (multiValueInterface.IsAssignableFrom(vrType))
                {
                    readValueMethod = property.PropertyType.IsArray
                        ? multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadValues))
                        : multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadSingleValue));
                    writeValueMethod = property.PropertyType.IsArray
                        ? multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteValues))
                        : multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteSingleValue));
                }
                else
                {
                    continue;
                }

                theVR = vr;
                break;
            }

            if (theVR == null)
            {
                throw new Exception($"No matching VR found for serializing {property}");
            }

            return new PropertySerializer(property, theVR, readValueMethod, writeValueMethod);
        }

        internal DicomSerializer(Type type)
        {
            foreach (var property in type.GetProperties())
            {
                var dicomAttributeField = typeof(DicomAttribute).GetField(property.Name, BindingFlags.Public | BindingFlags.Static);
                if (dicomAttributeField == null)
                {
                    throw new NotSupportedException($"Property {property.Name} in {type.Name} is not the keyword of a known DICOM attribute");
                }
                var dicomAttribute = (DicomAttribute)dicomAttributeField.GetValue(null);
                IEnumerable<ValueRepresentation> candidateVRs = dicomAttribute.VRs;
                if (dicomAttribute.TryGetExtendedVR(out ValueRepresentation extendedVR))
                {
                    candidateVRs = Enumerable.Repeat(extendedVR, 1).Concat(candidateVRs);
                }
                var propertySerializer = MakePropertySerializer(property, candidateVRs);
                _propertySerializers.Add(dicomAttribute.Tag, propertySerializer);
            }
        }

        internal void DeserializeProperties(object obj, DicomStreamReader reader)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                if (reader.TrySeek(tag))
                {
                    propertySerializer.DeserializePropertyValue.Invoke(obj, reader);
                    reader.EndReadValue();
                }
            }
        }

        internal void SerializeProperties(object obj, DicomStreamWriter writer)
        {
            foreach (var (tag, propertyValueSerializer) in _propertySerializers)
            {
                writer.WriteTag(tag);
                propertyValueSerializer.SerializePropertyValue.Invoke(obj, writer);
            }
        }

        internal bool TryGetSerializedLength(object obj, DicomVRCoding vrCoding, out long serializedLength)
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
        }
    }

    public class DicomSerializer<T> : DicomSerializer where T : new()
    {
        internal DicomSerializer()
            : base(typeof(T))
        {
        }

        public T Deserialize(DicomStreamReader reader)
        {
            var obj = new T();
            DeserializeProperties(obj, reader);
            return obj;
        }

        public bool TryGetSerializedLength(T obj, DicomVRCoding vrCoding, out uint serializedLength)
        {
            if (TryGetSerializedLength(obj, vrCoding, out long length) && (length< uint.MaxValue))
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

        public void Serialize(T obj, DicomStreamWriter writer)
        {
            SerializeProperties(obj, writer);
        }
    }
}
