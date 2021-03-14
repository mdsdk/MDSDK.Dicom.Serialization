// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class UnsignedShort : BinaryEncodedPrimitiveValue<UInt16>, IHas16BitExplicitVRLength, IMultiValue<int>
    {
        public UnsignedShort() : base("US") { }

        int[] IMultiValue<int>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, o => (int)o);

        int IMultiValue<int>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, o => (int)o);

        void IMultiValue<int>.WriteValues(DicomStreamWriter writer, int[] values) => ConvertAndWriteValues(writer, o => (ushort)o, values);

        void IMultiValue<int>.WriteSingleValue(DicomStreamWriter writer, int value) => ConvertAndWriteSingleValue(writer, o => (ushort)o, value);

    }
}
