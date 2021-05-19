﻿// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal abstract class ValueRepresentation
    {
        public string Name { get; }

        internal ValueTuple<byte, byte> Id { get; }

        internal ValueRepresentation(string name)
        {
            Name = name;
            Id = new ValueTuple<byte, byte>((byte)name[0], (byte)name[1]);
        }

        internal abstract object GetValue(DicomStreamReader reader);

        internal static object GetValue<T>(IMultiValue<T> vr, DicomStreamReader reader)
        {
            var value = vr.ReadValues(reader);
            return (value.Length == 1) ? value[0] : value;
        }

        internal const uint UndefinedLength = uint.MaxValue;

        internal uint GetDefinedValueLength(DicomStreamReader reader)
        {
            if (reader.ValueLength == UndefinedLength)
            {
                throw new IOException($"Undefined value length not valid for VR {Name}");
            }
            return reader.ValueLength;
        }

        internal int GetValueCount(DicomStreamReader reader, int unitSize)
        {
            Debug.Assert(unitSize > 1); // Prevent count overflow and/or bytes being read using this interface

            var valueLength = GetDefinedValueLength(reader);

            if ((valueLength % unitSize) != 0)
            {
                throw new IOException($"Invalid value length {valueLength} for VR {Name}");
            }

            return checked((int)(valueLength / unitSize));
        }

        internal int GetValueCount<T>(DicomStreamReader reader) where T : struct, IFormattable
        {
            return GetValueCount(reader, Unsafe.SizeOf<T>());
        }

        internal void EnsureSingleValue(DicomStreamReader reader, int unitSize)
        {
            if (reader.ValueLength != unitSize)
            {
                throw new IOException($"Invalid single value length {reader.ValueLength} for VR {Name}");
            }
        }
        
        internal void EnsureSingleValue<T>(DicomStreamReader reader) where T : struct, IFormattable
        {
            EnsureSingleValue(reader, Unsafe.SizeOf<T>());
        }

        private Dictionary<Type, object> _multiValueConverters = new();

        internal void AddMultiValueConverter<T, TConverter>() where TConverter : IMultiValue<T> where T : struct
        {
            if (this is IMultiValue<T>)
            {
                // We cannot foresee the consequences of allowing a converter that implements an
                // IMultiValue interface that is already implemented by this VR, so we err on the
                // side of caution and forbid it.
                throw new NotSupportedException($"Invalid converter, VR already supports this value type");
            }
            _multiValueConverters[typeof(T)] = Activator.CreateInstance(typeof(TConverter), this); ;
        }

        internal bool TryGetMultiValueConverter(Type userType, out object converter)
        {
            return _multiValueConverters.TryGetValue(userType, out converter);
        }
    }
}
