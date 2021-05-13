// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal sealed class IntegerString : AsciiEncodedMultiValue, IMultiValue<int>, IMultiValue<byte>
    {
        public IntegerString() : base("IS") { }

        int[] IMultiValue<int>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, int.Parse);
        
        int IMultiValue<int>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, int.Parse);

        void IMultiValue<int>.WriteValues(DicomStreamWriter writer, int[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);
        
        void IMultiValue<int>.WriteSingleValue(DicomStreamWriter writer, int value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);

        byte[] IMultiValue<byte>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, byte.Parse);

        byte IMultiValue<byte>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, byte.Parse);

        void IMultiValue<byte>.WriteValues(DicomStreamWriter writer, byte[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<byte>.WriteSingleValue(DicomStreamWriter writer, byte value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);
    }
}
