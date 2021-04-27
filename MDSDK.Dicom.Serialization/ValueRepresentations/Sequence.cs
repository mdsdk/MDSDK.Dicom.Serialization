// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal sealed class Sequence<T> : ValueRepresentation, ISingleValue<List<T>>, IHas32BitExplicitVRLength, IMayHaveUndefinedLength
        where T : new()
    {
        private readonly DicomSerializer<T> _serializer;

        public Sequence() : base("SQ")
        {
            _serializer = DicomSerializer.GetSerializer<T>();
        }

        public List<T> ReadValue(DicomStreamReader reader)
        {
            var items = new List<T>();
            reader.ReadSequenceItems(_serializer.Deserialize, items.Add);
            return items;
        }

        internal override object GetValue(DicomStreamReader reader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(DicomStreamWriter writer, List<T> value)
        {
            using (var itemEnumerator = value.GetEnumerator())
            {
                writer.WriteSequenceItems(_serializer.Serialize, itemEnumerator);
            }
        }
    }
}
