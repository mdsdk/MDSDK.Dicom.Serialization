// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Text;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal abstract class AsciiEncodedValue : ValueRepresentation, IHasDefinedLengthOnly, 
        IHasLightWeightValueLengthCalculation<string>
    {
        internal AsciiEncodedValue(string vr) : base(vr) { }

        internal string ReadEntireValue(DicomStreamReader reader)
        {
            var valueLength = GetDefinedValueLength(reader);
            var bytes = reader.Input.ReadBytes(valueLength);
            var significantBytes = StripNonSignificantBytes(bytes);
            var value = Encoding.ASCII.GetString(significantBytes);
            if (reader.CurrentTag == DicomTag.SpecificCharacterSet)
            {
                reader.ApplySpecfificCharacterSet(value.Split('\\'));
            }
            reader.EndReadValue();
            return value;
        }

        private static ReadOnlySpan<byte> StripNonSignificantBytes(byte[] bytes)
        {
            const byte Space = 0x20;
            const byte Null = 0x00;

            var start = 0;
            var end = bytes.Length;

            while ((start < end) && (bytes[start] == Space))
            {
                start++;
            }

            while ((end > start) && ((bytes[end - 1] == Space) || (bytes[end - 1] == Null)))
            {
                end--;
            }

            return bytes.AsSpan(start, end - start);
        }

        internal override object GetValue(DicomStreamReader reader) => ReadEntireValue(reader);

        internal void WriteEntireValue(DicomStreamWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            writer.WriteVRWithDefinedValueLength(this, bytes.Length, out bool pad);
            writer.Output.WriteBytes(bytes);
            if (pad)
            {
                writer.Output.WriteByte((byte)' ');
            }
        }

        long IHasLightWeightValueLengthCalculation<string>.GetUnpaddedValueLength(string value) => value.Length;

        long IHasLightWeightValueLengthCalculation<string>.GetUnpaddedValueLength(string[] values)
        {
            if (values.Length == 0)
            {
                return 0;
            }
            else
            {
                long unpaddedValueLength = values[0].Length;
                for (var i = 1; i < values.Length; i++)
                {
                    unpaddedValueLength++; // '\' separator
                    unpaddedValueLength += values[i].Length;
                }
                return unpaddedValueLength;
            }
        }
    }
}