// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class DecimalString : AsciiEncodedMultiValue, IMultiValue<decimal>, IMultiValue<double>, IMultiValue<float>
    {
        public DecimalString() : base("DS") { }

        decimal[] IMultiValue<decimal>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, decimal.Parse);
        
        decimal IMultiValue<decimal>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, decimal.Parse);

        void IMultiValue<decimal>.WriteValues(DicomStreamWriter writer, decimal[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<decimal>.WriteSingleValue(DicomStreamWriter writer, decimal value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);

        double[] IMultiValue<double>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, double.Parse);
        
        double IMultiValue<double>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, double.Parse);

        void IMultiValue<double>.WriteValues(DicomStreamWriter writer, double[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<double>.WriteSingleValue(DicomStreamWriter writer, double value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);

        float[] IMultiValue<float>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, float.Parse);
        
        float IMultiValue<float>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, float.Parse);

        void IMultiValue<float>.WriteValues(DicomStreamWriter writer, float[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<float>.WriteSingleValue(DicomStreamWriter writer, float value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);
    }
}
