// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.Linq;
using System.Globalization;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class AsciiEncodedMultiValue : AsciiEncodedValue, IMultiValue<string>, IHas16BitExplicitVRLength
    {
        internal AsciiEncodedMultiValue(string vr) : base(vr) { }

        public string[] ReadValues(DicomStreamReader reader)
        {
            var entireValue = ReadEntireValue(reader);
            return (entireValue.Length == 0) ? Array.Empty<string>() : entireValue.Split('\\');
        }

        public string ReadSingleValue(DicomStreamReader reader) => ReadEntireValue(reader);

        internal T[] ReadAndConvertValues<T>(DicomStreamReader reader, Func<string, NumberFormatInfo, T> convert)
        {
            var stringValues = ReadValues(reader);
            var values = new T[stringValues.Length];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = convert(stringValues[i], NumberFormatInfo.InvariantInfo);
            }
            return values;
        }

        internal T ReadAndConvertSingleValue<T>(DicomStreamReader reader, Func<string, NumberFormatInfo, T> convert)
        {
            var stringValue = ReadSingleValue(reader);
            return convert(stringValue, NumberFormatInfo.InvariantInfo);
        }

        public void WriteValues(DicomStreamWriter writer, string[] values)
        {
            if (values.Any(value => value.Contains('\\')))
            {
                throw new ArgumentException("value may not contain \\", nameof(values));
            }
            WriteEntireValue(writer, string.Join('\\', values));
        }

        public void WriteSingleValue(DicomStreamWriter writer, string value) => WriteEntireValue(writer, value);

        internal void ConvertAndWriteValues<T>(DicomStreamWriter writer, Func<T, NumberFormatInfo, string> convert, T[] values)
        {
            var stringValues = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                stringValues[i] = convert(values[i], NumberFormatInfo.InvariantInfo);
            }
            WriteValues(writer, stringValues);
        }

        internal void ConvertAndWriteSingleValue<T>(DicomStreamWriter writer, Func<T, NumberFormatInfo, string> convert, T value)
        {
            var stringValue = convert(value, NumberFormatInfo.InvariantInfo);
            WriteSingleValue(writer, stringValue);
        }
    }
}
