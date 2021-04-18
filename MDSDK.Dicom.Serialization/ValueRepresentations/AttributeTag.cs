// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Collections.Generic;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class AttributeTag : ValueRepresentation, IMultiValue<DicomTag>, IHas16BitExplicitVRLength,
        IHasLightWeightValueLengthCalculation<DicomTag>
    {
        public AttributeTag() : base("AT") { }

        public DicomTag[] ReadValues(DicomStreamReader reader)
        {
            var count = GetValueCount(reader, 4);
            var array = new DicomTag[count];
            for (var i = 0; i < count; i++)
            {
                array[i] = DicomTag.ReadFrom(reader.Input);
            }
            reader.EndReadValue();
            return array;
        }

        public DicomTag ReadSingleValue(DicomStreamReader reader)
        {
            EnsureSingleValue(reader, 4);
            var tag = DicomTag.ReadFrom(reader.Input);
            reader.EndReadValue();
            return tag;
        }

        internal override string ToString(DicomStreamReader reader)
        {
            IEnumerable<string> EnumerateStringValues(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    var tag = DicomTag.ReadFrom(reader.Input);
                    DicomAttribute.TryLookup(tag, out DicomAttribute attribute);
                    yield return (attribute == null) ? tag.ToString() : attribute.Keyword ?? attribute.Name;
                }
            }

            var count = GetValueCount(reader, 4);
            var result = string.Join('\\', EnumerateStringValues(count));
            reader.EndReadValue();
            return result;
        }

        internal override object GetValue(DicomStreamReader reader) => GetValue<DicomTag>(this, reader);

        public void WriteValues(DicomStreamWriter writer, DicomTag[] values)
        {
            writer.WriteVRWithDefinedValueLength(this, 4 * values.Length, out _);
            foreach (var value in values)
            {
                value.WriteTo(writer.Output);
            }
        }

        public void WriteSingleValue(DicomStreamWriter writer, DicomTag value)
        {
            writer.WriteVRWithDefinedValueLength(this, 4, out _);
            value.WriteTo(writer.Output);
        }

        long IHasLightWeightValueLengthCalculation<DicomTag>.GetUnpaddedValueLength(DicomTag value) => 4;

        long IHasLightWeightValueLengthCalculation<DicomTag>.GetUnpaddedValueLength(DicomTag[] values) => values.Length * 4;
    }
}
