// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization
{
    /// <summary>A type that indicates whether DICOM data elements are coded with or without VR</summary>
    public enum DicomVRCoding
    {
        /// <summary>DICOM data elements are coded without VR</summary>
        Implicit,

        /// <summary>DICOM data elements are coded with VR</summary>
        Explicit
    }
}
