// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.TransferSyntaxes;
using System;
using System.IO;
using System.Linq;

namespace MDSDK.Dicom.Serialization
{
    public class DicomFile
    {
        public DicomFileMetaInformation MetaInformation { get; private set; }

        public DicomUID SOPClassUID { get; private set; }

        public TransferSyntax TransferSyntax { get; private set; }

        private static readonly byte[] BeforeFileMetaInformationLength = new[]
        {
            (byte)'D',  (byte)'I',  (byte)'C', (byte)'M',   // "DICM" 
            (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, // FileMetaInformationGroupLength tag 
            (byte)'U',  (byte)'L',                          // VR
            (byte)0x04, (byte)0x00,                         // 16-bit length of value 
        };

        private static readonly byte[] AfterFileMetaInformationLength = new[]
        {
            (byte)0x02, (byte)0x00, (byte)0x01, (byte)0x00, // FileMetaInformationVersion tag 
            (byte)'O',  (byte)'B',                          // VR
            (byte)0x00, (byte)0x00,                         // padding
            (byte)0x02, (byte)0x00, (byte)0x00, (byte)0x00, // 32-bit length of value
            (byte)0x00, (byte)0x01,                         // actual value (0, 1)
        };

        private static readonly DicomSerializer<DicomFileMetaInformation> MetaInformationSerializer
            = DicomSerializer.GetSerializer<DicomFileMetaInformation>();

        public bool CanRead(Stream stream, out DicomStreamReader dataSetReader)
        {
            Span<byte> header = stackalloc byte[128 + BeforeFileMetaInformationLength.Length + 4 + AfterFileMetaInformationLength.Length];

            for (var i = 0; i < header.Length;)
            {
                var n = stream.Read(header.Slice(i));
                if (n == 0)
                {
                    dataSetReader = null;
                    return false;
                }
                i += n;
            }

            var before = header.Slice(128, BeforeFileMetaInformationLength.Length);
            var after = header.Slice(128 + BeforeFileMetaInformationLength.Length + 4, AfterFileMetaInformationLength.Length);

            if (!before.SequenceEqual(BeforeFileMetaInformationLength) || !after.SequenceEqual(AfterFileMetaInformationLength))
            {
                dataSetReader = null;
                return false;
            }

            var metaInformationLength = BitConverter.ToUInt32(header.Slice(128 + BeforeFileMetaInformationLength.Length, 4));

            var input = new BinaryStreamReader(stream, ByteOrder.LittleEndian);
            input.Read(metaInformationLength - AfterFileMetaInformationLength.Length, () =>
            {
                var metaInformationReader = new DicomStreamReader(input, DicomVRCoding.Explicit);
                MetaInformation = MetaInformationSerializer.Deserialize(metaInformationReader);
            });

            if (DicomUID.TryLookup(MetaInformation.MediaStorageSOPClassUID, out DicomUID sopClassUID))
            {
                SOPClassUID = sopClassUID;
            }

            if (DicomTransferSyntax.TryLookup(MetaInformation.TransferSyntaxUID, out TransferSyntax transferSyntax))
            {
                TransferSyntax = transferSyntax;
                input.ByteOrder = transferSyntax.ByteOrder;
                dataSetReader = new DicomStreamReader(input, transferSyntax.VRCoding);
            }
            else
            {
                input.ByteOrder = ByteOrder.LittleEndian;
                dataSetReader = new DicomStreamReader(input, DicomVRCoding.Explicit);
            }

            return true;
        }
    }
}