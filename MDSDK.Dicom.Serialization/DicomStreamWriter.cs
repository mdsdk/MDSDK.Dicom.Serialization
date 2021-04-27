// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>Provides methods for reading DICOM data elements to a stream using a given transfer syntax</summary>
    public class DicomStreamWriter
    {
        /// <summary>Creates a DicomStreamWriter for writing DICOM data elements to a stream using the given transfer syntax</summary>
        public static DicomStreamWriter Create(BufferedStreamWriter output, DicomUID transferSyntaxUID)
        {
            var transferSyntax = new DicomTransferSyntax(transferSyntaxUID);
            return new DicomStreamWriter(output, transferSyntax);
        }

        /// <summary>The stream to which the DicomStreamWriter writes the DICOM data elements</summary>
        public BufferedStreamWriter Output { get; }

        internal DicomTransferSyntax TransferSyntax { get; }

        internal BinaryDataWriter DataWriter { get; }

        /// <summary>The VR coding used to write DICOM data elements</summary>
        public DicomVRCoding VRCoding { get; }

        internal DicomStreamWriter(BufferedStreamWriter output, DicomTransferSyntax transferSyntax)
        {
            Output = output;
            TransferSyntax = transferSyntax;
            DataWriter = new BinaryDataWriter(output, transferSyntax.ByteOrder);
            VRCoding = transferSyntax.VRCoding;
        }

        private bool _specificCharacterSetWritten;

        internal void WriteTag(DicomTag tag)
        {
            if (!_specificCharacterSetWritten && (tag >= DicomTag.SpecificCharacterSet))
            {
                if (tag == DicomTag.SpecificCharacterSet)
                {
                    throw new InvalidOperationException("SpecificCharacterSet cannot be set explicitly");
                }
                DicomTag.SpecificCharacterSet.WriteTo(DataWriter);
                DicomVR.CS.WriteSingleValue(this, "ISO_IR 192");
                _specificCharacterSetWritten = true;
            }
            tag.WriteTo(DataWriter);
        }

        internal static long GetDataElementLength(ValueRepresentation vr, DicomVRCoding vrCoding, long unpaddedValueLength)
        {
            if (unpaddedValueLength >= uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 32-bit range");
            }

            var valueLength = ((unpaddedValueLength % 2) != 0) ? (uint)unpaddedValueLength + 1 : (uint)unpaddedValueLength;

            long dataElementLength = 4;

            if (vrCoding == DicomVRCoding.Explicit)
            {
                dataElementLength += 2;
                if (vr is IHas16BitExplicitVRLength)
                {
                    if (valueLength >= ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 16-bit range");
                    }
                    dataElementLength += 2;
                }
                else
                {
                    dataElementLength += 6;
                }
            }
            else
            {
                dataElementLength += 4;
            }

            dataElementLength += valueLength;

            return dataElementLength;
        }

        private const uint UndefinedLength = uint.MaxValue;

        private const uint ZeroLength = 0;

        internal void WriteVRWithDefinedValueLength(ValueRepresentation vr, long unpaddedValueLength, out bool pad)
        {
            if (unpaddedValueLength >= UndefinedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 32-bit range");
            }

            pad = (unpaddedValueLength % 2) != 0;

            var valueLength = pad ? (uint)unpaddedValueLength + 1 : (uint)unpaddedValueLength;

            if (VRCoding == DicomVRCoding.Explicit)
            {
                DataWriter.Write(vr.Id.Item1);
                DataWriter.Write(vr.Id.Item2);
                if (vr is IHas16BitExplicitVRLength)
                {
                    if (valueLength >= ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 16-bit range");
                    }
                    DataWriter.Write((ushort)valueLength);
                }
                else
                {
                    DataWriter.WriteZeros(2);
                    DataWriter.Write(valueLength);
                }
            }
            else
            {
                DataWriter.Write(valueLength);
            }
        }

        private void WriteVRWithUndefinedValueLength(ValueRepresentation vr)
        {
            if (VRCoding == DicomVRCoding.Explicit)
            {
                DataWriter.Write(vr.Id.Item1);
                DataWriter.Write(vr.Id.Item2);
                DataWriter.WriteZeros(2);
            }
            DataWriter.Write(UndefinedLength);
        }

        private void WriteTagLength(DicomTag tag, uint length)
        {
            Debug.Assert(!tag.HasVR);
            WriteTag(tag);
            DataWriter.Write(length);
        }

        internal void WriteSequenceItems<T>(Action<DicomStreamWriter, T> itemSerializer, IEnumerator<T> itemEnumerator)
        {
            WriteVRWithUndefinedValueLength(DicomVR.SQ);
            while (itemEnumerator.MoveNext())
            {
                WriteTagLength(DicomTag.Item, UndefinedLength);
                itemSerializer.Invoke(this, itemEnumerator.Current);
                WriteTagLength(DicomTag.ItemDelimitationItem, ZeroLength);
            }
            WriteTagLength(DicomTag.SequenceDelimitationItem, ZeroLength);
        }
    }
}
