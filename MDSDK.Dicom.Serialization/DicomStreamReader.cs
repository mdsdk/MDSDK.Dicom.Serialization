// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace MDSDK.Dicom.Serialization
{
    public class DicomStreamReader
    {
        public BinaryStreamReader Input { get; init; }

        public DicomVRCoding VRCoding { get; init; }

        public DicomStreamReader(BinaryStreamReader input, DicomVRCoding vrCoding)
        {
            Input = input;
            VRCoding = vrCoding;
        }

        public Encoding SpecificCharsetEncoding { get; private set; }

        private DicomStreamReader CreateNestedReader()
        {
            return new DicomStreamReader(Input, VRCoding)
            {
                SpecificCharsetEncoding = SpecificCharsetEncoding
            };
        }

        private const uint UndefinedLength = uint.MaxValue;

        public DicomTag CurrentTag { get; private set; } = DicomTag.Undefined;

        public ValueRepresentation ExplicitVR { get; private set; }

        public uint ValueLength { get; private set; }

        private void ReadTagVRLength()
        {
            if (CurrentTag != DicomTag.Undefined)
            {
                throw new InvalidOperationException($"Reading of {CurrentTag} not completed");
            }

            CurrentTag = DicomTag.ReadFrom(Input);

            if (CurrentTag.HasVR && (VRCoding == DicomVRCoding.Explicit))
            {
                var b0 = Input.ReadByte();
                var b1 = Input.ReadByte();
                var id = new ValueTuple<byte, byte>(b0, b1);
                ExplicitVR = DicomVR.Lookup(id);
                if (ExplicitVR is IHas16BitExplicitVRLength)
                {
                    ValueLength = Input.Read<UInt16>();
                }
                else
                {
                    Input.SkipBytes(2);
                    ValueLength = Input.Read<UInt32>();
                }
            }
            else
            {
                ValueLength = Input.Read<UInt32>();
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

        private void SkipValueWithDefinedLength()
        {
            Input.SkipBytes(ValueLength);
            CurrentTag = DicomTag.Undefined;
        }

        private void SkipValue()
        {
            if (CurrentTag == DicomTag.SpecificCharacterSet)
            {
                var value = DicomVR.CS.ReadValues(this).First();
                SpecificCharsetEncoding = DicomCharacterSet.GetEncoding(value);
            }
            else if (ValueLength == UndefinedLength)
            {
                var sequenceReader = CreateNestedReader();
                sequenceReader.SkipItemsOfSequenceWithUndefinedLength();
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
                item = Input.Read<T>(ValueLength, () => itemDeserializer.Invoke(itemReader));
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

        public void ToXml(XElement dataset)
        {
            Debug.Assert(CurrentTag == DicomTag.Undefined);

            while (!Input.AtEnd)
            {
                ReadTagVRLength();

                if (!CurrentTag.IsBeforePixelData)
                {
                    break;
                }

                DicomAttribute.TryLookup(CurrentTag, out DicomAttribute attribute);

                var dataElementName = (attribute == null)
                    ? $"{(CurrentTag.IsPrivateTag ? 'P' : 'U')}{CurrentTag.GroupNumber:X4}.{CurrentTag.ElementNumber:X4}"
                    : attribute.Keyword;

                var dataElement = new XElement(dataElementName);
                dataset.Add(dataElement);

                var vr = ExplicitVR ?? attribute?.ImplicitVR;
                if (vr == null)
                {
                    dataElement.SetAttributeValue("Comment", "Skipped (Cannot determine VR)");
                    SkipValue();
                }
                else
                {
                    if (vr == DicomVR.SQ)
                    {
                        static XElement DeserializeItemXml(DicomStreamReader itemReader)
                        {
                            var itemDataset = new XElement(DicomAttribute.Item.Keyword);
                            itemReader.ToXml(itemDataset);
                            return itemDataset;
                        }

                        ReadSequenceItems(DeserializeItemXml, dataElement.Add);
                    }
                    else if (ValueLength == UndefinedLength)
                    {
                        dataElement.SetAttributeValue("Comment", "Skipped (UndefinedLength)");
                        SkipItemsOfSequenceWithUndefinedLength();
                    }
                    else
                    {
                        dataElement.Value = vr.ToString(this);
                    }
                }
            }
        }
    }
}
