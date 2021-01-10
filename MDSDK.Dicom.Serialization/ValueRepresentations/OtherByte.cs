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
                return reader.Input.ReadBytes(reader.ValueLength);
            }
            else
            {
                var chunks = new List<byte[]>();

                while (reader.TryReadItemTagOfSequenceWithUndefinedLength())
                {
                    if (reader.ValueLength < uint.MaxValue)
                    {
                        chunks.Add(reader.Input.ReadBytes(reader.ValueLength));
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

        public override string ToString(DicomStreamReader reader)
        {
            var value = ReadValue(reader);
            return (value.Length > 256) ? Convert.ToBase64String(value) : Convert.ToHexString(value);
        }
    }
}
