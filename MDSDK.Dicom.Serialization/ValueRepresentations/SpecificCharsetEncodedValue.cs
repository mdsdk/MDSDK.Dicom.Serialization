// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Text;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public abstract class SpecificCharsetEncodedValue : ValueRepresentation, IHasDefinedLengthOnly
    {
        internal SpecificCharsetEncodedValue(string vr) : base(vr) { }

        protected string ReadEntireValue(DicomStreamReader reader)
        {
            var valueLength = GetDefinedValueLength(reader);
            var bytes = reader.Input.ReadBytes(valueLength);
            reader.EndReadValue();
            var significantBytes = StripNonSignificantBytes(bytes);
            var encoding = reader.SpecificCharsetEncoding ?? Encoding.ASCII;
            return encoding.GetString(significantBytes);
        }

        protected static ReadOnlySpan<byte> StripNonSignificantBytes(byte[] bytes)
        {
            const byte Space = 0x20;
            const byte Null = 0x00;

            var end = bytes.Length;

            while ((end > 0) && ((bytes[end - 1] == Space) || (bytes[end - 1] == Null)))
            {
                end--;
            }

            return bytes.AsSpan(0, end);
        }

        internal override string ToString(DicomStreamReader reader) => ReadEntireValue(reader);

        internal void WriteEntireValue(DicomStreamWriter writer, string value)
        {
            var encoding = writer.SpecificCharsetEncoding ?? Encoding.ASCII;
            var bytes = encoding.GetBytes(value);
            writer.WriteVRLength(this, bytes.Length, out bool pad);
            writer.Output.WriteBytes(bytes);
            if (pad)
            {
                writer.Output.WriteByte((byte)' ');
            }
        }
    }
}