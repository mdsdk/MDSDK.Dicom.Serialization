// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class BinaryEncodedPrimitiveValue<T> : BinaryEncodedPrimitiveValueBase<T>, IMultiValue<T> where T : struct, IFormattable
    {
        internal BinaryEncodedPrimitiveValue(string vr) : base(vr) { }

        public T[] ReadValues(DicomStreamReader reader) => ReadArray(reader);

        public T ReadSingleValue(DicomStreamReader reader)
        {
            EnsureSingleValue<T>(reader);
            return reader.Input.Read<T>();
        }

        public void WriteValues(DicomStreamWriter writer, T[] values) => WriteArray(writer, values);

        public void WriteSingleValue(DicomStreamWriter writer, T value)
        {
            writer.WriteVRLength(this, Unsafe.SizeOf<T>(), out _);
            writer.Output.Write<T>(value);
        }
    }
}
