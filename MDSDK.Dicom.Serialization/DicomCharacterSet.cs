// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Text;

namespace MDSDK.Dicom.Serialization
{
    public static class DicomCharacterSet
    {
        private static NotSupportedException NotSupported(params string[] specificCharacterSet)
        {
            return new NotSupportedException($"DICOM character set {string.Join('\\', specificCharacterSet)} is not supported");
        }

        private static Encoding GetEncoding(string specificCharacterSet)
        {
            try
            {
                return specificCharacterSet switch
                {
                    "" => Encoding.ASCII,                                     // Default repertoire
                    "ISO_IR 100" => Encoding.GetEncoding("iso-8859-1"),       // Latin 1
                    "ISO_IR 101" => Encoding.GetEncoding("iso-8859-2"),       // Latin 2
                    "ISO_IR 109" => Encoding.GetEncoding("iso-8859-3"),       // Latin 3
                    "ISO_IR 110" => Encoding.GetEncoding("iso-8859-4"),       // Latin 4
                    "ISO_IR 144" => Encoding.GetEncoding("iso-8859-5"),       // Cyrillic
                    "ISO_IR 127" => Encoding.GetEncoding("iso-8859-6"),       // Arabic
                    "ISO_IR 126" => Encoding.GetEncoding("iso-8859-7"),       // Greek
                    "ISO_IR 138" => Encoding.GetEncoding("iso-8859-8"),       // Hebrew
                    "ISO_IR 148" => Encoding.GetEncoding("iso-8859-9"),       // Latin 5
                    "ISO_IR 13" => Encoding.GetEncoding("iso-2022-jp"),       // Japanese
                    "ISO_IR 166" => Encoding.GetEncoding("windows-874"),      // Thai
                    "ISO 2022 IR 6" => Encoding.ASCII,                        // Default repertoire
                    "ISO 2022 IR 100" => Encoding.GetEncoding("iso-8859-1"),  // Latin 1
                    "ISO 2022 IR 101" => Encoding.GetEncoding("iso-8859-2"),  // Latin 2
                    "ISO 2022 IR 109" => Encoding.GetEncoding("iso-8859-3"),  // Latin 3
                    "ISO 2022 IR 110" => Encoding.GetEncoding("iso-8859-4"),  // Latin 4
                    "ISO 2022 IR 144" => Encoding.GetEncoding("iso-8859-5"),  // Cyrillic
                    "ISO 2022 IR 127" => Encoding.GetEncoding("iso-8859-6"),  // Arabic
                    "ISO 2022 IR 126" => Encoding.GetEncoding("iso-8859-7"),  // Greek
                    "ISO 2022 IR 138" => Encoding.GetEncoding("iso-8859-8"),  // Hebrew
                    "ISO 2022 IR 148" => Encoding.GetEncoding("iso-8859-9"),  // Latin 5
                    "ISO 2022 IR 13" => Encoding.GetEncoding("iso-2022-jp"),  // Japanese
                    "ISO 2022 IR 166" => Encoding.GetEncoding("windows-874"), // Thai
                    "ISO 2022 IR 87" => Encoding.GetEncoding("iso-2022-jp"),  // Japanese
                    "ISO 2022 IR 159" => Encoding.GetEncoding("iso-2022-jp"), // Japanese
                    "ISO 2022 IR 149" => Encoding.GetEncoding("iso-2022-kr"), // Korean
                    "ISO 2022 IR 58" => Encoding.GetEncoding("gb2312"),       // Simplified Chinese
                    "ISO_IR 192" => Encoding.UTF8,                            // UTF-8
                    "GB18030" => Encoding.GetEncoding("gb18030"),             // GB18030
                    "GBK" => Encoding.GetEncoding("gb18030"),                 // One- and two-byte subset of GB18030
                    _ => throw NotSupported(specificCharacterSet)
                };
            }
            catch (ArgumentException error)
            {
                throw new NotSupportedException(error.Message
                    + ". Ensure to register System.Text.CodePagesEncodingProvider from System.Text.Encoding.CodePages.dll");
            }
        }

        public static Encoding GetEncoding(string[] specificCharacterSet)
        {
            if ((specificCharacterSet == null) || (specificCharacterSet.Length == 0))
            {
                return Encoding.ASCII;
            }
            else if (specificCharacterSet.Length == 1)
            {
                return GetEncoding(specificCharacterSet[0]);
            }
            else
            {
                throw NotSupported(specificCharacterSet);
            }
        }
    }
}
