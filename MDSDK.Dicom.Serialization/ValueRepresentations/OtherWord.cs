﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal sealed class OtherWord : OtherBinaryEncodedPrimitiveValue<ushort>
    {
        public OtherWord() : base("OW") { }
    }
}
