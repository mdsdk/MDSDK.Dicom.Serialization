// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public interface ISingleValue
    {
    }

    public interface ISingleValue<T> : ISingleValue
    {
        T ReadValue(DicomStreamReader dicomStreamReader);

        void WriteValue(DicomStreamWriter dicomStreamWriter, T value);

    }
}
