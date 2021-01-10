// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;

namespace MDSDK.Dicom.Serialization.TransferSyntaxes
{
    public class ImplicitVRLittleEndian : TransferSyntax
    {
        internal ImplicitVRLittleEndian()
            : base(DicomUID.ImplicitVRLittleEndian, DicomVRCoding.Implicit, ByteOrder.LittleEndian)
        {
        }
    }
}
