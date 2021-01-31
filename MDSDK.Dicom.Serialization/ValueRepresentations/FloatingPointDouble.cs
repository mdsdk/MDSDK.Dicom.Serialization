// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class FloatingPointDouble : BinaryEncodedPrimitiveValue<Double>, IHas16BitExplicitVRLength
    {
        public FloatingPointDouble() : base("FD") { }
    }
}
