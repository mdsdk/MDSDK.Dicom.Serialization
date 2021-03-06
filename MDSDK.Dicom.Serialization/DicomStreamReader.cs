﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.Internal;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>Provides methods for reading DICOM data elements from a stream using a given transfer syntax</summary>
    public class DicomStreamReader
    {
        /// <summary>Creates a DicomStreamReader for reading DICOM data elements from a stream using the given transfer syntax</summary>
        public static DicomStreamReader Create(BufferedStreamReader input, DicomUID transferSyntaxUID)
        {
            var transferSyntax = new DicomTransferSyntax(transferSyntaxUID);
            return new DicomStreamReader(input, transferSyntax);
        }

#pragma warning disable 1591

        public BufferedStreamReader Input { get; }

#pragma warning restore 1591

        internal DicomTransferSyntax TransferSyntax { get; }

        internal BinaryDataReader DataReader { get; }

        internal DicomVRCoding VRCoding { get; }

        internal DicomStreamReader(BufferedStreamReader input, DicomTransferSyntax transferSyntax)
        {
            Input = input;
            TransferSyntax = transferSyntax;
            DataReader = new BinaryDataReader(input, transferSyntax.ByteOrder);
            VRCoding = transferSyntax.VRCoding;
        }

        internal StringDecoder EncodedStringDecoder { get; private set; }

        private DicomStreamReader CreateNestedReader()
        {
            return new DicomStreamReader(Input, TransferSyntax)
            {
                EncodedStringDecoder = EncodedStringDecoder
            };
        }

        private const uint UndefinedLength = uint.MaxValue;

        internal DicomTag CurrentTag { get; private set; } = DicomTag.Undefined;

        internal ValueRepresentation ExplicitVR { get; private set; }

        internal uint ValueLength { get; private set; }

        private void ReadTagVRLength()
        {
            if (CurrentTag != DicomTag.Undefined)
            {
                throw new InvalidOperationException($"Reading of {CurrentTag} not completed");
            }

            CurrentTag = DicomTag.ReadFrom(DataReader);

            if (CurrentTag.GroupNumber == 0xFFFF)
            {
                throw new IOException($"Invalid DICOM tag {CurrentTag} in input"); // See part 5, section 7.5.1 Item Encoding Rules
            }

            if (CurrentTag.HasVR && (VRCoding == DicomVRCoding.Explicit))
            {
                DataReader.Read(out byte b0);
                DataReader.Read(out byte b1);
                var id = new ValueTuple<byte, byte>(b0, b1);
                ExplicitVR = DicomVR.Lookup(id);
                if (ExplicitVR is IHas16BitExplicitVRLength)
                {
                    ValueLength = DataReader.Read<UInt16>();
                }
                else
                {
                    Input.SkipBytes(2);
                    ValueLength = DataReader.Read<UInt32>();
                }
            }
            else
            {
                ValueLength = DataReader.Read<UInt32>();
                if (CurrentTag.IsDelimitationItem && (ValueLength != 0))
                {
                    throw new IOException($"Expected 0 length for {CurrentTag} but got {ValueLength}");
                }
            }
        }

        internal bool TrySeek(DicomTag tag)
        {
            while (true)
            {
                if (CurrentTag == DicomTag.Undefined)
                {
                    if (Input.AtEnd)
                    {
                        return false;
                    }
                    ReadTagVRLength();
                }
                if (tag < CurrentTag)
                {
                    return false;
                }
                if (tag == CurrentTag)
                {
                    return true;
                }
                SkipValue();
                Debug.Assert(CurrentTag == DicomTag.Undefined);
            }
        }

        internal void EndReadValue()
        {
            CurrentTag = DicomTag.Undefined;
        }

        private void SkipValueWithUndefinedLength()
        {
            var sequenceReader = CreateNestedReader();
            sequenceReader.SkipItemsOfSequenceWithUndefinedLength();
            CurrentTag = DicomTag.Undefined;
        }

        private void SkipValueWithDefinedLength()
        {
            Input.SkipBytes(ValueLength);
            CurrentTag = DicomTag.Undefined;
        }

        internal void ApplySpecfificCharacterSet(string[] values)
        {
            EncodedStringDecoder = StringDecoder.Get(values);
        }

        private static bool IsLegacyGroupLengthTag(DicomTag tag) => tag.ElementNumber == 0x0000;

        private void SkipValue()
        {
            if (CurrentTag == DicomTag.SpecificCharacterSet)
            {
                var values = DicomVR.CS.ReadValues(this);
                ApplySpecfificCharacterSet(values);
            }
            else if ((CurrentTag.GroupNumber == DicomTag.PixelDataGroupNumber) && !IsLegacyGroupLengthTag(CurrentTag))
            {
                throw new IOException($"Value of {CurrentTag} tag in pixel data group must be processed and may not be skipped");
            }
            else if (ValueLength == UndefinedLength)
            {
                SkipValueWithUndefinedLength();
            }
            else
            {
                SkipValueWithDefinedLength();
            }
            Debug.Assert(CurrentTag == DicomTag.Undefined);
        }

        internal void SkipToDelimiter(DicomTag delimitationItemTag)
        {
            if (!TrySeek(delimitationItemTag))
            {
                throw new IOException($"Missing terminating {delimitationItemTag}");
            }
            CurrentTag = DicomTag.Undefined;
        }

        internal bool TryReadItemTagOfSequenceWithUndefinedLength()
        {
            ReadTagVRLength();
            if (CurrentTag == DicomTag.Item)
            {
                return true;
            }
            else if (CurrentTag == DicomTag.SequenceDelimitationItem)
            {
                return false;
            }
            else
            {
                throw new IOException($"Expected {DicomTag.Item} or {DicomTag.SequenceDelimitationItem} but got {CurrentTag}");
            }
        }

        private bool TryReadItemTagOfSequenceWithDefinedLength()
        {
            if (!Input.AtEnd)
            {
                ReadTagVRLength();
                if (CurrentTag != DicomTag.Item)
                {
                    throw new IOException($"Expected {DicomTag.Item} but got {CurrentTag}");
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SkipItemsOfSequenceWithUndefinedLength()
        {
            while (TryReadItemTagOfSequenceWithUndefinedLength())
            {
                if (ValueLength == UndefinedLength)
                {
                    var itemReader = CreateNestedReader();
                    itemReader.SkipToDelimiter(DicomTag.ItemDelimitationItem);
                    CurrentTag = DicomTag.Undefined;
                }
                else
                {
                    SkipValueWithDefinedLength();
                }
            }
        }

        private T ReadItemValue<T>(Func<DicomStreamReader, T> itemDeserializer)
        {
            T item;
            var itemReader = CreateNestedReader();
            if (ValueLength == UndefinedLength)
            {
                item = itemDeserializer.Invoke(itemReader);
                itemReader.SkipToDelimiter(DicomTag.ItemDelimitationItem);
            }
            else
            {
                item = Input.Read<T>(ValueLength, () =>
                {
                    var result = itemDeserializer.Invoke(itemReader);
                    Input.SkipRemainingBytes();
                    return result;
                });
            }
            CurrentTag = DicomTag.Undefined;
            return item;
        }

        internal void ReadSequenceItems<T>(Func<DicomStreamReader, T> itemDeserializer, Action<T> handleItem)
        {
            if (ValueLength == UndefinedLength)
            {
                var sequenceItemsReader = CreateNestedReader();
                while (sequenceItemsReader.TryReadItemTagOfSequenceWithUndefinedLength())
                {
                    var item = sequenceItemsReader.ReadItemValue(itemDeserializer);
                    handleItem(item);
                }
            }
            else
            {
                Input.Read(ValueLength, () =>
                {
                    var sequenceItemsReader = CreateNestedReader();
                    while (sequenceItemsReader.TryReadItemTagOfSequenceWithDefinedLength())
                    {
                        var item = sequenceItemsReader.ReadItemValue(itemDeserializer);
                        handleItem(item);
                    }
                });
            }
            EndReadValue();
        }

        /// <summary>Tries to skip to the value field of the PixelData attribute, if present</summary>
        public bool TrySkipToPixelData(out uint valueLength)
        {
            // Note that TrySeek will throw if the stream contains any tag in the pixel data group (0x7FE0) before the PixelData tag.
            // This to prevent accidental mis-interpretation of the pixel data.

            if (TrySeek(DicomTag.PixelData))
            {
                valueLength = ValueLength;
                return true;
            }
            else
            {
                valueLength = 0;
                return false;
            }
        }

        private void GetEncapsulatedPixelDataFramePositionsUsingBasicOffsetTable(long[] framePositions)
        {
            if ((ValueLength % 4) != 0)
            {
                throw new Exception($"Invalid basic offset table length {ValueLength}");
            }

            var numberOfFrames = ValueLength / 4;
            if (numberOfFrames != framePositions.Length)
            {
                throw new Exception($"Expected {framePositions.Length} frame offsets in basic offset table but got {numberOfFrames}");
            }

            var positionOfFirstItemTagOfFirstFrameAfterBasicOffsetTable = Input.Position + 4 * numberOfFrames;

            for (var i = 0; i < numberOfFrames; i++)
            {
                var offset = DataReader.Read<UInt32>();
                framePositions[i] = positionOfFirstItemTagOfFirstFrameAfterBasicOffsetTable + offset;
            }
        }

        private void GetEncapsulatedPixelDataFramePositionsWithoutBasicOffsetTable(long[] framePositions)
        {
            if (TransferSyntax.IsJPEG)
            {
                // End-of-frame marker 0xFFD9 == JPEG Interchange EOI == JPEG 2000 EOC
                GetPotentiallyFragmentedEncapsulatedPixelDataFramePositions(framePositions, new byte[] { 0xFF, 0xD9 }); 
            }
            else
            {
                GetNonFragmentedEncapsulatedPixelDataFramePositions(framePositions);
            }
        }

        private void GetPotentiallyFragmentedEncapsulatedPixelDataFramePositions(long[] framePositions,
            byte[] endOfFrameMarker)
        {
            bool IsEndOfFrameMarker(Span<byte> bytes)
            {
                for (var i = 0; i < endOfFrameMarker.Length; i++)
                {
                    if (bytes[i] != endOfFrameMarker[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            for (var i = 0; i < framePositions.Length; i++)
            {
                framePositions[i] = Input.Position;

                var endOfFrameReached = false;

                while (!endOfFrameReached)
                {
                    ReadStartOfPixelDataFragment();

                    if (ValueLength < endOfFrameMarker.Length + 1)
                    {
                        throw new Exception($"Pixel data fragment too short");
                    }

                    // End-of-frame marker may be followed by a one padding byte to ensure fragment length is even

                    Input.SkipBytes(ValueLength - (endOfFrameMarker.Length + 1));

                    var endOfFragment = Input.ReadBytes(endOfFrameMarker.Length + 1);

                    endOfFrameReached = IsEndOfFrameMarker(endOfFragment) || IsEndOfFrameMarker(endOfFragment.AsSpan(1));

                    CurrentTag = DicomTag.Undefined;
                }
            }

            ReadEndOfPixelDataFragmentSequence();
        }

        private void GetNonFragmentedEncapsulatedPixelDataFramePositions(long[] framePositions)
        {
            for (var i = 0; i < framePositions.Length; i++)
            {
                framePositions[i] = Input.Position;

                ReadStartOfPixelDataFragment();
                Input.SkipBytes(ValueLength);
                CurrentTag = DicomTag.Undefined;
            }

            ReadEndOfPixelDataFragmentSequence();
        }

        private void ReadStartOfPixelDataFragment()
        {
            ReadTagVRLength();
            if (CurrentTag != DicomTag.Item)
            {
                throw new Exception($"Expected {DicomTag.Item} for pixel data fragment but got {CurrentTag}");
            }

            if (ValueLength == UndefinedLength)
            {
                throw new Exception($"Pixel data fragment may not have undefined length");
            }
        }

        private void ReadEndOfPixelDataFragmentSequence()
        {
            ReadTagVRLength();
            if (CurrentTag != DicomTag.SequenceDelimitationItem)
            {
                throw new Exception($"Expected {DicomTag.SequenceDelimitationItem} after pixel data fragments but got {CurrentTag}");
            }
        }

        /// <summary>Gets the positions of the encapsulated pixel data frames relative to the start of the input stream</summary>
        public void GetEncapsulatedPixelDataFramePositions(long[] framePositions)
        {
            if (!TrySkipToPixelData(out uint valueLength))
            {
                throw new Exception($"Missing PixelData");
            }

            if (valueLength != uint.MaxValue)
            {
                throw new Exception($"Expected encapsulated pixel data length {uint.MaxValue} but got {valueLength}");
            }

            EndReadValue();

            if (!TryReadItemTagOfSequenceWithUndefinedLength())
            {
                throw new Exception($"Expected {DicomTag.Item} for Basic Offset Table but got {CurrentTag}");
            }

            if (ValueLength > 0)
            {
                GetEncapsulatedPixelDataFramePositionsUsingBasicOffsetTable(framePositions);
                EndReadValue();
            }
            else
            {
                EndReadValue();
                GetEncapsulatedPixelDataFramePositionsWithoutBasicOffsetTable(framePositions);
            }
        }

#pragma warning disable 1591
        
        public void ReadEncapsulatedPixelDataFrame(Action<BufferedStreamReader> decodeFrame)
        {
            if (!TryReadItemTagOfSequenceWithUndefinedLength())
            {
                throw new Exception($"Expected fragment {DicomTag.Item} but got {CurrentTag}");
            }

            if (ValueLength == UndefinedLength)
            {
                throw new Exception($"Fragment {DicomTag.Item} may not have undefined length");
            }

            if (ValueLength > Input.BytesRemaining)
            {
                throw new Exception($"Fragment {DicomTag.Item} length exceeds input");
            }

            if (ValueLength == Input.BytesRemaining)
            {
                Input.Read(ValueLength, () =>
                {
                    decodeFrame(Input);

                    // Pixel data decoding may end at an uneven byte boundary. 
                    // In this case there will be one padding byte left to read

                    if (Input.BytesRemaining == 1)
                    {
                        Input.SkipRemainingBytes();
                    }
                });
            }
            else
            {
                if (Input.BytesRemaining > int.MaxValue)
                {
                    throw new NotSupportedException($"Multi-fragment frame length {Input.BytesRemaining} not supported");
                }

                var buffer = ArrayPool<byte>.Shared.Rent((int)Input.BytesRemaining);
                try
                {
                    var n = 0;
                    do
                    {
                        var fragmentLength = checked((int)ValueLength);
                        Input.ReadAll(buffer.AsSpan(n, fragmentLength));
                        EndReadValue();
                        n += fragmentLength;
                    }
                    while (TryReadItemTagOfSequenceWithUndefinedLength());

                    var bufferReader = new BufferedStreamReader(buffer, n);
                    decodeFrame.Invoke(bufferReader);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
                }
            }
        }

#pragma warning restore 1591

        /// <summary>Reads a user defined data set using the given DicomDataConsumer implementation</summary>
        public void ReadDataSet<TDataSet>(TDataSet dataSet, DicomDataConsumer<TDataSet> consumer)
        {
            Debug.Assert(CurrentTag == DicomTag.Undefined);

            while (!Input.AtEnd)
            {
                ReadTagVRLength();

                if (CurrentTag.GroupNumber >= DicomTag.PixelDataGroupNumber)
                {
                    break;
                }

                DicomAttribute.TryLookup(CurrentTag, out DicomAttribute attribute);

                var vr = ExplicitVR ?? attribute?.ImplicitVR;

                var dataConsumptionOptions = consumer.GetOptions(CurrentTag, vr?.Name);

                if (dataConsumptionOptions.HasFlag(DicomDataConsumptionOptions.Skip))
                {
                    SkipValue();
                }
                else
                {
                    if (vr == null)
                    {
                        consumer.SkippedValueWithUnknownVR(dataSet, CurrentTag, attribute);
                        SkipValue();
                    }
                    else
                    {
                        if (vr == DicomVR.SQ)
                        {
                            TDataSet ConsumeItem(DicomStreamReader itemReader)
                            {
                                var itemDataSet = consumer.CreateItem();
                                itemReader.ReadDataSet(itemDataSet, consumer);
                                return itemDataSet;
                            }

                            using (var sequenceItemConsumer = consumer.CreateSequenceItemConsumer(dataSet, CurrentTag, attribute))
                            {
                                ReadSequenceItems(ConsumeItem, sequenceItemConsumer.AddItem);
                            }
                        }
                        else if (ValueLength == UndefinedLength)
                        {
                            consumer.SkippedValueWithUndefinedLength(dataSet, CurrentTag, attribute);
                            SkipItemsOfSequenceWithUndefinedLength();
                        }
                        else if (dataConsumptionOptions.HasFlag(DicomDataConsumptionOptions.RawValue))
                        {
                            var rawValue = Input.ReadBytes(ValueLength);
                            consumer.ConsumeValue(dataSet, CurrentTag, attribute, rawValue);
                            EndReadValue();
                        }
                        else
                        {
                            var tag = CurrentTag; // Needed because vr.GetValue() calls EndReadValue() which leaves CurrentTag == Undefined
                            var value = vr.GetValue(this);
                            consumer.ConsumeValue(dataSet, tag, attribute, value);
                        }
                    }
                }
            }
        }
    }
}