// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Linq;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class SpecificCharsetEncodedMultiValue : SpecificCharsetEncodedValue, IMultiValue<string>
    {
        internal SpecificCharsetEncodedMultiValue(string vr) : base(vr) { }

        public string[] ReadValues(DicomStreamReader reader)
        {
            var entireValue = ReadEntireValue(reader);
            return (entireValue.Length == 0) ? Array.Empty<string>() : entireValue.Split('\\');
        }

        public string ReadSingleValue(DicomStreamReader reader) => ReadEntireValue(reader);

        public void WriteValues(DicomStreamWriter writer, string[] values)
        {
            if (values.Any(value => value.Contains('\\')))
            {
                throw new ArgumentException("value may not contain \\", nameof(values));
            }
            WriteEntireValue(writer, string.Join('\\', values));
        }

        public void WriteSingleValue(DicomStreamWriter writer, string value) => WriteEntireValue(writer, value);
    }
}
