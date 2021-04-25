// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public abstract class BinaryEncodedPrimitiveValueBase<T> : ValueRepresentation, IHasLightWeightValueLengthCalculation<T> 
        where T : unmanaged, IFormattable
    {
        private static readonly Type[] AllowedTypes = new[]
        {
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)
        };

        static BinaryEncodedPrimitiveValueBase()
        {
            Debug.Assert(AllowedTypes.Contains(typeof(T)));
        }

        internal BinaryEncodedPrimitiveValueBase(string vr) : base(vr) { }

        protected T[] ReadArray(DicomStreamReader reader)
        {
            var count = GetValueCount<T>(reader);
            var array = new T[count];
            reader.Input.Read<T>(array);
            reader.EndReadValue();
            return array;
        }

        internal void WriteArray(DicomStreamWriter writer, T[] value)
        {
            var valueLength = Unsafe.SizeOf<T>() * value.LongLength;
            writer.WriteVRWithDefinedValueLength(this, valueLength, out _);
            writer.Output.Write<T>(value);
        }

        long IHasLightWeightValueLengthCalculation<T>.GetUnpaddedValueLength(T value) => Unsafe.SizeOf<T>();

        long IHasLightWeightValueLengthCalculation<T>.GetUnpaddedValueLength(T[] values) => values.LongLength * Unsafe.SizeOf<T>();
    }
}
