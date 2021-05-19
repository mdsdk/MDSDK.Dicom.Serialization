// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MDSDK.Dicom.Serialization.ValueRepresentations.Extensions;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>Provided methods for serializing and deserializing C# objects to and from DICOM</summary>
    public class DicomSerializer
    {
        private static Type GetNonNullableType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        private class Converter : IEquatable<Converter>
        {
            public Type FromType { get; init; }
            public Type ToType { get; init; }
            public Func<object, object> Convert { get; init; }
            public bool Equals(Converter other) => (other != null) && (FromType == other.FromType) && (ToType == other.ToType);
            public override bool Equals(object obj) => (obj is Converter other) && Equals(other);
            public override int GetHashCode() => HashCode.Combine(FromType, ToType);
            public override string ToString() => $"{FromType}->{ToType}";
        }

        /// <summary>Adds a converter</summary>
        public static void AddMultiValueConverter<T, TConverter>(string vrId) where TConverter : IMultiValue<T> where T : struct
        {
            var vr = DicomVR.Lookup(vrId);
            vr.AddMultiValueConverter<T, TConverter>();
        }

        class PropertySerializer
        {
            public DicomPropertyInfo DicomProperty { get; }

            public Type NonNullablePropertyType { get; }

            public object VR { get; }

            public MethodInfo ReadValueMethod { get; }

            public MethodInfo WriteValueMethod { get; }

            public Func<DicomStreamReader, object> DeserializePropertyValue { get; }

            public Action<DicomStreamWriter, object> SerializePropertyValue { get; }

            public Func<object, DicomVRCoding, long> GetSerializedPropertyLength { get; }

            public PropertySerializer(DicomPropertyInfo dicomProperty, object vr, MethodInfo readValueMethod, MethodInfo writeValueMethod)
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
                            return DicomStreamWriter.GetDataElementLength((ValueRepresentation)VR, vrCoding, unpaddedValueLength);
                        }
                    };
                }
            }
        }

        internal Type DicomObjectType { get; }

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
                if (vr.TryGetMultiValueConverter(elementType, out object converter) || multiValueInterface.IsAssignableFrom(vrType))
                {
                    var readValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadValues));
                    var writeValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteValues));
                    propertySerializer = new PropertySerializer(dicomProperty, converter ?? vr, readValueMethod, writeValueMethod);
                    return true;
                }
            }
            else
            {
                var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(nonNullablePropertyType);
                if (vr.TryGetMultiValueConverter(nonNullablePropertyType, out object converter) || multiValueInterface.IsAssignableFrom(vrType))
                {
                    var readValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadSingleValue));
                    var writeValueMethod = multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteSingleValue));
                    propertySerializer = new PropertySerializer(dicomProperty, converter ?? vr, readValueMethod, writeValueMethod);
                    return true;
                }
            }

            propertySerializer = null;
            return false;
        }

        private PropertySerializer MakeEnumPropertySerializer(DicomPropertyInfo dicomProperty, Type enumType)
        {
            Type genericEnumVRType;

            var vr = dicomProperty.DicomAttribute.VR;
            if (vr == DicomVR.US)
            {
                if (enumType.GetEnumUnderlyingType() != typeof(ushort))
                {
                    throw new Exception($"Underlying type of {enumType} must be ushort");
                }
                genericEnumVRType = typeof(EnumUnsignedShort<>);
            }
            else if (vr == DicomVR.CS)
            {
                genericEnumVRType = typeof(EnumCodeString<>);
            }
            else
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

            var vr = dicomProperty.DicomAttribute.VR;

            if (!TryMakePropertySerializer(dicomProperty, vr, out PropertySerializer propertySerializer))
            {
                throw new Exception($"{dicomProperty} is not compatible with VR {vr}");
            }

            return propertySerializer;
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

        internal void DeserializeProperties(DicomStreamReader dicomStreamReader, object obj)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                if (dicomStreamReader.TrySeek(tag))
                {
                    if ((dicomStreamReader.ValueLength == 0) && propertySerializer.NonNullablePropertyType.IsValueType)
                    {
                        dicomStreamReader.EndReadValue();
                        propertySerializer.DicomProperty.SetValue(obj, null); // assign default value 
                    }
                    else
                    {
                        var propertyValue = propertySerializer.DeserializePropertyValue.Invoke(dicomStreamReader);
                        propertySerializer.DicomProperty.SetValue(obj, propertyValue);
                    }
                }
                else
                {
                    propertySerializer.DicomProperty.SetValue(obj, null); // assigns default value if property type is a value type
                }
            }
        }

        internal void SerializeProperties(DicomStreamWriter dicomStreamWriter, object obj)
        {
            foreach (var (tag, propertySerializer) in _propertySerializers)
            {
                var propertyValue = propertySerializer.DicomProperty.GetValue(obj);
                if (propertyValue != null)
                {
                    dicomStreamWriter.WriteTag(tag);
                    propertySerializer.SerializePropertyValue.Invoke(dicomStreamWriter, propertyValue);
                }
            }
        }

        /// <summary>Deserializes a C# object from a DICOM data stream</summary>
        public object Deserialize(DicomStreamReader reader)
        {
            var dicomObject = Activator.CreateInstance(DicomObjectType);
            DeserializeProperties(reader, dicomObject);
            return dicomObject;
        }

        /// <summary>Serializes a C# object to a DICOM data stream</summary>
        public void Serialize(DicomStreamWriter writer, object dicomObject)
        {
            SerializeProperties(writer, dicomObject);
        }

        /// <summary>Tries to calculate the length of the resulting DICOM data stream when the given object is serialized using the given VR coding</summary>
        public bool TryGetSerializedLength(object obj, DicomVRCoding vrCoding, out long serializedLength)
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

        /// <summary>Returns a DicomSerializer that can be used to serialize and deserialize C# objects of type T to and from DICOM</summary>
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

        /// <summary>Deserialize a C# object of type T from a DICOM data stream using the given transfer syntax</summary>
        public static T Deserialize<T>(BufferedStreamReader input, DicomUID transferSyntaxUID) where T : new()
        {
            var serializer = GetSerializer<T>();
            var dicomStreamReader = DicomStreamReader.Create(input, transferSyntaxUID);
            var obj = new T();
            serializer.DeserializeProperties(dicomStreamReader, obj);
            return obj;
        }

        /// <summary>Serializes a C# object of type T to a DICOM data stream using the given transfer syntax</summary>
        public static void Serialize<T>(BufferedStreamWriter output, DicomUID transferSyntaxUID, T obj)
        {
            var serializer = GetSerializer<T>();
            var dicomStreamWriter = DicomStreamWriter.Create(output, transferSyntaxUID);
            serializer.SerializeProperties(dicomStreamWriter, obj);
            output.Flush(FlushMode.Shallow);
        }
    }

    /// <summary>Provided methods for serializing and deserializing C# objects of type T to and from DICOM</summary>
    public class DicomSerializer<T> : DicomSerializer
    {
        internal DicomSerializer()
            : base(typeof(T))
        {
        }

        /// <summary>Deserializes a C# object of type T from a DICOM data stream</summary>
        public new T Deserialize(DicomStreamReader reader)
        {
            var obj = Activator.CreateInstance<T>();
            DeserializeProperties(reader, obj);
            return obj;
        }

        /// <summary>Serializes a C# object of type T to a DICOM data stream</summary>
        public void Serialize(DicomStreamWriter writer, T obj)
        {
            SerializeProperties(writer, obj);
        }
    }
}
