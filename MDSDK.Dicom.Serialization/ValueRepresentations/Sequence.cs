// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    public sealed class Sequence<T> : ValueRepresentation, ISingleValue<List<T>>, IHas32BitExplicitVRLength, IMayHaveUndefinedLength
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

        public override string ToString(DicomStreamReader dicomStreamReader)
        {
            throw new NotImplementedException();
        }

        public void WriteValue(DicomStreamWriter writer, List<T> value)
        {
            throw new NotImplementedException();

            /*
            var items = new List<T>();
            using (var sequenceItemReader = reader.GetSequenceReader(_deserializer.Deserialize))
            {
                while (sequenceItemReader.MoveNext())
                {
                    items.Add(sequenceItemReader.Current);
                }
            }
            return items;
            */
        }
    }
}
