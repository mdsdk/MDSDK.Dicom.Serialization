// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.IO;
using System.Linq;
using static MDSDK.Dicom.Serialization.DicomFileFormat;

namespace MDSDK.Dicom.Serialization
{
    public class DicomFileReader
    {
        public DicomFileMetaInformation FileMetaInformation { get; private set; }

        public DicomUID MediaStorageSOPClassUID { get; private set; }

        public DicomTransferSyntax TransferSyntax { get; private set; }

        public DicomStreamReader DataSetReader { get; private set; }

        private DicomFileReader(Stream stream, out bool isValid)
        {
            Span<byte> header = stackalloc byte[128 + BeforeFileMetaInformationLength.Length + 4 + AfterFileMetaInformationLength.Length];

            for (var i = 0; i < header.Length;)
            {
                var n = stream.Read(header.Slice(i));
                if (n == 0)
                {
                    isValid = false;
                    return;
                }
                i += n;
            }

            var before = header.Slice(128, BeforeFileMetaInformationLength.Length);
            var after = header.Slice(128 + BeforeFileMetaInformationLength.Length + 4, AfterFileMetaInformationLength.Length);

            if (!before.SequenceEqual(BeforeFileMetaInformationLength) || !after.SequenceEqual(AfterFileMetaInformationLength))
            {
                isValid = false;
                return;
            }

            var metaInformationLength = BitConverter.ToUInt32(header.Slice(128 + BeforeFileMetaInformationLength.Length, 4));

            var input = new BinaryStreamReader(ByteOrder.LittleEndian, stream);
            input.Read(metaInformationLength - AfterFileMetaInformationLength.Length, () =>
            {
                var metaInformationReader = new DicomStreamReader(DicomVRCoding.Explicit, input);
                FileMetaInformation = FileMetaInformationSerializer.Deserialize(metaInformationReader);
            });

            MediaStorageSOPClassUID = FileMetaInformation.MediaStorageSOPClassUID;

            TransferSyntax = new DicomTransferSyntax(FileMetaInformation.TransferSyntaxUID);
            input.ByteOrder = TransferSyntax.ByteOrder;
            DataSetReader = new DicomStreamReader(TransferSyntax.VRCoding, input);

            isValid = true;
        }

        public static bool TryCreate(Stream stream, out DicomFileReader reader)
        {
            var dicomFileReader = new DicomFileReader(stream, out bool isValid);
            reader = isValid ? dicomFileReader : null;
            return isValid;
        }
    }
}