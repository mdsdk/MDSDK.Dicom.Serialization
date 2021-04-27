// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal class BinaryEncodedPrimitiveValue<T> : BinaryEncodedPrimitiveValueBase<T>, IMultiValue<T>, IHasDefinedLengthOnly 
        where T : unmanaged, IFormattable
    {
        internal BinaryEncodedPrimitiveValue(string vr) : base(vr) { }

        public T[] ReadValues(DicomStreamReader reader) => ReadArray(reader);

        public T ReadSingleValue(DicomStreamReader reader)
        {
            EnsureSingleValue<T>(reader);
            var result = reader.DataReader.Read<T>();
            reader.EndReadValue();
            return result;
        }

        internal T2[] ReadAndConvertValues<T2>(DicomStreamReader reader, Func<T, T2> convert)
        {
            var values = ReadValues(reader);
            var convertedValues = new T2[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                convertedValues[i] = convert(values[i]);
            }
            return convertedValues;
        }

        internal T2 ReadAndConvertSingleValue<T2>(DicomStreamReader reader, Func<T, T2> convert)
        {
            var value = ReadSingleValue(reader);
            return convert(value);
        }

        internal override object GetValue(DicomStreamReader reader) => GetValue<T>(this, reader);

        public void WriteValues(DicomStreamWriter writer, T[] values) => WriteArray(writer, values);

        public void WriteSingleValue(DicomStreamWriter writer, T value)
        {
            writer.WriteVRWithDefinedValueLength(this, Unsafe.SizeOf<T>(), out _);
            writer.DataWriter.Write(value);
        }

        internal void ConvertAndWriteValues<T2>(DicomStreamWriter writer, Func<T2, T> convert, T2[] values)
        {
            var convertedValues = new T[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                convertedValues[i] = convert(values[i]);
            }
            WriteValues(writer, convertedValues);
        }

        protected void ConvertAndWriteSingleValue<T2>(DicomStreamWriter writer, Func<T2, T> convert, T2 value)
        {
            var convertedValue = convert(value);
            WriteSingleValue(writer, convertedValue);
        }
    }
}
