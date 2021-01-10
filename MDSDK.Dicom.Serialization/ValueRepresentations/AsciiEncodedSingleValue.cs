// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class AsciiEncodedSingleValue : AsciiEncodedValue, ISingleValue<string>, IHasDefinedLengthOnly
    {
        internal AsciiEncodedSingleValue(string vr) : base(vr) { }

        public string ReadValue(DicomStreamReader reader) => ReadEntireValue(reader);

        public void WriteValue(DicomStreamWriter writer, string value) => WriteEntireValue(writer, value);
    }
}
