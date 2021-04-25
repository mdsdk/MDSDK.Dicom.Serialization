// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDSDK.Dicom.Serialization
{
    public class DicomStreamWriter
    {
        public BinaryStreamWriter Output { get; init; }

        public DicomVRCoding VRCoding { get; init; }

        public DicomStreamWriter(DicomVRCoding vrCoding, BinaryStreamWriter output)
        {
            VRCoding = vrCoding;
            Output = output;
        }

        private bool _specificCharacterSetWritten;

        public void WriteTag(DicomTag tag)
        {
            if (!_specificCharacterSetWritten && (tag >= DicomTag.SpecificCharacterSet))
            {
                if (tag == DicomTag.SpecificCharacterSet)
                {
                    throw new InvalidOperationException("SpecificCharacterSet cannot be set explicitly");
                }
                DicomTag.SpecificCharacterSet.WriteTo(Output);
                DicomVR.CS.WriteSingleValue(this, "ISO_IR 192");
                _specificCharacterSetWritten = true;
            }
            tag.WriteTo(Output);
        }

        public static long GetDataElementLength(ValueRepresentation vr, DicomVRCoding vrCoding, long unpaddedValueLength)
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

        public void WriteVRWithDefinedValueLength(ValueRepresentation vr, long unpaddedValueLength, out bool pad)
        {
            if (unpaddedValueLength >= UndefinedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 32-bit range");
            }

            pad = (unpaddedValueLength % 2) != 0;

            var valueLength = pad ? (uint)unpaddedValueLength + 1 : (uint)unpaddedValueLength;

            if (VRCoding == DicomVRCoding.Explicit)
            {
                Output.WriteByte(vr.Id.Item1);
                Output.WriteByte(vr.Id.Item2);
                if (vr is IHas16BitExplicitVRLength)
                {
                    if (valueLength >= ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 16-bit range");
                    }
                    Output.Write<UInt16>((ushort)valueLength);
                }
                else
                {
                    Output.WriteZeros(2);
                    Output.Write<UInt32>(valueLength);
                }
            }
            else
            {
                Output.Write<UInt32>(valueLength);
            }
        }

        private void WriteVRWithUndefinedValueLength(ValueRepresentation vr)
        {
            if (VRCoding == DicomVRCoding.Explicit)
            {
                Output.WriteByte(vr.Id.Item1);
                Output.WriteByte(vr.Id.Item2);
                Output.WriteZeros(2);
            }
            Output.Write<UInt32>(UndefinedLength);
        }

        private void WriteTagLength(DicomTag tag, uint length)
        {
            WriteTag(tag);
            Output.Write<uint>(length);
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
