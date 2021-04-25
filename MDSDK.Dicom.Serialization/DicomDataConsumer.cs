// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization
{
    [Flags]
    public enum DicomTagConsumptionOptions { Skip = 1, RawValue = Skip << 1 };

    public abstract class DicomDataConsumer<TDataSet>
    {
        public interface ISequenceItemConsumer : IDisposable
        {
            void AddItem(TDataSet item);
        }

        public virtual DicomTagConsumptionOptions GetOptions(DicomTag tag) => tag.IsPrivate ? DicomTagConsumptionOptions.Skip : default;

        public virtual void SkippedValueWithUnknownVR(TDataSet dataSet, DicomTag tag, DicomAttribute attribute) { }

        public virtual void SkippedValueWithUndefinedLength(TDataSet dataSet, DicomTag tag, DicomAttribute attribute) { }

        public abstract void ConsumeValue(TDataSet dataSet, DicomTag tag, DicomAttribute attribute, object value);

        public abstract ISequenceItemConsumer CreateSequenceItemConsumer(TDataSet dataSet, DicomTag tag, DicomAttribute attribute);

        public abstract TDataSet CreateItem();

        public virtual void Dipose() { }
    }
}
