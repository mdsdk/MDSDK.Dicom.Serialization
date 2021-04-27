// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations.Mixed
{
    internal sealed class UnsignedShortOrOtherWord : OtherBinaryEncodedPrimitiveValue<ushort>, IMixedValueRepresentation
    {
        public UnsignedShortOrOtherWord() : base("US or OW") { }

        public ValueRepresentation DefaultVR => DicomVR.OW;
    }
}
