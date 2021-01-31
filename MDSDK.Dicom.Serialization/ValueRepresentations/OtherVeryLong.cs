// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class OtherVeryLong : OtherBinaryEncodedPrimitiveValue<ulong>
    {
        public OtherVeryLong() : base("OV") { }
    }
}
