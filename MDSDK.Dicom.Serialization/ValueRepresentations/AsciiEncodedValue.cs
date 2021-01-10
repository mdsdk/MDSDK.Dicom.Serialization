// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.Text;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public abstract class AsciiEncodedValue : ValueRepresentation, IHasDefinedLengthOnly
    {
        internal AsciiEncodedValue(string vr) : base(vr) { }

        protected string ReadEntireValue(DicomStreamReader reader)
        {
            var valueLength = GetDefinedValueLength(reader);
            var bytes = reader.Input.ReadBytes(valueLength);
            var significantBytes = StripNonSignificantBytes(bytes);
            return Encoding.ASCII.GetString(significantBytes);
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

        public override string ToString(DicomStreamReader reader) => ReadEntireValue(reader);

        protected void WriteEntireValue(DicomStreamWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            writer.WriteVRLength(this, bytes.Length, out bool pad);
            writer.Output.WriteBytes(bytes);
            if (pad)
            {
                writer.Output.WriteByte((byte)' ');
            }
        }
    }
}