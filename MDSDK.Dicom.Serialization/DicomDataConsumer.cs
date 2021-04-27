// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.Dicom.Serialization
{
    /// <summary>Return value of <see cref="DicomDataConsumer{TDataSet}.GetOptions(DicomTag, string)"/></summary>
    [Flags]
    public enum DicomDataConsumptionOptions 
    {
        /// <summary>Skip the attribute</summary>
        Skip = 1,
        /// <summary>Return the raw value of the attribute without converting to its value representation</summary>
        RawValue = Skip << 1 
    };

    /// <summary>Base class for consuming DICOM data into a generic data set</summary>
    public abstract class DicomDataConsumer<TDataSet>
    {
        /// <summary>Interface for consuming sequence items</summary>
        public interface ISequenceItemConsumer : IDisposable
        {
            /// <summary>Adds an item to the sequence</summary>
            void AddItem(TDataSet item);
        }

        /// <summary>Returns options that control data consumption</summary>
        public virtual DicomDataConsumptionOptions GetOptions(DicomTag tag, string vrs) => tag.IsPrivate ? DicomDataConsumptionOptions.Skip : default;

        /// <summary>Called when an attribute with unknown value representation is skipped</summary>
        public virtual void SkippedValueWithUnknownVR(TDataSet dataSet, DicomTag tag, DicomAttribute attribute) { }

        /// <summary>Called when a non-sequence attribute with undefined length is skipped</summary>
        public virtual void SkippedValueWithUndefinedLength(TDataSet dataSet, DicomTag tag, DicomAttribute attribute) { }

        /// <summary>Called to consume an attribute and its value</summary>
        public abstract void ConsumeValue(TDataSet dataSet, DicomTag tag, DicomAttribute attribute, object value);

        /// <summary>Called to consume an sequence attribute</summary>
        public abstract ISequenceItemConsumer CreateSequenceItemConsumer(TDataSet dataSet, DicomTag tag, DicomAttribute attribute);

        /// <summary>Called to create a data set for a sequence item</summary>
        public abstract TDataSet CreateItem();

        /// <inheritdoc/>
        public virtual void Dipose() { }
    }
}
