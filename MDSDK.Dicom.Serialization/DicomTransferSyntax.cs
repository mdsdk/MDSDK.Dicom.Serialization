// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;

namespace MDSDK.Dicom.Serialization
{
    internal class DicomTransferSyntax : IEquatable<DicomTransferSyntax>
    {
        public DicomUID UID { get; }

        public DicomVRCoding VRCoding { get; }

        public ByteOrder ByteOrder { get; }

        public DicomTransferSyntax(DicomUID uid)
        {
            if (!uid.IsTransferSyntaxUID)
            {
                throw new ArgumentException("UID is not a DICOM transfer syntax UID");
            }
            UID = uid;
            VRCoding = (uid == DicomUID.TransferSyntax.ImplicitVRLittleEndian) ? DicomVRCoding.Implicit : DicomVRCoding.Explicit;
            ByteOrder = (uid == DicomUID.TransferSyntax.Retired.ExplicitVRBigEndian) ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
        }

        public bool Equals(DicomTransferSyntax other) => UID == other.UID;

        public override bool Equals(object obj) => (obj is DicomTransferSyntax other) && Equals(other);

        public override int GetHashCode() => UID.GetHashCode();

        public override string ToString() => UID.ToString();
    }
}
