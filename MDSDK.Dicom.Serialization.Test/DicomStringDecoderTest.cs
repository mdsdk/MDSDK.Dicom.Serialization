// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;
using System.Globalization;

namespace MDSDK.Dicom.Serialization.Test
{
    public static class DicomStringDecoderTest
    {
        class TestCase
        {
            public ValueRepresentation VR { get; set; }
            public string[] SpecificCharacterSets { get; set; }
            public string CharacterString { get; set; }
            public string EncodedRepresentation { get; set; }

            private int ParseNibblePair(string nibblePair)
            {
                if (nibblePair.StartsWith("0x"))
                {
                    return int.Parse(nibblePair.Substring(2), NumberStyles.HexNumber);
                }
                else
                {
                    var nibbles = nibblePair.Split('/');
                    return (int.Parse(nibbles[0]) << 4) + int.Parse(nibbles[1]);
                }
            }

            private byte[] ParseEncodedRepresentation()
            {
                var nibblePairs = EncodedRepresentation.Split(' ');
                var bytes = new byte[nibblePairs.Length];
                for (var i = 0; i < nibblePairs.Length; i++)
                {
                    bytes[i] = (byte)ParseNibblePair(nibblePairs[i]);
                }
                return bytes;
            }

            public void Run()
            {
                Console.WriteLine($"Expected {CharacterString}");
                var bytes = ParseEncodedRepresentation();
                var dicomStringDecoder = DicomStringDecoder.Get(SpecificCharacterSets);
                var result = dicomStringDecoder.Decode(VR, bytes);
                Console.WriteLine($"     Got {result}");
                if (result != CharacterString)
                {
                    Console.WriteLine($"         Failed");
                    throw new Exception("Actual result differs from expected result");
                }
                Console.WriteLine($"         OK");
            }
        }

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_H.3.html
        
        static readonly TestCase H31_Japanese_PN_Kanji = new TestCase
        {
            VR = DicomVR.PN,
            SpecificCharacterSets = new[] { "", "ISO 2022 IR 87" },
            CharacterString = "Yamada^Tarou=山田^太郎=やまだ^たろう",
            EncodedRepresentation = "05/09 06/01 06/13 06/01 06/04 06/01 5/14 05/04 06/01 07/02 06/15 07/05 03/13 01/11 02/04 04/02 03/11 03/03 04/05 04/04 01/11 02/08 04/02 05/14 01/11 02/04 04/02 04/02 04/00 04/15 03/10 01/11 02/08 04/02 03/13 01/11 02/04 04/02 02/04 06/04 02/04 05/14 02/04 04/00 01/11 02/08 04/02 05/14 01/11 02/04 04/02 02/04 03/15 02/04 06/13 02/04 02/06 01/11 02/08 04/02"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_H.3.2.html
        
        static readonly TestCase H32_Japanese_PN_Katakana_Kanji = new TestCase
        {
            VR = DicomVR.PN,
            SpecificCharacterSets = new[] { "ISO 2022 IR 13", "ISO 2022 IR 87" },
            CharacterString = "ヤマダ^タロウ=山田^太郎=やまだ^たろう",
            EncodedRepresentation = "13/04 12/15 12/00 13/14 05/14 12/00 13/11 11/03 03/13 01/11 02/04 04/02 03/11 03/03 04/05 04/04 01/11 02/08 04/10 05/14 01/11 02/04 04/02 04/02 04/00 04/15 03/10 01/11 02/08 04/10 03/13 01/11 02/04 04/02 02/04 06/04 02/04 05/14 02/04 04/00 01/11 02/08 04/10 05/14 01/11 02/04 04/02 02/04 03/15 02/04 06/13 02/04 02/06 01/11 02/08 04/10"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_I.2.html

        static readonly TestCase I2_Korean_PN_Hangule_Hanja = new TestCase
        {
            VR = DicomVR.PN,
            SpecificCharacterSets = new[] { "", "ISO 2022 IR 149" },
            CharacterString = "Hong^Gildong=洪^吉洞=홍^길동",
            EncodedRepresentation = "04/08 06/15 06/14 06/07 05/14 04/07 06/09 06/12 06/04 06/15 06/14 06/07 03/13 01/11 02/04 02/09 04/03 15/11 15/03 05/14 01/11 02/04 02/09 04/03 13/01 12/14 13/04 13/07 03/13 01/11 02/04 02/09 04/03 12/08 10/11 05/14 01/11 02/04 02/09 04/03 11/01 14/06 11/05 11/15"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/chapter_J.html

        static readonly TestCase J1_Chinese_PN_UTF8 = new TestCase
        {
            VR = DicomVR.PN,
            SpecificCharacterSets = new[] { "ISO_IR 192" },
            CharacterString = "Wang^XiaoDong=王^小東=",
            EncodedRepresentation = "0x57 0x61 0x6e 0x67 0x5e 0x58 0x69 0x61 0x6f 0x44 0x6f 0x6e 0x67 0x3d 0xe7 0x8e 0x8b 0x5e 0xe5 0xb0 0x8f 0xe6 0x9d 0xb1 0x3d"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_J.2.html
        
        static readonly TestCase J2_Chinese_LT_UTF8 = new TestCase
        {
            VR = DicomVR.LT,
            SpecificCharacterSets = new[] { "ISO_IR 192" },
            CharacterString = "The first line includes中文.\r\nThe second line includes中文, too.\r\nThe third line.\r\n",
            EncodedRepresentation = "0x54 0x68 0x65 0x20 0x66 0x69 0x72 0x73 0x74 0x20 0x6c 0x69 0x6e 0x65 0x20 0x69 0x6e 0x63 0x6c 0x75 0x64 0x65 0x73 0xe4 0xb8 0xad 0xe6 0x96 0x87 0x2e 0x0d 0x0a 0x54 0x68 0x65 0x20 0x73 0x65 0x63 0x6f 0x6e 0x64 0x20 0x6c 0x69 0x6e 0x65 0x20 0x69 0x6e 0x63 0x6c 0x75 0x64 0x65 0x73 0xe4 0xb8 0xad 0xe6 0x96 0x87 0x2c 0x20 0x74 0x6f 0x6f 0x2e 0x0d 0x0a 0x54 0x68 0x65 0x20 0x74 0x68 0x69 0x72 0x64 0x20 0x6c 0x69 0x6e 0x65 0x2e 0x0d 0x0a"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_J.3.html

        static readonly TestCase J3_Chinese_PN_GB18030 = new TestCase
        {
            VR = DicomVR.PN,
            SpecificCharacterSets = new[] { "GB18030" },
            CharacterString = "Wang^XiaoDong=王^小东=",
            EncodedRepresentation = "0x57 0x61 0x6e 0x67 0x5e 0x58 0x69 0x61 0x6f 0x44 0x6f 0x6e 0x67 0x3d 0xcd 0xf5 0x5e 0xd0 0xa1 0xb6 0xab 0x3d"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_J.2.html

        static readonly TestCase J4_Chinese_LT_GB18030 = new TestCase
        {
            VR = DicomVR.LT,
            SpecificCharacterSets = new[] { "GB18030" },
            CharacterString = "The first line includes中文.\r\nThe second line includes中文, too.\r\nThe third line.\r\n",
            EncodedRepresentation = "0x54 0x68 0x65 0x20 0x66 0x69 0x72 0x73 0x74 0x20 0x6c 0x69 0x6e 0x65 0x20 0x69 0x6e 0x63 0x6c 0x75 0x64 0x65 0x73 0xd6 0xd0 0xce 0xc4 0x2e 0x0d 0x0a 0x54 0x68 0x65 0x20 0x73 0x65 0x63 0x6f 0x6e 0x64 0x20 0x6c 0x69 0x6e 0x65 0x20 0x69 0x6e 0x63 0x6c 0x75 0x64 0x65 0x73 0xd6 0xd0 0xce 0xc4 0x2c 0x20 0x74 0x6f 0x6f 0x2e 0x0d 0x0a 0x54 0x68 0x65 0x20 0x74 0x68 0x69 0x72 0x64 0x20 0x6c 0x69 0x6e 0x65 0x2e 0x0d 0x0a"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_K.2.html
        
        static readonly TestCase K2_Chinese_PN_GB2312 = new TestCase
        {
            VR = DicomVR.PN,
            SpecificCharacterSets = new[] { "", "ISO 2022 IR 58" },
            CharacterString = "Zhang^XiaoDong=张^小东=",
            EncodedRepresentation = "0x5A 0x68 0x61 0x6E 0x67 0x5E 0x58 0x69 0x61 0x6F 0x44 0x6F 0x6E 0x67 0x3D 0x1B 0x24 0x29 0x41 0xD5 0xC5 0x5E 0x1B 0x24 0x29 0x41 0xD0 0xA1 0xB6 0xAB 0x3D"
        };

        // http://dicom.nema.org/medical/dicom/current/output/chtml/part05/sect_K.3.html

        static readonly TestCase K3_Chinese_LT_GB2312 = new TestCase
        {
            VR = DicomVR.LT,
            SpecificCharacterSets = new[] { "", "ISO 2022 IR 58" },
            CharacterString = "1.第一行文字。\r\n2.第二行文字。\r\n3.第三行文字。\r\n",
            EncodedRepresentation = "0x31 0x2e 0x1B 0x24 0x29 0x41 0xB5 0xDA 0xD2 0xBB 0xD0 0xD0 0xCE 0xC4 0xD7 0xD6 0xA1 0xA3 0x0D 0x0A 0x32 0x2e 0x1B 0x24 0x29 0x41 0xB5 0xDA 0xB6 0xFE 0xD0 0xD0 0xCE 0xC4 0xD7 0xD6 0xA1 0xA3 0x0D 0x0A 0x33 0x2e 0x1B 0x24 0x29 0x41 0xB5 0xDA 0xC8 0xFD 0xD0 0xD0 0xCE 0xC4 0xD7 0xD6 0xA1 0xA3 0x0D 0x0A"
        };

        public static void Run()
        {
            H31_Japanese_PN_Kanji.Run();
            H32_Japanese_PN_Katakana_Kanji.Run();
            I2_Korean_PN_Hangule_Hanja.Run();
            J1_Chinese_PN_UTF8.Run();
            J2_Chinese_LT_UTF8.Run();
            J3_Chinese_PN_GB18030.Run();
            J4_Chinese_LT_GB18030.Run();
            K2_Chinese_PN_GB2312.Run();
            K3_Chinese_LT_GB2312.Run();
        }
    }
}
