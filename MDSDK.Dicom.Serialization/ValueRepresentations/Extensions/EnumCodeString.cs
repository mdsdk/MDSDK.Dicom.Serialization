// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Globalization;

namespace MDSDK.Dicom.Serialization.ValueRepresentations.Extensions
{
    public class EnumCodeString<T> : CodeString, IMultiValue<T>, IHasLightWeightValueLengthCalculation<T> where T : struct, Enum
    {
        private static T Convert(string s, NumberFormatInfo _) => Enum.Parse<T>(s);

        private static string ToString(T value, NumberFormatInfo _) => Enum.GetName<T>(value);

        T[] IMultiValue<T>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, Convert);

        T IMultiValue<T>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, Convert);

        void IMultiValue<T>.WriteValues(DicomStreamWriter writer, T[] values) => ConvertAndWriteValues(writer, ToString, values);

        void IMultiValue<T>.WriteSingleValue(DicomStreamWriter writer, T value) => ConvertAndWriteSingleValue(writer, ToString, value);

        long IHasLightWeightValueLengthCalculation<T>.GetUnpaddedValueLength(T value) => Enum.GetName<T>(value).Length;

        long IHasLightWeightValueLengthCalculation<T>.GetUnpaddedValueLength(T[] values)
        {
            if (values.Length == 0)
            {
                return 0;
            }
            else
            {
                long unpaddedValueLength = Enum.GetName<T>(values[0]).Length;
                for (var i = 1; i < values.Length; i++)
                {
                    unpaddedValueLength++; // '\' separator
                    unpaddedValueLength += Enum.GetName<T>(values[i]).Length;
                }
                return unpaddedValueLength;
            }
        }
    }
}
