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
    public class DicomDeserializer
    {
        private readonly SortedList<DicomTag, Action<object, DicomStreamReader>> _propertyValueDeserializers = new();

        private static Action<object, DicomStreamReader> MakePropertyDeserializer(PropertyInfo property, 
            IEnumerable<ValueRepresentation> candidateVRs)
        {
            ValueRepresentation theVR = null;
            MethodInfo theReadValueMethod = null;

            foreach (var vr in candidateVRs)
            {
                var vrType = vr.GetType();

                var singleValueInterface = typeof(ISingleValue<>).MakeGenericType(property.PropertyType);
                var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(property.PropertyType);

                MethodInfo readValueMethod = null;

                if (singleValueInterface.IsAssignableFrom(vrType))
                {
                    readValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.ReadValue));
                }
                else if (multiValueInterface.IsAssignableFrom(vrType))
                {
                    readValueMethod = property.PropertyType.IsArray
                        ? multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadValues))
                        : multiValueInterface.GetMethod(nameof(IMultiValue<object>.ReadSingleValue));
                }

                if ((readValueMethod == null) || !property.PropertyType.IsAssignableFrom(readValueMethod.ReturnType))
                {
                    continue;
                }

                if (theReadValueMethod == null)
                {
                    theVR = vr;
                    theReadValueMethod = readValueMethod;
                }
                else if (readValueMethod != theReadValueMethod)
                {
                    throw new Exception($"Mutiple methods found for deserializing {property}: {theReadValueMethod} and {readValueMethod}");
                }
            }

            if (theReadValueMethod == null)
            {
                throw new Exception($"No matching VR found for deserializing {property}");
            }

            void DeserializePropertyValue(object obj, DicomStreamReader reader)
            {
                try
                {
                    var value = theReadValueMethod.Invoke(theVR, new object[] { reader });
                    property.SetValue(obj, value);
                }
                catch (Exception error)
                {
                    var rootCause = error.GetBaseException();
                    throw new Exception($"Error deserializing {property} using VR {theVR}: {rootCause.Message}", rootCause);
                }
            }

            return DeserializePropertyValue;
        }

        protected DicomDeserializer(Type type)
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
                var propertyDeserializer = MakePropertyDeserializer(property, candidateVRs);
                _propertyValueDeserializers.Add(dicomAttribute.Tag, propertyDeserializer);
            }
        }

        protected void Deserialize(object obj, DicomStreamReader reader)
        {
            foreach (var (tag, propertyDeserializer) in _propertyValueDeserializers)
            {
                if (reader.TrySeek(tag))
                {
                    propertyDeserializer.Invoke(obj, reader);
                    reader.EndReadValue();
                }
            }
        }

        private static readonly Dictionary<Type, DicomDeserializer> s_deserializers = new();

        public static DicomDeserializer<T> GetDeserializer<T>() where T : new()
        {
            DicomDeserializer deserializer;

            lock (s_deserializers)
            {
                if (!s_deserializers.TryGetValue(typeof(T), out deserializer))
                {
                    deserializer = new DicomDeserializer<T>();
                    s_deserializers[typeof(T)] = deserializer;
                }
            }

            return (DicomDeserializer<T>)deserializer; 
        }

        public static T Deserialize<T>(Stream stream, TransferSyntax transferSyntax) where T : new()
        {
            var deserializer = GetDeserializer<T>();
            var input = new BinaryStreamReader(stream, transferSyntax.ByteOrder);
            var reader = new DicomStreamReader(input, transferSyntax.VRCoding);
            return deserializer.Deserialize(reader);
        }
    }

    public class DicomDeserializer<T> : DicomDeserializer where T : new()
    {
        internal DicomDeserializer()
            : base(typeof(T))
        {
        }

        public T Deserialize(DicomStreamReader reader)
        {
            var obj = new T();
            Deserialize(obj, reader);
            return obj;
        }
    }
}
