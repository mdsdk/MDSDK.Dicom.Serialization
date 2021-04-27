// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.Buffers;
using System.IO;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>Provides methods for reading and writing DICOM files</summary>
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

        private static bool TryReadAll(BufferedStreamReader streamReader, Span<byte> bytes)
        {
            while (bytes.Length > 0)
            {
                var n = streamReader.ReadSome(bytes);
                if (n == 0)
                {
                    return false;
                }
                bytes = bytes.Slice(n);
            }
            return true;
        }

        /// <summary>Tries to read the DICOM file header from a stream</summary>
        public static bool TryReadHeader(BufferedStreamReader input, out DicomFileMetaInformation fileMetaInformation)
        {
            fileMetaInformation = null;

            Span<byte> header = stackalloc byte[128 + BeforeFileMetaInformationLength.Length + 4 + AfterFileMetaInformationLength.Length];

            if (!TryReadAll(input, header))
            {
                return false;
            }

            var before = header.Slice(128, BeforeFileMetaInformationLength.Length);
            var after = header.Slice(128 + BeforeFileMetaInformationLength.Length + 4, AfterFileMetaInformationLength.Length);

            if (!before.SequenceEqual(BeforeFileMetaInformationLength) || !after.SequenceEqual(AfterFileMetaInformationLength))
            {
                return false;
            }

            var metaInformationLength = BitConverter.ToUInt32(header.Slice(128 + BeforeFileMetaInformationLength.Length, 4));
            
            DicomFileMetaInformation tmp = null;

            input.Read((int)metaInformationLength - AfterFileMetaInformationLength.Length, () =>
            {
                var metaInformationReader = new DicomStreamReader(input, DicomTransferSyntax.ExplicitVRLittleEndian);
                tmp = FileMetaInformationSerializer.Deserialize(metaInformationReader);
            });

            fileMetaInformation = tmp;
            return true;
        }

        /// <summary>Writes a DICOM file header to a stream</summary>
        public static void WriteHeader(BufferedStreamWriter output, DicomFileMetaInformation fileMetaInformation)
        {
            output.WriteZeros(128);
            output.WriteBytes(BeforeFileMetaInformationLength);
            
            var fileMetaInformationLengthPosition = output.Position;

            output.WriteZeros(4);
            output.WriteBytes(AfterFileMetaInformationLength);

            var metaInformationWriter = new DicomStreamWriter(output, DicomTransferSyntax.ExplicitVRLittleEndian);
            FileMetaInformationSerializer.Serialize(metaInformationWriter, fileMetaInformation);

            var endOfFileMetaInformationPosition = output.Position;

            output.Flush(FlushMode.Shallow);

            var fileMetaInformationLength = endOfFileMetaInformationPosition - (fileMetaInformationLengthPosition + 4);
            output.Stream.Seek(fileMetaInformationLengthPosition, SeekOrigin.Begin);
            output.Stream.Write(BitConverter.GetBytes((uint)fileMetaInformationLength));
            output.Stream.Seek(endOfFileMetaInformationPosition, SeekOrigin.Begin);
        }
    }
}