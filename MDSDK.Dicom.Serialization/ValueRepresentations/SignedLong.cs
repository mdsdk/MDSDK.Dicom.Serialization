// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class SignedLong : BinaryEncodedPrimitiveValue<Int32>, IHas16BitExplicitVRLength
    {
        public SignedLong() : base("SL") { }
    }
}
