// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Text;

namespace MDSDK.Dicom.Serialization
{
    public class DicomStreamWriter
    {
        public BinaryStreamWriter Output { get; init; }

        public DicomVRCoding VRCoding { get; init; }

        public DicomStreamWriter(BinaryStreamWriter output, DicomVRCoding vrCoding)
        {
            Output = output;
            VRCoding = vrCoding;
        }

        public Encoding SpecificCharsetEncoding { get; private set; }

        private DicomStreamWriter CreateNestedWriter()
        {
            return new DicomStreamWriter(Output, VRCoding)
            {
                SpecificCharsetEncoding = SpecificCharsetEncoding
            };
        }

        public void WriteTag(DicomTag tag)
        {
            tag.WriteTo(Output);
        }

        public static long GetDataElementLength(ValueRepresentation vr, DicomVRCoding vrCoding, long unpaddedValueLength)
        {
            if (unpaddedValueLength >= uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 32-bit range");
            }

            var valueLength = ((unpaddedValueLength % 2) != 0) ? (uint)unpaddedValueLength + 1: (uint)unpaddedValueLength;

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

        public void WriteVRLength(ValueRepresentation vr, long unpaddedValueLength, out bool pad)
        {
            if (unpaddedValueLength >= uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(unpaddedValueLength), $"{unpaddedValueLength} is out of 32-bit range");
            }

            pad = (unpaddedValueLength % 2) != 0;
            
            var valueLength = pad ? (uint)unpaddedValueLength + 1: (uint)unpaddedValueLength;

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
    }
}
