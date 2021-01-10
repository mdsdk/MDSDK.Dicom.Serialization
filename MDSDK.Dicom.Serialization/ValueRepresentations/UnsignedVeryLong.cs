// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public sealed class UnsignedVeryLong : BinaryEncodedPrimitiveValue<UInt64>, IHas32BitExplicitVRLength
    {
        internal UnsignedVeryLong() : base("UV") { }
    }
}
