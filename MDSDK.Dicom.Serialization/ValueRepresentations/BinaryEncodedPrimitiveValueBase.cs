// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class BinaryEncodedPrimitiveValueBase<T> : ValueRepresentation where T : struct, IFormattable
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
            return array;
        }

        public override string ToString(DicomStreamReader reader)
        {
            IEnumerable<string> EnumerateStringValues(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    var value = reader.Input.Read<T>();
                    yield return value.ToString(null, NumberFormatInfo.InvariantInfo);
                }
            }

            var count = GetValueCount<T>(reader);
            return string.Join('\\', EnumerateStringValues(count));
        }

        protected void WriteArray(DicomStreamWriter writer, T[] value)
        {
            var valueLength = Unsafe.SizeOf<T>() * value.LongLength;
            writer.WriteVRLength(this, valueLength, out _);
            writer.Output.Write<T>(value);
        }
    }
}
