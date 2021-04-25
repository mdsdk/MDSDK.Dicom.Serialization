// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace MDSDK.Dicom.Serialization.Test
{
    class DicomToXmlConverter : DicomDataConsumer<XElement>
    {
        private static string ToString(object value)
        {
            return value switch
            {
                string stringValue => stringValue,
                DicomTag dicomTagValue => dicomTagValue.ToString(),
                IFormattable formattableValue => formattableValue.ToString(null, NumberFormatInfo.InvariantInfo),
                IEnumerable enumerableValue => string.Join('\\', enumerableValue.Cast<object>().Select(ToString)),
                _ => throw new NotSupportedException(value.GetType().Name)
            };
        }

        private XElement AddElement(XElement dataSet, DicomTag tag, DicomAttribute attribute)
        {
            var dataElementName = (attribute == null)
                ? $"{(tag.IsPrivate ? 'P' : 'U')}{tag.GroupNumber:X4}.{tag.ElementNumber:X4}"
                : attribute.Keyword;

            var dataElement = new XElement(dataElementName);
            dataSet.Add(dataElement);
            return dataElement;
        }

        public override void ConsumeValue(XElement dataSet, DicomTag tag, DicomAttribute attribute, object value)
        {
            AddElement(dataSet, tag, attribute).Value = ToString(value);
        }

        class SequenceItemContainer : ISequenceItemConsumer
        {
            private readonly XElement _sequenceElement;

            public SequenceItemContainer(XElement sequenceElement) => _sequenceElement = sequenceElement;

            public void AddItem(XElement item) => _sequenceElement.Add(item);

            public void Dispose() { }
        }

        public override ISequenceItemConsumer CreateSequenceItemConsumer(XElement dataSet, DicomTag tag, DicomAttribute attribute)
        {
            var sequenceElement = AddElement(dataSet, tag, attribute);
            return new SequenceItemContainer(sequenceElement);
        }

        public override XElement CreateItem() => new XElement(DicomAttribute.Item.Keyword);
    }
}
