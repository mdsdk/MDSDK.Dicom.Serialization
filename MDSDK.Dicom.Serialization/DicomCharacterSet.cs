// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Text;

namespace MDSDK.Dicom.Serialization
{
    public static class DicomCharacterSet
    {
        public static Encoding GetEncoding(string specificCharacterSet)
        {
            switch (specificCharacterSet)
            {
                case "ISO_IR 100":
                    return Encoding.GetEncoding("iso-8859-1");
                case "ISO_IR 192":
                    return Encoding.GetEncoding("utf-8");
                default:
                    throw new NotSupportedException($"DICOM character set {specificCharacterSet} is not supported");
            }
        }
    }
}
