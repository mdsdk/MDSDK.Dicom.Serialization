// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class DateTime : AsciiEncodedMultiValue
    {
        public DateTime() : base("DT") { }
    }
}
