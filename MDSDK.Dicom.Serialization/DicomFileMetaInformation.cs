// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization
{
    /// <summary>File meta information as defined in Part 7 of the DICOM standard</summary>
    public class DicomFileMetaInformation
    {
#pragma warning disable CS1591

        public string Media​StorageSOP​ClassUID { get; set; }

        public string Media​StorageSOP​InstanceUID { get; set; }

        public string Transfer​SyntaxUID { get; set; }

        public string ImplementationClassUID { get; set; }

        public string ImplementationVersionName { get; set; }

        public string SourceApplicationEntityTitle { get; set; }

        public string PrivateInformationCreatorUID { get; set; }

        public byte[] PrivateInformation { get; set; }

#pragma warning restore
    }
}
