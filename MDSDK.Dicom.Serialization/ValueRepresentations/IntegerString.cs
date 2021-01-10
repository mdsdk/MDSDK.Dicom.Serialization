// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public sealed class IntegerString : AsciiEncodedMultiValue, IMultiValue<int>
    {
        internal IntegerString() : base("IS") { }

        int[] IMultiValue<int>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, int.Parse);
        
        int IMultiValue<int>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, int.Parse);

        void IMultiValue<int>.WriteValues(DicomStreamWriter writer, int[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);
        
        void IMultiValue<int>.WriteSingleValue(DicomStreamWriter writer, int value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);
    }
}
