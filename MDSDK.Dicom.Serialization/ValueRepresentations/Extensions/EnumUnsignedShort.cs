// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.Serialization.ValueRepresentations.Extensions
{
    public class EnumUnsignedShort<T> : UnsignedShort, IMultiValue<T>, IHasLightWeightValueLengthCalculation<T> where T : unmanaged, Enum
    {
        private static readonly Dictionary<ushort, T> UshortToEnum = new();
        
        private static readonly Dictionary<T, ushort> EnumToUshort = new();

        static EnumUnsignedShort()
        {
            foreach (var enumValue in Enum.GetValues<T>())
            {
                var ushortValue = (ushort)(object)enumValue;
                UshortToEnum.Add(ushortValue, enumValue);
                EnumToUshort.Add(enumValue, ushortValue);
            }
        }

        T[] IMultiValue<T>.ReadValues(DicomStreamReader reader)
        {
            var ushortValues = ReadValues(reader);
            var values = new T[ushortValues.Length];
            for (var i = 0; i < ushortValues.Length; i++)
            {
                values[i] = UshortToEnum[ushortValues[i]];
            }
            return values;
        }

        T IMultiValue<T>.ReadSingleValue(DicomStreamReader reader) => UshortToEnum[ReadSingleValue(reader)];

        void IMultiValue<T>.WriteValues(DicomStreamWriter writer, T[] values)
        {
            var ushortValues = new ushort[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                ushortValues[i] = EnumToUshort[values[i]];
            }
            WriteValues(writer, ushortValues);
        }

        void IMultiValue<T>.WriteSingleValue(DicomStreamWriter writer, T value) => WriteSingleValue(writer, EnumToUshort[value]);

        long IHasLightWeightValueLengthCalculation<T>.GetUnpaddedValueLength(T value) => Unsafe.SizeOf<UInt16>();

        long IHasLightWeightValueLengthCalculation<T>.GetUnpaddedValueLength(T[] values) => values.LongLength * Unsafe.SizeOf<UInt16>();
    }
}
