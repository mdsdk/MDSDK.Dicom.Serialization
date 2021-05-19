// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
#pragma warning disable 1591
    
    public interface ISingleValue
    {
    }

    public interface ISingleValue<T> : ISingleValue
    {
        T ReadValue(DicomStreamReader dicomStreamReader);

        void WriteValue(DicomStreamWriter dicomStreamWriter, T value);
    }

#pragma warning restore 1591
}
