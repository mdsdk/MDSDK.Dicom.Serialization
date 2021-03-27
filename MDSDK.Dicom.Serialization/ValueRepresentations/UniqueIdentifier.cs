// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Globalization;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class UniqueIdentifier : AsciiEncodedMultiValue, IMultiValue<DicomUID>
    {
        public UniqueIdentifier() : base("UI") { }

        private static DicomUID ToDicomUID(string uid, NumberFormatInfo _) => new DicomUID(uid);

        private static string ToString(DicomUID dicomUID, NumberFormatInfo _) => dicomUID.UID;

        DicomUID[] IMultiValue<DicomUID>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, ToDicomUID);

        DicomUID IMultiValue<DicomUID>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, ToDicomUID);

        void IMultiValue<DicomUID>.WriteValues(DicomStreamWriter writer, DicomUID[] values) => ConvertAndWriteValues(writer, ToString, values);

        void IMultiValue<DicomUID>.WriteSingleValue(DicomStreamWriter writer, DicomUID value) => ConvertAndWriteSingleValue(writer, ToString, value);
    }
}
