// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.IO;

namespace MDSDK.Dicom.Serialization
{
    public static class DicomFileFormat
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

        public static void WriteHeader(Stream stream, DicomFileMetaInformation fileMetaInformation)
        {
            var output = new BinaryStreamWriter(ByteOrder.LittleEndian, stream);
            
            output.WriteZeros(128);
            output.WriteBytes(BeforeFileMetaInformationLength);

            var fileMetaInformationLengthPosition = output.Position;
            
            output.WriteZeros(4);
            output.WriteBytes(AfterFileMetaInformationLength);
            
            var headerWriter = new DicomStreamWriter(DicomVRCoding.Explicit, output);
            FileMetaInformationSerializer.Serialize(headerWriter, fileMetaInformation);
            
            var endOfFileMetaInformationPosition = output.Position;
            
            output.Flush(FlushMode.Shallow);

            var fileMetaInformationLength = endOfFileMetaInformationPosition - (fileMetaInformationLengthPosition + 4);
            stream.Seek(fileMetaInformationLengthPosition, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((uint)fileMetaInformationLength));
            stream.Seek(endOfFileMetaInformationPosition, SeekOrigin.Begin);
        }
    }
}