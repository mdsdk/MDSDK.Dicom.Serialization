using MDSDK.Dicom.Serialization.ValueRepresentations;
using System;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>Allows a property to be mapped to a DICOM attribute</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DicomAttributeAttribute : Attribute
    {
        internal DicomTag Tag { get; }

        internal ValueRepresentation VR { get; }

        /// <summary>Maps a property to a DICOM attribute</summary>
        public DicomAttributeAttribute(ushort groupNumber, ushort elementNumber, string vr)
        {
            Tag = new DicomTag(groupNumber, elementNumber);
            VR = DicomVR.Lookup(vr);
        }
    }
}
