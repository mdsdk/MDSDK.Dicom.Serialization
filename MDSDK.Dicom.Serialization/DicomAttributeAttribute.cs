using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;

namespace MDSDK.Dicom.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DicomAttributeAttribute : Attribute
    {
        public DicomTag Tag { get; }

        public ValueRepresentation VR { get; }

        public DicomAttributeAttribute(ushort groupNumber, ushort elementNumber, string vr)
        {
            Tag = new DicomTag(groupNumber, elementNumber);
            VR = DicomVR.Lookup(((byte)vr[0], (byte)vr[1]));
        }
    }
}
