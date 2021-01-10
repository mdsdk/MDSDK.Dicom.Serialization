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
        private readonly SortedList<DicomTag, Action<object, DicomStreamWriter>> _propertyValueSerializers = new();

        private static Action<object, DicomStreamWriter> MakePropertySerializer(PropertyInfo property,
            IEnumerable<ValueRepresentation> candidateVRs)
        {
            ValueRepresentation theVR = null;
            MethodInfo theWriteValueMethod = null;

            foreach (var vr in candidateVRs)
            {
                var vrType = vr.GetType();

                var singleValueInterface = typeof(ISingleValue<>).MakeGenericType(property.PropertyType);
                var multiValueInterface = typeof(IMultiValue<>).MakeGenericType(property.PropertyType);

                MethodInfo writeValueMethod = null;

                if (singleValueInterface.IsAssignableFrom(vrType))
                {
                    writeValueMethod = singleValueInterface.GetMethod(nameof(ISingleValue<object>.WriteValue));
                }
                else if (multiValueInterface.IsAssignableFrom(vrType))
                {
                    writeValueMethod = property.PropertyType.IsArray
                        ? multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteValues))
                        : multiValueInterface.GetMethod(nameof(IMultiValue<object>.WriteSingleValue));
                }

                if ((writeValueMethod == null) || !writeValueMethod.GetParameters()[1].ParameterType.IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                if (theWriteValueMethod == null)
                {
                    theVR = vr;
                    theWriteValueMethod = writeValueMethod;
                }
                else if (writeValueMethod != theWriteValueMethod)
                {
                    throw new Exception($"Mutiple methods found for serializing {property}: {theWriteValueMethod} and {writeValueMethod}");
                }
            }

            if (theWriteValueMethod == null)
            {
                throw new Exception($"No matching VR found for serializing {property}");
            }

            void SerializePropertyValue(object obj, DicomStreamWriter writer)
            {
                try
                {
                    var value = property.GetValue(obj);
                    theWriteValueMethod.Invoke(theVR, new object[] { writer, value });
                }
                catch (Exception error)
                {
                    var rootCause = error.GetBaseException();
                    throw new Exception($"Error serializing {property} using VR {theVR}: {rootCause.Message}", rootCause);
                }
            }

            return SerializePropertyValue;
        }

        private DicomSerializer(Type type)
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
                _propertyValueSerializers.Add(dicomAttribute.Tag, propertySerializer);
            }
        }

        public void Serialize(DicomStreamWriter output, object obj)
        {
            foreach (var (tag, propertySerializer) in _propertyValueSerializers)
            {
                output.WriteTag(tag);
                propertySerializer.Invoke(obj, output);
            }
        }

        private static readonly Dictionary<Type, DicomSerializer> s_serializers = new();

        public static DicomSerializer GetSerializer(Type type) 
        {
            lock (s_serializers)
            {
                if (!s_serializers.TryGetValue(type, out DicomSerializer serializer))
                {
                    serializer = new DicomSerializer(type);
                    s_serializers[type] = serializer;
                }
                return serializer;
            }
        }

        public static void Serialize<T>(Stream stream, TransferSyntax transferSyntax, T obj) 
        {
            var serializer = GetSerializer(typeof(T));
            var output = new BinaryStreamWriter(stream, transferSyntax.ByteOrder);
            var writer = new DicomStreamWriter(output, transferSyntax.VRCoding);
            serializer.Serialize(writer, obj);
        }
    }
}
