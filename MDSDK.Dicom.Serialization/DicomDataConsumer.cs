// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization
{
    public abstract class DicomDataConsumer<TDataSet>
    {
        public interface ISequenceItemConsumer : IDisposable
        {
            void AddItem(TDataSet item);
        }

        public virtual void SkippedValueWithUnknownVR(TDataSet dataSet, DicomTag tag, DicomAttribute attribute) { }

        public virtual void SkippedValueWithUndefinedLength(TDataSet dataSet, DicomTag tag, DicomAttribute attribute) { }

        public abstract void ConsumeValue(TDataSet dataSet, DicomTag tag, DicomAttribute attribute, object value);

        public abstract ISequenceItemConsumer CreateSequenceItemConsumer(TDataSet dataSet, DicomTag tag, DicomAttribute attribute);

        public abstract TDataSet CreateItem();

        public virtual void Dipose() { }
    }
}
