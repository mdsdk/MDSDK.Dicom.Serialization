// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.Serialization.ValueRepresentations.Mixed
{
    public class UnsignedOrSignedShort : ValueRepresentation, IMultiValue<int>, IHasLightWeightValueLengthCalculation<int>, IHas16BitExplicitVRLength, IMixedValueRepresentation
    {
        public UnsignedOrSignedShort() : base("US or SS") { }

        private IMultiValue<int> US_or_SS(DicomStreamReader reader) => (reader.ExplicitVR is SignedShort) ? DicomVR.SS : DicomVR.US;

        public int[] ReadValues(DicomStreamReader reader) => US_or_SS(reader).ReadValues(reader);

        public int ReadSingleValue(DicomStreamReader reader) => US_or_SS(reader).ReadSingleValue(reader);

        private IMultiValue<int> US_or_SS(DicomStreamWriter writer) => DicomVR.US;

        public void WriteValues(DicomStreamWriter writer, int[] values) => US_or_SS(writer).WriteValues(writer, values);

        public void WriteSingleValue(DicomStreamWriter writer, int value) => US_or_SS(writer).WriteSingleValue(writer, value);

        public long GetUnpaddedValueLength(int value) => 2;

        public long GetUnpaddedValueLength(int[] values) => 2 * values.Length;

        internal override object GetValue(DicomStreamReader reader) => ReadValues(reader);

        public ValueRepresentation DefaultVR => DicomVR.US;
    }
}
