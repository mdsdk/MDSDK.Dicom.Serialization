// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Collections.Generic;
using static MDSDK.Dicom.Serialization.Internal.LegacyCharacterSet;

namespace MDSDK.Dicom.Serialization.Internal
{
    internal class LegacyEncoding 
    {
        public static Dictionary<string, LegacyEncoding> LookupTable { get; } = new Dictionary<string, LegacyEncoding>();

        public LegacyCharacterSet G0 { get; }

        public LegacyCharacterSet G1 { get; }

        internal LegacyEncoding(LegacyCharacterSet g0, LegacyCharacterSet g1)
        {
            G0 = g0;
            G1 = g1;

            if ((G0 == IR_6) && (G1 == null))
            {
                LookupTable.Add("", this);
                LookupTable.Add("ISO 2022 IR 6", this);
            }
            else 
            {
                var registrationNumber = (G1 ?? G0).ISORegistrationNumber;
                LookupTable.Add($"ISO_IR {registrationNumber}", this);
                LookupTable.Add($"ISO 2022 IR {registrationNumber}", this);
            }
        }

        // Classic single-byte
        public static readonly LegacyEncoding Default = new LegacyEncoding(IR_6, null);
        public static readonly LegacyEncoding LatinAlphabetNo1 = new LegacyEncoding(IR_6, IR_100);
        public static readonly LegacyEncoding LatinAlphabetNo2 = new LegacyEncoding(IR_6, IR_101);
        public static readonly LegacyEncoding LatinAlphabetNo3 = new LegacyEncoding(IR_6, IR_109);
        public static readonly LegacyEncoding LatinAlphabetNo4 = new LegacyEncoding(IR_6, IR_110);
        public static readonly LegacyEncoding Cyrillic = new LegacyEncoding(IR_6, IR_144);
        public static readonly LegacyEncoding Arabic = new LegacyEncoding(IR_6, IR_127);
        public static readonly LegacyEncoding Greek  = new LegacyEncoding(IR_6, IR_126);
        public static readonly LegacyEncoding Hebrew = new LegacyEncoding(IR_6, IR_138);
        public static readonly LegacyEncoding LatinAlphabetNo5 = new LegacyEncoding(IR_6, IR_148);
        public static readonly LegacyEncoding Japanese_JIS_X_0201 = new LegacyEncoding(IR_14, IR_13);
        public static readonly LegacyEncoding Thai = new LegacyEncoding(IR_6, IR_166);
        
        // Classic multi-byte
        public static readonly LegacyEncoding Japanese_JIS_X_0208 = new LegacyEncoding(IR_87, null);
        public static readonly LegacyEncoding Japanese_JIS_X_0212 = new LegacyEncoding(IR_159, null);
        public static readonly LegacyEncoding Korean = new LegacyEncoding(null, IR_149);
        public static readonly LegacyEncoding SimplifiedChinese = new LegacyEncoding(null, IR_58);
    }
}
