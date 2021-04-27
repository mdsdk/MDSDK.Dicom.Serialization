// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>DICOM tag</summary>
    public struct DicomTag : IEquatable<DicomTag>, IComparable<DicomTag>
    {
        private readonly uint _value;

        /// <summary>The group number of the DICOM tag</summary>
        public ushort GroupNumber => (ushort)(_value >> 16);

        /// <summary>The element number of the DICOM tag</summary>
        public ushort ElementNumber => (ushort)(_value & 0xFFFF);

        /// <summary>Creates a DICOM tag from the given group and element number</summary>
        public DicomTag(ushort groupNumber, ushort elementNumber)
        {
            _value = (uint)((groupNumber << 16) | elementNumber);
        }

        /// <summary>Indicates whether this is a private tag</summary>
        public bool IsPrivate => (GroupNumber & 1) != 0;

#pragma warning disable CS1591

        public bool Equals(DicomTag other) => _value == other._value;

        public int CompareTo(DicomTag other) => _value.CompareTo(other._value);

        public override bool Equals(object obj) => (obj is DicomTag dicomTag) && Equals(dicomTag);

        public override int GetHashCode() => (int)_value;

        public override string ToString() => $"({GroupNumber:X4},{ElementNumber:X4})";

        public static bool operator ==(DicomTag a, DicomTag b) => a._value == b._value;
        public static bool operator !=(DicomTag a, DicomTag b) => a._value != b._value;

        public static bool operator <(DicomTag a, DicomTag b) => a._value < b._value;
        public static bool operator >(DicomTag a, DicomTag b) => a._value > b._value;

        public static bool operator <=(DicomTag a, DicomTag b) => a._value <= b._value;
        public static bool operator >=(DicomTag a, DicomTag b) => a._value >= b._value;

#pragma warning restore

        internal static DicomTag ReadFrom(BinaryDataReader input)
        {
            input.Read(out ushort groupNumber);
            input.Read(out ushort elementNumber);
            return new DicomTag(groupNumber, elementNumber);
        }

        internal void WriteTo(BinaryDataWriter output)
        {
            output.Write(GroupNumber);
            output.Write(ElementNumber);
        }

        internal static readonly DicomTag CommandGroupLength = new DicomTag(0x0000, 0x0000);
        internal static readonly DicomTag SpecificCharacterSet = new DicomTag(0x0008, 0x0005);
        internal static readonly DicomTag Item = new DicomTag(0xFFFE, 0xE000);
        internal static readonly DicomTag ItemDelimitationItem = new DicomTag(0xFFFE, 0xE00D);
        internal static readonly DicomTag SequenceDelimitationItem = new DicomTag(0xFFFE, 0xE0DD);

        internal static readonly DicomTag Undefined = new DicomTag(0xFFFF, 0xFFFF);

        internal bool IsDelimitationItem => (this == ItemDelimitationItem) || (this == SequenceDelimitationItem);

        internal bool HasVR => (this != Item) && !IsDelimitationItem;

        internal const ushort PixelDataGroupNumber = 0x7FE0;

        internal static readonly DicomTag ExtendedOffsetTable = new DicomTag(PixelDataGroupNumber, 0x001);
        internal static readonly DicomTag ExtendedOffsetTableLength = new DicomTag(PixelDataGroupNumber, 0x002);
        internal static readonly DicomTag FloatPixelData = new DicomTag(PixelDataGroupNumber, 0x008);
        internal static readonly DicomTag DoubleFloatPixelData = new DicomTag(PixelDataGroupNumber, 0x009);
        internal static readonly DicomTag PixelData = new DicomTag(PixelDataGroupNumber, 0x0010);
    }
}
