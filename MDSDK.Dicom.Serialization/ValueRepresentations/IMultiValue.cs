// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
#pragma warning disable 1591
    
    public interface IMultiValue
    {
    }
    
    public interface IMultiValue<T> : IMultiValue
    {
        T[] ReadValues(DicomStreamReader reader);

        T ReadSingleValue(DicomStreamReader reader);

        void WriteValues(DicomStreamWriter writer, T[] values);

        void WriteSingleValue(DicomStreamWriter writer, T value);
    }

#pragma warning restore 1591
}
