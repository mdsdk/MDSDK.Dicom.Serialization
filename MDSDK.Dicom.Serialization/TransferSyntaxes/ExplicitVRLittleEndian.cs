// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;

namespace MDSDK.Dicom.Serialization.TransferSyntaxes
{
    public class ExplicitVRLittleEndian : TransferSyntax
    {
        internal ExplicitVRLittleEndian()
            : base(DicomUID.ExplicitVRLittleEndian, DicomVRCoding.Explicit, ByteOrder.LittleEndian)
        {
        }
    }
}
