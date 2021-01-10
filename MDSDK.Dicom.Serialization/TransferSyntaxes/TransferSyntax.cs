// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;

namespace MDSDK.Dicom.Serialization.TransferSyntaxes
{
    public class TransferSyntax
    {
        public DicomUID DicomUID { get; }

        public DicomVRCoding VRCoding { get; }

        public ByteOrder ByteOrder { get; }
        
        protected TransferSyntax(DicomUID dicomUID, DicomVRCoding vrCoding, ByteOrder byteOrder)
        {
            DicomUID = dicomUID;
            VRCoding = vrCoding;
            ByteOrder = byteOrder;
        }

        public override string ToString() => DicomUID.ToString();
    }
}
