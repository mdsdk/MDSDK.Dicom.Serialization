// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.IO;
using System.Text;

// DICOM related documentation
//
// Part 3 "Information Object Definitions"
// 
//     Section C.12.1.1.2 "Specific Character Set"
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part03/sect_C.12.html#sect_C.12.1.1.2
//
// Part 5 "Data Structures and Encoding"
//
//     Section 6.1 "Support of Character Repertoires" 
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_6.html#sect_6.1
//
//     Appendix E "DICOM Default Character Repertoire (Normative)"
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_E.html
//
//     Appendix H "H Character Sets and Person Name Value Representation in the Japanese Language (Informative)"
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_H.html
//
//     Appendix I "Character Sets and Person Name Value Representation in the Korean Language (Informative)"
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_I.html
//
//     Appendix J "Character Sets and Person Name Value Representation using Unicode UTF-8, GB18030 and GBK (Informative)"
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_J.html
//
//     Appendix K "Character Sets and Person Name Value Representation in the Chinese Language with Code Extensions (Informative)"
//     http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_K.html

namespace MDSDK.Dicom.Serialization.Internal
{
    internal abstract class StringDecoder
    {
        private static Encoding GB18030Encoding { get; set; }
        
        static StringDecoder()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GB18030Encoding = Encoding.GetEncoding("GB18030");
        }

        public abstract string Decode(ValueRepresentation vr, ReadOnlyMemory<byte> bytes);

        internal static StringDecoder Default { get; } = new SimpleDicomStringDecoder(Encoding.ASCII);

        private static Encoding GetEncoding(string specificCharacterSet)
        {
            switch (specificCharacterSet)
            {
                case "": return Encoding.ASCII;
                case "ISO_IR 192": return Encoding.UTF8;
                case "GB18030": return GB18030Encoding;
                case "GBK": return GB18030Encoding;
            }
            var legacyEncoding = GetLegacyEncoding(specificCharacterSet);
            return legacyEncoding.G1.Encoding;
        }

        private static LegacyEncoding GetLegacyEncoding(string specificCharacterSet)
        {
            LegacyEncoding.LookupTable.TryGetValue(specificCharacterSet, out LegacyEncoding legacyEncoding);
            return legacyEncoding ?? throw new Exception($"Unknown character set '{specificCharacterSet}'");
        }

        private static LegacyEncoding[] GetLegacyEncodings(string[] specificCharacterSets)
        {
            var legacyEncodings = new LegacyEncoding[specificCharacterSets.Length];
            for (var i = 0; i < specificCharacterSets.Length; i++)
            {
                legacyEncodings[i] = GetLegacyEncoding(specificCharacterSets[i]);
            }
            return legacyEncodings;
        }

        public static StringDecoder Get(string[] specificCharacterSets)
        {
            if ((specificCharacterSets == null) || (specificCharacterSets.Length == 0))
            {
                return Default;
            }
            else if (specificCharacterSets.Length == 1)
            {
                var encoding = GetEncoding(specificCharacterSets[0]);
                return new SimpleDicomStringDecoder(encoding);
            }
            else
            {
                var legacyEncodings = GetLegacyEncodings(specificCharacterSets);
                return new LegacyDicomStringDecoder(legacyEncodings);
            }
        }
    }

    internal class SimpleDicomStringDecoder : StringDecoder
    {
        internal Encoding Encoding { get; }

        internal SimpleDicomStringDecoder(Encoding encoding)
        {
            Encoding = encoding;
        }

        public override string Decode(ValueRepresentation vr, ReadOnlyMemory<byte> bytes) => Encoding.GetString(bytes.Span);
    }

    internal class LegacyDicomStringDecoder : StringDecoder
    {
        internal LegacyEncoding[] LegacyEncodings { get; }

        internal LegacyDicomStringDecoder(LegacyEncoding[] legacyEncodings)
        {
            LegacyEncodings = legacyEncodings;
        }

        private bool StartsWith(ReadOnlySpan<byte> bytes, byte[] escapeSequence)
        {
            for (var i = 0; i < escapeSequence.Length; i++)
            {
                if ((i == bytes.Length) || (bytes[i] != escapeSequence[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private void DecodeEscapedSubstrings(ReadOnlyMemory<byte> bytes, StringBuilder stringBuilder, string separators, out int separatorPos,
            out char separator)
        {
            var g0 = LegacyEncodings[0].G0;
            var g1 = LegacyEncodings[0].G1;

            var escapeSequencePos = -1;
            var substringStartPos = 0;
            var currentPos = 0;

            var activeCharacterSet = g0;

            void ProcessEscapeSequence()
            {
                escapeSequencePos = currentPos;

                var span = bytes.Span.Slice(escapeSequencePos);

                activeCharacterSet = null;

                for (var i = 0; (activeCharacterSet == null) && (i < LegacyEncodings.Length); i++)
                {
                    var encoding = LegacyEncodings[i];
                    if ((encoding.G0 != null) && StartsWith(span, encoding.G0.EscapeSequence))
                    {
                        activeCharacterSet = g0 = encoding.G0;
                    }
                    else if ((encoding.G1 != null) && StartsWith(span, encoding.G1.EscapeSequence))
                    {
                        activeCharacterSet = g1 = encoding.G1;
                    }
                }

                if (activeCharacterSet == null)
                {
                    throw new IOException("Unknown escape sequence in input");
                }

                currentPos += activeCharacterSet.EscapeSequence.Length;

                substringStartPos = currentPos;
            }

            void ConsumePendingSubstring()
            {
                if (currentPos > substringStartPos)
                {
                    var spanStartPos = (activeCharacterSet.EncodingUsesISO2022EscapeSequences && (escapeSequencePos >= 0))
                        ? escapeSequencePos
                        : substringStartPos;
                    var span = bytes.Span.Slice(spanStartPos, currentPos - spanStartPos);
                    var substring = activeCharacterSet.Encoding.GetString(span);
                    var normalizedSubstring = substring.Normalize(NormalizationForm.FormKC);
                    stringBuilder.Append(normalizedSubstring);
                    substringStartPos = currentPos;
                }
                escapeSequencePos = -1;
            }

            var span = bytes.Span;

            const byte ESC = 0x1B;

            separatorPos = -1;
            separator = default;

            while (currentPos < span.Length)
            {
                var b = span[currentPos];

                if ((activeCharacterSet == g0) && g0.IsSingleByte && separators.Contains((char)b))
                {
                    separatorPos = currentPos;
                    separator = (char)b;
                    break;
                }

                if (b == ESC)
                {
                    ConsumePendingSubstring();
                    ProcessEscapeSequence();
                }
                else
                {
                    var characterSet = (b & 0x80) == 0 ? g0 : g1;
                    if (activeCharacterSet != characterSet)
                    {
                        ConsumePendingSubstring();
                        activeCharacterSet = characterSet;
                        if (activeCharacterSet == null)
                        {
                            throw new IOException($"Missing character set for {b}");
                        }
                    }
                    currentPos++;
                }
            }

            ConsumePendingSubstring();
        }

        private string Decode(ReadOnlyMemory<byte> bytes, string separators)
        {
            var stringBuilder = new StringBuilder();
            DecodeEscapedSubstrings(bytes, stringBuilder, separators, out int separatorPos, out char separator);
            while (separatorPos >= 0)
            {
                stringBuilder.Append(separator);
                bytes = bytes.Slice(separatorPos + 1);
                DecodeEscapedSubstrings(bytes, stringBuilder, separators, out separatorPos, out separator);
            }
            return stringBuilder.ToString();
        }

        public override string Decode(ValueRepresentation vr, ReadOnlyMemory<byte> bytes)
        {
            string separators;

            if (vr is ISingleValue<string>)
            {
                separators = "\r\n\t\f";
            }
            else
            {
                separators = "\\";
                if (vr is PersonName)
                {
                    separators += "^=";
                }
            }

            return Decode(bytes, separators);
        }
    }
}
