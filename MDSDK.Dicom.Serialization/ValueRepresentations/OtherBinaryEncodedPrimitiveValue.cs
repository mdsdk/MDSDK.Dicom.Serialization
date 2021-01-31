// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class OtherBinaryEncodedPrimitiveValue<T> : BinaryEncodedPrimitiveValueBase<T>, ISingleValue<T[]>, IHas32BitExplicitVRLength, IMayHaveUndefinedLength 
        where T : unmanaged, IFormattable
    {
        internal OtherBinaryEncodedPrimitiveValue(string vr) : base(vr) { }

        public virtual T[] ReadValue(DicomStreamReader reader)
        {
            if (reader.ValueLength != UndefinedLength)
            {
                return ReadArray(reader);
            }
            else
            {
                throw new NotSupportedException($"Multi-chunk value reading not implemented by {this}");
            }
        }

        public virtual void WriteValue(DicomStreamWriter writer, T[] value) => WriteArray(writer, value);
    }
}
