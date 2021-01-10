// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class UniversalResourceIdentifier : AsciiEncodedSingleValue, IHas32BitExplicitVRLength
    {
        public UniversalResourceIdentifier() : base("UR") { }
    }
}
