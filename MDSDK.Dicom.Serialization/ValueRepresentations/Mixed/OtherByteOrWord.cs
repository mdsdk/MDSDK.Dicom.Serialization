// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization.ValueRepresentations.Mixed
{
    internal sealed class OtherByteOrWord : ValueRepresentation, ISingleValue<Array>, IHas32BitExplicitVRLength, IMayHaveUndefinedLength, IMixedValueRepresentation
    {
        public OtherByteOrWord() : base("OB or OW") { }

        public Array ReadValue(DicomStreamReader reader)
        {
            return (reader.ExplicitVR == DicomVR.OB) ? DicomVR.OB.ReadValue(reader) : DicomVR.OW.ReadValue(reader); 
        }

        public void WriteValue(DicomStreamWriter writer, Array value)
        {
            if (value is byte[] bytes)
            {
                DicomVR.OB.WriteValue(writer, bytes);
            }
            else if (value is ushort[] words)
            {
                DicomVR.OW.WriteValue(writer, words);
            }
            else
            {
                throw new ArgumentException("value is not byte[] or ushort[]");
            }
        }

        internal override object GetValue(DicomStreamReader reader) => ReadValue(reader);

        public ValueRepresentation DefaultVR => DicomVR.OW;
    }
}
