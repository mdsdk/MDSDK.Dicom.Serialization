// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.Dicom.Serialization.ValueRepresentations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MDSDK.Dicom.Serialization
{
    public static class DicomVR
    {
        public static readonly ApplicationEntity AE = new();
        public static readonly AgeString AS = new();
        public static readonly AttributeTag AT = new();
        public static readonly CodeString CS = new();
        public static readonly Date DA = new();
        public static readonly DecimalString DS = new();
        public static readonly DateTime DT = new();
        public static readonly FloatingPointSingle FL = new();
        public static readonly FloatingPointDouble FD = new();
        public static readonly IntegerString IS = new();
        public static readonly LongString LO = new();
        public static readonly LongText LT = new();
        public static readonly OtherByte OB = new();
        public static readonly OtherDouble OD = new();
        public static readonly OtherFloat OF = new();
        public static readonly OtherLong OL = new();
        public static readonly OtherVeryLong OV = new();
        public static readonly OtherWord OW = new();
        public static readonly PersonName PN = new();
        public static readonly ShortString SH = new();
        public static readonly SignedLong SL = new();
        public static readonly Sequence<object> SQ = new();
        public static readonly SignedShort SS = new();
        public static readonly ShortText ST = new();
        public static readonly SignedVeryLong SV = new();
        public static readonly Time TM = new();
        public static readonly UnlimitedCharacters UC = new();
        public static readonly UniqueIdentifier UI = new();
        public static readonly UnsignedLong UL = new();
        public static readonly Unknown UN = new();
        public static readonly UniversalResourceIdentifier UR = new();
        public static readonly UnsignedShort US = new();
        public static readonly UnlimitedText UT = new();
        public static readonly UnsignedVeryLong UV = new();

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

            foreach (var field in typeof(DicomVR).GetFields())
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

        public static ValueRepresentation Lookup(System.ValueTuple<byte, byte> id)
        {
            return s_vrMap[id];
        }
    }
}

