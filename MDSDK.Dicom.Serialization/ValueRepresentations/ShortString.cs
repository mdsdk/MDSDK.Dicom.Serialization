// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class ShortString : SpecificCharsetEncodedMultiValue, IHas16BitExplicitVRLength
    {
        public ShortString() : base("SH") { }
    }
}
