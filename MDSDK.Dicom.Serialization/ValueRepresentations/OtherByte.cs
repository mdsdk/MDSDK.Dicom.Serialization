// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public class OtherByte : OtherBinaryEncodedPrimitiveValue<byte>
    {
        public OtherByte() : base("OB") { }

        public override byte[] ReadValue(DicomStreamReader reader)
        {
            if (reader.ValueLength < uint.MaxValue)
            {
                var result = reader.Input.ReadBytes(reader.ValueLength);
                reader.EndReadValue();
                return result;
            }
            else
            {
                var chunks = new List<byte[]>();

                while (reader.TryReadItemTagOfSequenceWithUndefinedLength())
                {
                    if (reader.ValueLength < uint.MaxValue)
                    {
                        var chunk = reader.Input.ReadBytes(reader.ValueLength);
                        reader.EndReadValue();
                        chunks.Add(chunk);
                    }
                    else
                    {
                        throw new NotSupportedException("Cannot read chunks with undefined length");
                    }
                }

                var totalLength = chunks.Sum(chunk => chunk.LongLength);
                var byteArray = new byte[totalLength];
                Span<byte> copyWindow = byteArray;
                foreach (var chunk in chunks)
                {
                    chunk.CopyTo(copyWindow);
                    copyWindow = copyWindow.Slice(chunk.Length);
                }
                return byteArray;
            }
        }
    }
}
