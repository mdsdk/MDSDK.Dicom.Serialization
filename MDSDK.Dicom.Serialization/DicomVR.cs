// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.Dicom.Serialization.ValueRepresentations;
using MDSDK.Dicom.Serialization.ValueRepresentations.Mixed;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MDSDK.Dicom.Serialization
{
    internal static class DicomVR
    {
        internal static readonly ApplicationEntity AE = new();
        internal static readonly AgeString AS = new();
        internal static readonly AttributeTag AT = new();
        internal static readonly CodeString CS = new();
        internal static readonly Date DA = new();
        internal static readonly DecimalString DS = new();
        internal static readonly DateTime DT = new();
        internal static readonly FloatingPointSingle FL = new();
        internal static readonly FloatingPointDouble FD = new();
        internal static readonly IntegerString IS = new();
        internal static readonly LongString LO = new();
        internal static readonly LongText LT = new();
        internal static readonly OtherByte OB = new();
        internal static readonly OtherDouble OD = new();
        internal static readonly OtherFloat OF = new();
        internal static readonly OtherLong OL = new();
        internal static readonly OtherVeryLong OV = new();
        internal static readonly OtherWord OW = new();
        internal static readonly PersonName PN = new();
        internal static readonly ShortString SH = new();
        internal static readonly SignedLong SL = new();
        internal static readonly Sequence<object> SQ = new();
        internal static readonly SignedShort SS = new();
        internal static readonly ShortText ST = new();
        internal static readonly SignedVeryLong SV = new();
        internal static readonly Time TM = new();
        internal static readonly UnlimitedCharacters UC = new();
        internal static readonly UniqueIdentifier UI = new();
        internal static readonly UnsignedLong UL = new();
        internal static readonly Unknown UN = new();
        internal static readonly UniversalResourceIdentifier UR = new();
        internal static readonly UnsignedShort US = new();
        internal static readonly UnlimitedText UT = new();
        internal static readonly UnsignedVeryLong UV = new();

        internal static class Mixed
        {
            internal static readonly OtherByteOrWord OB_or_OW = new();
            internal static readonly UnsignedOrSignedShort US_or_SS = new();
            internal static readonly UnsignedShortOrOtherWord US_or_OW = new();
        }

        private static readonly Dictionary<System.ValueTuple<byte, byte>, ValueRepresentation> s_vrMap;

        static DicomVR()
        {
            var vrsThatAreAlwaysSingleValued = new ValueRepresentation[]
            {
                // Source: http://dicom.nema.org/medical/dicom/current/output/html/part05.html#sect_6.4 + added missing OV
                LT, OB, OD, OF, OL, OV, OW, SQ, ST, UN, UR, UT
            };

            var vrsWith16BitExplicitVRLength = new ValueRepresentation[]  
            {
                // Source: http://dicom.nema.org/medical/dicom/current/output/html/part05.html#sect_7.1.2
                AE, AS, AT, CS, DA, DS, DT, FL, FD, IS, LO, LT, PN, SH, SL, SS, ST, TM, UI, UL, US
            };

            var vrsThatMayHaveUndefinedLength = new ValueRepresentation[]
            {
                // Source: http://dicom.nema.org/medical/dicom/current/output/html/part05.html#sect_7.1.2
                OB, OD, OF, OL, OV, OW, SQ, UN
            };

            s_vrMap = new Dictionary<System.ValueTuple<byte, byte>, ValueRepresentation>();

            foreach (var field in typeof(DicomVR).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (typeof(ValueRepresentation).IsAssignableFrom(field.FieldType))
                {
                    var vr = (ValueRepresentation)field.GetValue(null);
                    Debug.Assert(vr.Name == field.Name);
                    Debug.Assert((vr is ISingleValue) != (vr is IMultiValue));
                    Debug.Assert((vr is ISingleValue) == vrsThatAreAlwaysSingleValued.Contains(vr));
                    Debug.Assert((vr is IHas16BitExplicitVRLength) != (vr is IHas32BitExplicitVRLength));
                    Debug.Assert((vr is IHas16BitExplicitVRLength) == vrsWith16BitExplicitVRLength.Contains(vr));
                    Debug.Assert((vr is IMayHaveUndefinedLength) != (vr is IHasDefinedLengthOnly));
                    Debug.Assert((vr is IMayHaveUndefinedLength) == vrsThatMayHaveUndefinedLength.Contains(vr));
                    s_vrMap.Add(vr.Id, vr);
                }
            }
        }

        internal static ValueRepresentation Lookup(System.ValueTuple<byte, byte> id)
        {
            return s_vrMap[id];
        }

        internal static ValueRepresentation Lookup(string id)
        {
            var b0 = (byte)id[0];
            var b1 = (byte)id[1];
            return Lookup(new System.ValueTuple<byte, byte>(b0, b1));
        }
    }
}

