﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal sealed class UnsignedLong : BinaryEncodedPrimitiveValue<UInt32>, IHas16BitExplicitVRLength
    {
        public UnsignedLong() : base("UL") { }
    }
}
