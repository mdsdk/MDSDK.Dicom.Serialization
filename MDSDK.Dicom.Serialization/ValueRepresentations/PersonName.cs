﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal sealed class PersonName : SpecificCharsetEncodedMultiValue, IHas16BitExplicitVRLength
    {
        public PersonName() : base("PN") { }
    }
}
