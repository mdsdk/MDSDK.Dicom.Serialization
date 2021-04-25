// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Text;

namespace MDSDK.Dicom.Serialization.Internal
{
    class LegacyCharacterSet
    {
        public LegacyCharacterSetType EncodingType { get; }

        public bool IsSingleByte => EncodingType == LegacyCharacterSetType.SingleByte;

        public int ISORegistrationNumber { get; }

        public string Name { get; }

        public Encoding Encoding { get; }

        public bool EncodingUsesISO2022EscapeSequences { get; }

        public byte[] EscapeSequence { get; }

        private LegacyCharacterSet(LegacyCharacterSetType encodingType, int isoRegistrationNumber, int codePage, string name,
            params byte[] escapeSequence)
        {
            EncodingType = encodingType;
            ISORegistrationNumber = isoRegistrationNumber;
            Encoding = Encoding.GetEncoding(codePage);
            EncodingUsesISO2022EscapeSequences = Encoding.BodyName.StartsWith("iso-2022");
            Name = name;
            EscapeSequence = escapeSequence;
        }

        const byte ESC = 0x1B;

        public static readonly LegacyCharacterSet IR_6 = new(LegacyCharacterSetType.SingleByte, 6, 20127, "ISO 646", ESC, 0x28, 0x42);
        public static readonly LegacyCharacterSet IR_13 = new(LegacyCharacterSetType.SingleByte, 13, 50220, "JIS X 0201: Katakana", ESC, 0x29, 0x49);
        public static readonly LegacyCharacterSet IR_14 = new(LegacyCharacterSetType.SingleByte, 14, 50220, "JIS X 0201: Romaji", ESC, 0x28, 0x4A);
        public static readonly LegacyCharacterSet IR_58 = new(LegacyCharacterSetType.MultiByte, 58, 20936, "GB 2312-80", ESC, 0x24, 0x29, 0x41);
        public static readonly LegacyCharacterSet IR_87 = new(LegacyCharacterSetType.MultiByte, 87, 50220, "JIS X 0208: Kanji", ESC, 0x24, 0x42);
        public static readonly LegacyCharacterSet IR_100 = new(LegacyCharacterSetType.SingleByte, 100, 28591, "ISO 8859-1 Latin-1 Western European", ESC, 0x2D, 0x41);
        public static readonly LegacyCharacterSet IR_101 = new(LegacyCharacterSetType.SingleByte, 101, 28592, "ISO 8859-2 Latin-2 Central European", ESC, 0x2D, 0x42);
        public static readonly LegacyCharacterSet IR_109 = new(LegacyCharacterSetType.SingleByte, 109, 28593, "ISO 8859-3 Latin-3 South European", ESC, 0x2D, 0x43);
        public static readonly LegacyCharacterSet IR_110 = new(LegacyCharacterSetType.SingleByte, 110, 28594, "ISO 8859-4 Latin-4 North European", ESC, 0x2D, 0x44);
        public static readonly LegacyCharacterSet IR_126 = new(LegacyCharacterSetType.SingleByte, 126, 28597, "ISO 8859-7 Latin/Greek", ESC, 0x2D, 0x46);
        public static readonly LegacyCharacterSet IR_127 = new(LegacyCharacterSetType.SingleByte, 127, 28596, "ISO 8859-6 Latin/Arabic", ESC, 0x2D, 0x47);
        public static readonly LegacyCharacterSet IR_138 = new(LegacyCharacterSetType.SingleByte, 138, 28598, "ISO 8859-8 Latin/Hebrew", ESC, 0x2D, 0x48);
        public static readonly LegacyCharacterSet IR_144 = new(LegacyCharacterSetType.SingleByte, 144, 28595, "ISO 8859-5 Latin/Cyrillic", ESC, 0x2D, 0x4C);
        public static readonly LegacyCharacterSet IR_148 = new(LegacyCharacterSetType.SingleByte, 148, 28599, "ISO 8859-9 Latin-5 Turkish", ESC, 0x2D, 0x4D);
        public static readonly LegacyCharacterSet IR_149 = new(LegacyCharacterSetType.MultiByte, 149, 51949, "KS X 1001: Hangul and Hanja", ESC, 0x24, 0x29, 0x43);
        public static readonly LegacyCharacterSet IR_159 = new(LegacyCharacterSetType.MultiByte, 159, 50220, "JIS X 0212: Supplementary Kanji set", ESC, 0x24, 0x28, 0x44);
        public static readonly LegacyCharacterSet IR_166 = new(LegacyCharacterSetType.SingleByte, 166, 874, "TIS 620-2533 (1990)", ESC, 0x2D, 0x54);
    }
}
