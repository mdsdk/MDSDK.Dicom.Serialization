// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class SignedVeryLong : BinaryEncodedPrimitiveValue<Int64>, IHas32BitExplicitVRLength
    {
        public SignedVeryLong() : base("SV") { }
    }
}
