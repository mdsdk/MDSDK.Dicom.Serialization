// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class SignedShort : BinaryEncodedPrimitiveValue<Int16>, IHas16BitExplicitVRLength
    {
        public SignedShort() : base("SS") { }
    }
}
