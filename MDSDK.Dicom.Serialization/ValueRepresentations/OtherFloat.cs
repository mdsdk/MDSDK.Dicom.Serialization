// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public sealed class OtherFloat : OtherBinaryEncodedPrimitiveValue<float>
    {
        internal OtherFloat() : base("OF") { }
    }
}
