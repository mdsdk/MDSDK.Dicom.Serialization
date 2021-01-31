﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class OtherLong : OtherBinaryEncodedPrimitiveValue<uint>
    {
        public OtherLong() : base("OL") { }
    }
}
