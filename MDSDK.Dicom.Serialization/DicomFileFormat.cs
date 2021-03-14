// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.TransferSyntaxes;
using System;
using System.IO;
using System.Linq;

namespace MDSDK.Dicom.Serialization
{
    static class DicomFileFormat
    {
        internal static readonly byte[] BeforeFileMetaInformationLength = new[]
        {
            (byte)'D',  (byte)'I',  (byte)'C', (byte)'M',   // "DICM" 
            (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, // FileMetaInformationGroupLength tag 
            (byte)'U',  (byte)'L',                          // VR
            (byte)0x04, (byte)0x00,                         // 16-bit length of value 
        };

        internal static readonly byte[] AfterFileMetaInformationLength = new[]
        {
            (byte)0x02, (byte)0x00, (byte)0x01, (byte)0x00, // FileMetaInformationVersion tag 
            (byte)'O',  (byte)'B',                          // VR
            (byte)0x00, (byte)0x00,                         // padding
            (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, // 32-bit length of value
            (byte)0x00, (byte)0x01,                         // actual value (0, 1)
        };

        internal static readonly DicomSerializer<DicomFileMetaInformation> FileMetaInformationSerializer
            = DicomSerializer.GetSerializer<DicomFileMetaInformation>();
    }
}