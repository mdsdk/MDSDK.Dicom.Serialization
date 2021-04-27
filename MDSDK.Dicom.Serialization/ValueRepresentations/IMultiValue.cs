// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal interface IMultiValue
    {
    }
    
    internal interface IMultiValue<T> : IMultiValue
    {
        T[] ReadValues(DicomStreamReader reader);

        T ReadSingleValue(DicomStreamReader reader);

        void WriteValues(DicomStreamWriter writer, T[] values);

        void WriteSingleValue(DicomStreamWriter writer, T value);
    }
}
