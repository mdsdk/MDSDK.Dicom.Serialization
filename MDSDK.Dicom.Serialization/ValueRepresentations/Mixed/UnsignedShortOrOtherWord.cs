// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations.Mixed
{
    public class UnsignedShortOrOtherWord : OtherBinaryEncodedPrimitiveValue<ushort>, IMixedValueRepresentation
    {
        public UnsignedShortOrOtherWord() : base("US or OW") { }

        public ValueRepresentation DefaultVR => DicomVR.OW;
    }
}
