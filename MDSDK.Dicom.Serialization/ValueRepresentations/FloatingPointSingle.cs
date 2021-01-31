// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class FloatingPointSingle : BinaryEncodedPrimitiveValue<Single>, IHas16BitExplicitVRLength
    {
        public FloatingPointSingle() : base("FL") { }
    }
}
