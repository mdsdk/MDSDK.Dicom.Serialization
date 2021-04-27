// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal class SpecificCharsetEncodedSingleValue : SpecificCharsetEncodedValue, ISingleValue<string>
    {
        internal SpecificCharsetEncodedSingleValue(string vr) : base(vr) { }

        public string ReadValue(DicomStreamReader reader) => ReadEntireValue(reader);

        void ISingleValue<string>.WriteValue(DicomStreamWriter writer, string value) => WriteEntireValue(writer, value);
    }
}

