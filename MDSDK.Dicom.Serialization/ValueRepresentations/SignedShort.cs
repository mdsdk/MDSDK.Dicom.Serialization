// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public sealed class SignedShort : BinaryEncodedPrimitiveValue<Int16>, IHas16BitExplicitVRLength
    {
        internal SignedShort() : base("SS") { }
    }
}
