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

        private const uint UndefinedLength = uint.MaxValue;

        public void WriteVRLength(ValueRepresentation vr, long valueLength, out bool pad)
        {
            if (valueLength >= UndefinedLength)
            {
                throw new ArgumentOutOfRangeException($"Value length {valueLength} out of 32-bit range", nameof(valueLength));
            }

            pad = (valueLength % 2) != 0;
            
            if (pad)
            {
                valueLength++;
            }

            if (VRCoding == DicomVRCoding.Explicit)
            {
                Output.WriteByte(vr.Id.Item1);
                Output.WriteByte(vr.Id.Item2);
                if (vr is IHas16BitExplicitVRLength)
                {
                    if (valueLength >= ushort.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException($"Value length {valueLength} out of 16-bit range", nameof(valueLength));

                    }
                    Output.Write<UInt16>((ushort)valueLength);
                }
                else
                {
                    Output.WriteZeros(2);
                    Output.Write<UInt32>((uint)valueLength);
                }
            }
            else
            {
                Output.Write<UInt32>((uint)valueLength);
            }
        }

        /*
        private T ReadItemValue<T>(Func<DicomStreamReader, T> itemDeserializer)
        {
            T item;
            if (ValueLength == UndefinedLength)
            {
                var itemReader = CreateNestedReader();
                item = itemDeserializer.Invoke(itemReader);
                itemReader.SkipToDelimiter(DicomTag.ItemDelimitationItem);
            }
            else
            {
                var itemBox = Input.OpenBox(ValueLength);
                var itemReader = CreateNestedReader();
                item = itemDeserializer.Invoke(itemReader);
                Input.CloseBox(itemBox);
            }
            CurrentTag = DicomTag.Undefined;
            return item;
        }

        abstract class SequenceReader<T> : IEnumerator<T>
        {
            protected readonly DicomStreamReader _sequenceValueReader;

            protected readonly Func<DicomStreamReader, T> _itemDeserializer;

            protected readonly DicomStreamReader _sequenceItemsReader;

            protected SequenceReader(DicomStreamReader sequenceValueReader, Func<DicomStreamReader, T> itemDeserializer)
            {
                _sequenceValueReader = sequenceValueReader;
                _itemDeserializer = itemDeserializer;
                _sequenceItemsReader = sequenceValueReader.CreateNestedReader();
            }

            public abstract bool MoveNext();

            public T Current { get; protected set; }

            object IEnumerator.Current => Current;

            public abstract void Dispose();

            public void Reset() => throw new NotSupportedException();
        }

        class DefinedLengthSequenceRReader<T> : SequenceReader<T>
        {
            private readonly BinaryStreamReader.ContentBox _sequenceValueBox;

            public DefinedLengthSequenceRReader(DicomStreamReader reader, Func<DicomStreamReader, T> itemDeserializer)
                : base(reader, itemDeserializer)
            {
                _sequenceValueBox = _sequenceValueReader.StartReadValueWithDefinedLength();
            }

            public override bool MoveNext()
            {
                if (_sequenceItemsReader.TryReadItemTagOfSequenceWithDefinedLength())
                {
                    Current = _sequenceItemsReader.ReadItemValue(_itemDeserializer);
                    return true;
                }
                else
                {
                    _sequenceValueReader.EndReadValue(_sequenceValueBox);
                    return false;
                }
            }

            public override void Dispose()
            {
                // Nothing to do
            }
        }

        class UndefinedLengthSequenceReader<T> : SequenceReader<T>
        {
            public UndefinedLengthSequenceReader(DicomStreamReader reader, Func<DicomStreamReader, T> itemDeserializer)
                : base(reader, itemDeserializer)
            {
            }

            public override bool MoveNext()
            {
                if (_sequenceItemsReader.TryReadItemTagOfSequenceWithUndefinedLength())
                {
                    Current = _sequenceItemsReader.ReadItemValue(_itemDeserializer);
                    return true;
                }
                else
                {
                    _sequenceValueReader.CurrentTag = DicomTag.Undefined;
                    return false;
                }
            }

            public override void Dispose()
            {
                // Nothing to do
            }
        }

        internal IEnumerator<T> GetSequenceReader<T>(Func<DicomStreamReader, T> itemDeserializer)
        {
            return (ValueLength == UndefinedLength)
                ? new UndefinedLengthSequenceReader<T>(this, itemDeserializer)
                : new DefinedLengthSequenceRReader<T>(this, itemDeserializer);
        }
        */
    }
}
