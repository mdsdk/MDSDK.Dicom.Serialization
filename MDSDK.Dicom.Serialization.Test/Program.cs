// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Text;

namespace MDSDK.Dicom.Serialization.Test
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            DicomStringDecoderTest.Run();
            DicomSerializerTest.Run();
        }
    }
}
