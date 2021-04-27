// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.Dicom.Serialization.Internal;
using System;
using System.Text;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal abstract class SpecificCharsetEncodedValue : ValueRepresentation, IHasDefinedLengthOnly
    {
        internal SpecificCharsetEncodedValue(string vr) : base(vr) { }

        protected string ReadEntireValue(DicomStreamReader reader)
        {
            var valueLength = GetDefinedValueLength(reader);
            var bytes = reader.Input.ReadBytes(valueLength);
            reader.EndReadValue();
            var encoding = reader.EncodedStringDecoder ?? StringDecoder.Default;
            return encoding.Decode(this, TrimEnd(bytes));
        }

        protected static ReadOnlyMemory<byte> TrimEnd(byte[] bytes)
        {
            const byte Space = 0x20;
            const byte Null = 0x00;

            var n = bytes.Length;

            while ((n > 0) && ((bytes[n - 1] == Space) || (bytes[n - 1] == Null)))
            {
                n--;
            }

            return bytes.AsMemory(0, n);
        }

        internal override object GetValue(DicomStreamReader reader) => ReadEntireValue(reader);

        internal void WriteEntireValue(DicomStreamWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            writer.WriteVRWithDefinedValueLength(this, bytes.Length, out bool pad);
            writer.Output.WriteBytes(bytes);
            if (pad)
            {
                writer.Output.WriteByte((byte)' ');
            }
        }
    }
}