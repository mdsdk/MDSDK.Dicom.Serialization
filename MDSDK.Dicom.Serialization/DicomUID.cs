// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;

namespace MDSDK.Dicom.Serialization
{
    public readonly struct DicomUID : IEquatable<DicomUID>
    {
        private static readonly Dictionary<string, string> s_names = new();

        public string UID { get; }

        private DicomUID(string uid, string name)
        {
            UID = uid;
            s_names.Add(uid, name);
        }

        public string Name => ((UID != null) && s_names.TryGetValue(UID, out string name)) ? name : "<Unknown>";

        public override string ToString() => (UID == null) ? "null" : $"{Name} ({UID})";

        public DicomUID(string uid)
        {
            UID = uid;
        }

        public bool Equals(DicomUID other) => UID == other.UID;

        public override bool Equals(object obj) => (obj is DicomUID other) && Equals(other);

        public override int GetHashCode() => UID.GetHashCode();

        public static bool operator ==(DicomUID a, DicomUID b) => a.UID == b.UID;

        public static bool operator !=(DicomUID a, DicomUID b) => a.UID != b.UID;

        internal static class Retired
        {
            public static readonly DicomUID ExplicitVRBigEndian = new DicomUID("1.2.840.10008.1.​2.​2", "Explicit VR Big Endian (Retired)");
        }

        #region SOP Class

            public static readonly DicomUID VerificationSOPClass = new DicomUID("1.2.840.10008.1.1", "Verification SOP Class");
        public static readonly DicomUID MediaStorageDirectoryStorage = new DicomUID("1.2.840.10008.1.3.10", "Media Storage Directory Storage");
        public static readonly DicomUID StorageCommitmentPushModelSOPClass = new DicomUID("1.2.840.10008.1.20.1", "Storage Commitment Push Model SOP Class");
        public static readonly DicomUID ProceduralEventLoggingSOPClass = new DicomUID("1.2.840.10008.1.40", "Procedural Event Logging SOP Class");
        public static readonly DicomUID SubstanceAdministrationLoggingSOPClass = new DicomUID("1.2.840.10008.1.42", "Substance Administration Logging SOP Class");
        public static readonly DicomUID ModalityPerformedProcedureStepSOPClass = new DicomUID("1.2.840.10008.3.1.2.3.3", "Modality Performed Procedure Step SOP Class");
        public static readonly DicomUID ModalityPerformedProcedureStepRetrieveSOPClass = new DicomUID("1.2.840.10008.3.1.2.3.4", "Modality Performed Procedure Step Retrieve SOP Class");
        public static readonly DicomUID ModalityPerformedProcedureStepNotificationSOPClass = new DicomUID("1.2.840.10008.3.1.2.3.5", "Modality Performed Procedure Step Notification SOP Class");
        public static readonly DicomUID BasicFilmSessionSOPClass = new DicomUID("1.2.840.10008.5.1.1.1", "Basic Film Session SOP Class");
        public static readonly DicomUID BasicFilmBoxSOPClass = new DicomUID("1.2.840.10008.5.1.1.2", "Basic Film Box SOP Class");
        public static readonly DicomUID BasicGrayscaleImageBoxSOPClass = new DicomUID("1.2.840.10008.5.1.1.4", "Basic Grayscale Image Box SOP Class");
        public static readonly DicomUID BasicColorImageBoxSOPClass = new DicomUID("1.2.840.10008.5.1.1.4.1", "Basic Color Image Box SOP Class");
        public static readonly DicomUID PrintJobSOPClass = new DicomUID("1.2.840.10008.5.1.1.14", "Print Job SOP Class");
        public static readonly DicomUID BasicAnnotationBoxSOPClass = new DicomUID("1.2.840.10008.5.1.1.15", "Basic Annotation Box SOP Class");
        public static readonly DicomUID PrinterSOPClass = new DicomUID("1.2.840.10008.5.1.1.16", "Printer SOP Class");
        public static readonly DicomUID PrinterConfigurationRetrievalSOPClass = new DicomUID("1.2.840.10008.5.1.1.16.376", "Printer Configuration Retrieval SOP Class");
        public static readonly DicomUID VOILUTBoxSOPClass = new DicomUID("1.2.840.10008.5.1.1.22", "VOI LUT Box SOP Class");
        public static readonly DicomUID PresentationLUTSOPClass = new DicomUID("1.2.840.10008.5.1.1.23", "Presentation LUT SOP Class");
        public static readonly DicomUID MediaCreationManagementSOPClassUID = new DicomUID("1.2.840.10008.5.1.1.33", "Media Creation Management SOP Class UID");
        public static readonly DicomUID DisplaySystemSOPClass = new DicomUID("1.2.840.10008.5.1.1.40", "Display System SOP Class");
        public static readonly DicomUID ComputedRadiographyImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.1", "Computed Radiography Image Storage");
        public static readonly DicomUID DigitalXRayImageStorageForPresentation = new DicomUID("1.2.840.10008.5.1.4.1.1.1.1", "Digital X-Ray Image Storage - For Presentation");
        public static readonly DicomUID DigitalXRayImageStorageForProcessing = new DicomUID("1.2.840.10008.5.1.4.1.1.1.1.1", "Digital X-Ray Image Storage - For Processing");
        public static readonly DicomUID DigitalMammographyXRayImageStorageForPresentation = new DicomUID("1.2.840.10008.5.1.4.1.1.1.2", "Digital Mammography X-Ray Image Storage - For Presentation");
        public static readonly DicomUID DigitalMammographyXRayImageStorageForProcessing = new DicomUID("1.2.840.10008.5.1.4.1.1.1.2.1", "Digital Mammography X-Ray Image Storage - For Processing");
        public static readonly DicomUID DigitalIntraOralXRayImageStorageForPresentation = new DicomUID("1.2.840.10008.5.1.4.1.1.1.3", "Digital Intra-Oral X-Ray Image Storage - For Presentation");
        public static readonly DicomUID DigitalIntraOralXRayImageStorageForProcessing = new DicomUID("1.2.840.10008.5.1.4.1.1.1.3.1", "Digital Intra-Oral X-Ray Image Storage - For Processing");
        public static readonly DicomUID CTImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.2", "CT Image Storage");
        public static readonly DicomUID EnhancedCTImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.2.1", "Enhanced CT Image Storage");
        public static readonly DicomUID LegacyConvertedEnhancedCTImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.2.2", "Legacy Converted Enhanced CT Image Storage");
        public static readonly DicomUID UltrasoundMultiFrameImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.3.1", "Ultrasound Multi-frame Image Storage");
        public static readonly DicomUID MRImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.4", "MR Image Storage");
        public static readonly DicomUID EnhancedMRImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.4.1", "Enhanced MR Image Storage");
        public static readonly DicomUID MRSpectroscopyStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.4.2", "MR Spectroscopy Storage");
        public static readonly DicomUID EnhancedMRColorImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.4.3", "Enhanced MR Color Image Storage");
        public static readonly DicomUID LegacyConvertedEnhancedMRImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.4.4", "Legacy Converted Enhanced MR Image Storage");
        public static readonly DicomUID UltrasoundImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.6.1", "Ultrasound Image Storage");
        public static readonly DicomUID EnhancedUSVolumeStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.6.2", "Enhanced US Volume Storage");
        public static readonly DicomUID SecondaryCaptureImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.7", "Secondary Capture Image Storage");
        public static readonly DicomUID MultiFrameSingleBitSecondaryCaptureImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.7.1", "Multi-frame Single Bit Secondary Capture Image Storage");
        public static readonly DicomUID MultiFrameGrayscaleByteSecondaryCaptureImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.7.2", "Multi-frame Grayscale Byte Secondary Capture Image Storage");
        public static readonly DicomUID MultiFrameGrayscaleWordSecondaryCaptureImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.7.3", "Multi-frame Grayscale Word Secondary Capture Image Storage");
        public static readonly DicomUID MultiFrameTrueColorSecondaryCaptureImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.7.4", "Multi-frame True Color Secondary Capture Image Storage");
        public static readonly DicomUID _12LeadECGWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.1.1", "12-lead ECG Waveform Storage");
        public static readonly DicomUID GeneralECGWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.1.2", "General ECG Waveform Storage");
        public static readonly DicomUID AmbulatoryECGWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.1.3", "Ambulatory ECG Waveform Storage");
        public static readonly DicomUID HemodynamicWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.2.1", "Hemodynamic Waveform Storage");
        public static readonly DicomUID CardiacElectrophysiologyWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.3.1", "Cardiac Electrophysiology Waveform Storage");
        public static readonly DicomUID BasicVoiceAudioWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.4.1", "Basic Voice Audio Waveform Storage");
        public static readonly DicomUID GeneralAudioWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.4.2", "General Audio Waveform Storage");
        public static readonly DicomUID ArterialPulseWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.5.1", "Arterial Pulse Waveform Storage");
        public static readonly DicomUID RespiratoryWaveformStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.9.6.1", "Respiratory Waveform Storage");
        public static readonly DicomUID GrayscaleSoftcopyPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.1", "Grayscale Softcopy Presentation State Storage");
        public static readonly DicomUID ColorSoftcopyPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.2", "Color Softcopy Presentation State Storage");
        public static readonly DicomUID PseudoColorSoftcopyPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.3", "Pseudo-Color Softcopy Presentation State Storage");
        public static readonly DicomUID BlendingSoftcopyPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.4", "Blending Softcopy Presentation State Storage");
        public static readonly DicomUID XAXRFGrayscaleSoftcopyPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.5", "XA/XRF Grayscale Softcopy Presentation State Storage");
        public static readonly DicomUID GrayscalePlanarMPRVolumetricPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.6", "Grayscale Planar MPR Volumetric Presentation State Storage");
        public static readonly DicomUID CompositingPlanarMPRVolumetricPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.7", "Compositing Planar MPR Volumetric Presentation State Storage");
        public static readonly DicomUID AdvancedBlendingPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.8", "Advanced Blending Presentation State Storage");
        public static readonly DicomUID VolumeRenderingVolumetricPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.9", "Volume Rendering Volumetric Presentation State Storage");
        public static readonly DicomUID SegmentedVolumeRenderingVolumetricPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.10", "Segmented Volume Rendering Volumetric Presentation State Storage");
        public static readonly DicomUID MultipleVolumeRenderingVolumetricPresentationStateStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.11.11", "Multiple Volume Rendering Volumetric Presentation State Storage");
        public static readonly DicomUID XRayAngiographicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.12.1", "X-Ray Angiographic Image Storage");
        public static readonly DicomUID EnhancedXAImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.12.1.1", "Enhanced XA Image Storage");
        public static readonly DicomUID XRayRadiofluoroscopicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.12.2", "X-Ray Radiofluoroscopic Image Storage");
        public static readonly DicomUID EnhancedXRFImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.12.2.1", "Enhanced XRF Image Storage");
        public static readonly DicomUID XRay3DAngiographicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.13.1.1", "X-Ray 3D Angiographic Image Storage");
        public static readonly DicomUID XRay3DCraniofacialImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.13.1.2", "X-Ray 3D Craniofacial Image Storage");
        public static readonly DicomUID BreastTomosynthesisImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.13.1.3", "Breast Tomosynthesis Image Storage");
        public static readonly DicomUID BreastProjectionXRayImageStorageForPresentation = new DicomUID("1.2.840.10008.5.1.4.1.1.13.1.4", "Breast Projection X-Ray Image Storage - For Presentation");
        public static readonly DicomUID BreastProjectionXRayImageStorageForProcessing = new DicomUID("1.2.840.10008.5.1.4.1.1.13.1.5", "Breast Projection X-Ray Image Storage - For Processing");
        public static readonly DicomUID IntravascularOpticalCoherenceTomographyImageStorageForPresentation = new DicomUID("1.2.840.10008.5.1.4.1.1.14.1", "Intravascular Optical Coherence Tomography Image Storage - For Presentation");
        public static readonly DicomUID IntravascularOpticalCoherenceTomographyImageStorageForProcessing = new DicomUID("1.2.840.10008.5.1.4.1.1.14.2", "Intravascular Optical Coherence Tomography Image Storage - For Processing");
        public static readonly DicomUID NuclearMedicineImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.20", "Nuclear Medicine Image Storage");
        public static readonly DicomUID ParametricMapStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.30", "Parametric Map Storage");
        public static readonly DicomUID RawDataStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66", "Raw Data Storage");
        public static readonly DicomUID SpatialRegistrationStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66.1", "Spatial Registration Storage");
        public static readonly DicomUID SpatialFiducialsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66.2", "Spatial Fiducials Storage");
        public static readonly DicomUID DeformableSpatialRegistrationStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66.3", "Deformable Spatial Registration Storage");
        public static readonly DicomUID SegmentationStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66.4", "Segmentation Storage");
        public static readonly DicomUID SurfaceSegmentationStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66.5", "Surface Segmentation Storage");
        public static readonly DicomUID TractographyResultsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.66.6", "Tractography Results Storage");
        public static readonly DicomUID RealWorldValueMappingStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.67", "Real World Value Mapping Storage");
        public static readonly DicomUID SurfaceScanMeshStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.68.1", "Surface Scan Mesh Storage");
        public static readonly DicomUID SurfaceScanPointCloudStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.68.2", "Surface Scan Point Cloud Storage");
        public static readonly DicomUID VLEndoscopicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.1", "VL Endoscopic Image Storage");
        public static readonly DicomUID VideoEndoscopicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.1.1", "Video Endoscopic Image Storage");
        public static readonly DicomUID VLMicroscopicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.2", "VL Microscopic Image Storage");
        public static readonly DicomUID VideoMicroscopicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.2.1", "Video Microscopic Image Storage");
        public static readonly DicomUID VLSlideCoordinatesMicroscopicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.3", "VL Slide-Coordinates Microscopic Image Storage");
        public static readonly DicomUID VLPhotographicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.4", "VL Photographic Image Storage");
        public static readonly DicomUID VideoPhotographicImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.4.1", "Video Photographic Image Storage");
        public static readonly DicomUID OphthalmicPhotography8BitImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.1", "Ophthalmic Photography 8 Bit Image Storage");
        public static readonly DicomUID OphthalmicPhotography16BitImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.2", "Ophthalmic Photography 16 Bit Image Storage");
        public static readonly DicomUID StereometricRelationshipStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.3", "Stereometric Relationship Storage");
        public static readonly DicomUID OphthalmicTomographyImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.4", "Ophthalmic Tomography Image Storage");
        public static readonly DicomUID WideFieldOphthalmicPhotographyStereographicProjectionImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.5", "Wide Field Ophthalmic Photography Stereographic Projection Image Storage");
        public static readonly DicomUID WideFieldOphthalmicPhotography3DCoordinatesImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.6", "Wide Field Ophthalmic Photography 3D Coordinates Image Storage");
        public static readonly DicomUID OphthalmicOpticalCoherenceTomographyEnFaceImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.7", "Ophthalmic Optical Coherence Tomography En Face Image Storage");
        public static readonly DicomUID OphthalmicOpticalCoherenceTomographyBScanVolumeAnalysisStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.5.8", "Ophthalmic Optical Coherence Tomography B-scan Volume Analysis Storage");
        public static readonly DicomUID VLWholeSlideMicroscopyImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.77.1.6", "VL Whole Slide Microscopy Image Storage");
        public static readonly DicomUID LensometryMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.1", "Lensometry Measurements Storage");
        public static readonly DicomUID AutorefractionMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.2", "Autorefraction Measurements Storage");
        public static readonly DicomUID KeratometryMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.3", "Keratometry Measurements Storage");
        public static readonly DicomUID SubjectiveRefractionMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.4", "Subjective Refraction Measurements Storage");
        public static readonly DicomUID VisualAcuityMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.5", "Visual Acuity Measurements Storage");
        public static readonly DicomUID SpectaclePrescriptionReportStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.6", "Spectacle Prescription Report Storage");
        public static readonly DicomUID OphthalmicAxialMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.7", "Ophthalmic Axial Measurements Storage");
        public static readonly DicomUID IntraocularLensCalculationsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.78.8", "Intraocular Lens Calculations Storage");
        public static readonly DicomUID MacularGridThicknessAndVolumeReportStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.79.1", "Macular Grid Thickness and Volume Report Storage");
        public static readonly DicomUID OphthalmicVisualFieldStaticPerimetryMeasurementsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.80.1", "Ophthalmic Visual Field Static Perimetry Measurements Storage");
        public static readonly DicomUID OphthalmicThicknessMapStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.81.1", "Ophthalmic Thickness Map Storage");
        public static readonly DicomUID CornealTopographyMapStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.82.1", "Corneal Topography Map Storage");
        public static readonly DicomUID BasicTextSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.11", "Basic Text SR Storage");
        public static readonly DicomUID EnhancedSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.22", "Enhanced SR Storage");
        public static readonly DicomUID ComprehensiveSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.33", "Comprehensive SR Storage");
        public static readonly DicomUID Comprehensive3DSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.34", "Comprehensive 3D SR Storage");
        public static readonly DicomUID ExtensibleSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.35", "Extensible SR Storage");
        public static readonly DicomUID ProcedureLogStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.40", "Procedure Log Storage");
        public static readonly DicomUID MammographyCADSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.50", "Mammography CAD SR Storage");
        public static readonly DicomUID KeyObjectSelectionDocumentStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.59", "Key Object Selection Document Storage");
        public static readonly DicomUID ChestCADSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.65", "Chest CAD SR Storage");
        public static readonly DicomUID XRayRadiationDoseSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.67", "X-Ray Radiation Dose SR Storage");
        public static readonly DicomUID RadiopharmaceuticalRadiationDoseSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.68", "Radiopharmaceutical Radiation Dose SR Storage");
        public static readonly DicomUID ColonCADSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.69", "Colon CAD SR Storage");
        public static readonly DicomUID ImplantationPlanSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.70", "Implantation Plan SR Storage");
        public static readonly DicomUID AcquisitionContextSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.71", "Acquisition Context SR Storage");
        public static readonly DicomUID SimplifiedAdultEchoSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.72", "Simplified Adult Echo SR Storage");
        public static readonly DicomUID PatientRadiationDoseSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.73", "Patient Radiation Dose SR Storage");
        public static readonly DicomUID PlannedImagingAgentAdministrationSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.74", "Planned Imaging Agent Administration SR Storage");
        public static readonly DicomUID PerformedImagingAgentAdministrationSRStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.88.75", "Performed Imaging Agent Administration SR Storage");
        public static readonly DicomUID ContentAssessmentResultsStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.90.1", "Content Assessment Results Storage");
        public static readonly DicomUID EncapsulatedPDFStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.104.1", "Encapsulated PDF Storage");
        public static readonly DicomUID EncapsulatedCDAStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.104.2", "Encapsulated CDA Storage");
        public static readonly DicomUID EncapsulatedSTLStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.104.3", "Encapsulated STL Storage");
        public static readonly DicomUID PositronEmissionTomographyImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.128", "Positron Emission Tomography Image Storage");
        public static readonly DicomUID LegacyConvertedEnhancedPETImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.128.1", "Legacy Converted Enhanced PET Image Storage");
        public static readonly DicomUID EnhancedPETImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.130", "Enhanced PET Image Storage");
        public static readonly DicomUID BasicStructuredDisplayStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.131", "Basic Structured Display Storage");
        public static readonly DicomUID CTDefinedProcedureProtocolStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.200.1", "CT Defined Procedure Protocol Storage");
        public static readonly DicomUID CTPerformedProcedureProtocolStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.200.2", "CT Performed Procedure Protocol Storage");
        public static readonly DicomUID ProtocolApprovalStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.200.3", "Protocol Approval Storage");
        public static readonly DicomUID ProtocolApprovalInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.1.1.200.4", "Protocol Approval Information Model - FIND");
        public static readonly DicomUID ProtocolApprovalInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.1.1.200.5", "Protocol Approval Information Model - MOVE");
        public static readonly DicomUID ProtocolApprovalInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.1.1.200.6", "Protocol Approval Information Model - GET");
        public static readonly DicomUID RTImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.1", "RT Image Storage");
        public static readonly DicomUID RTDoseStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.2", "RT Dose Storage");
        public static readonly DicomUID RTStructureSetStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.3", "RT Structure Set Storage");
        public static readonly DicomUID RTBeamsTreatmentRecordStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.4", "RT Beams Treatment Record Storage");
        public static readonly DicomUID RTPlanStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.5", "RT Plan Storage");
        public static readonly DicomUID RTBrachyTreatmentRecordStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.6", "RT Brachy Treatment Record Storage");
        public static readonly DicomUID RTTreatmentSummaryRecordStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.7", "RT Treatment Summary Record Storage");
        public static readonly DicomUID RTIonPlanStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.8", "RT Ion Plan Storage");
        public static readonly DicomUID RTIonBeamsTreatmentRecordStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.9", "RT Ion Beams Treatment Record Storage");
        public static readonly DicomUID RTPhysicianIntentStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.10", "RT Physician Intent Storage");
        public static readonly DicomUID RTSegmentAnnotationStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.11", "RT Segment Annotation Storage");
        public static readonly DicomUID RTRadiationSetStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.12", "RT Radiation Set Storage");
        public static readonly DicomUID CArmPhotonElectronRadiationStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.481.13", "C-Arm Photon-Electron Radiation Storage");
        public static readonly DicomUID DICOSCTImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.501.1", "DICOS CT Image Storage");
        public static readonly DicomUID DICOSDigitalXRayImageStorageForPresentation = new DicomUID("1.2.840.10008.5.1.4.1.1.501.2.1", "DICOS Digital X-Ray Image Storage - For Presentation");
        public static readonly DicomUID DICOSDigitalXRayImageStorageForProcessing = new DicomUID("1.2.840.10008.5.1.4.1.1.501.2.2", "DICOS Digital X-Ray Image Storage - For Processing");
        public static readonly DicomUID DICOSThreatDetectionReportStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.501.3", "DICOS Threat Detection Report Storage");
        public static readonly DicomUID DICOS2DAITStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.501.4", "DICOS 2D AIT Storage");
        public static readonly DicomUID DICOS3DAITStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.501.5", "DICOS 3D AIT Storage");
        public static readonly DicomUID DICOSQuadrupoleResonance_QR_Storage = new DicomUID("1.2.840.10008.5.1.4.1.1.501.6", "DICOS Quadrupole Resonance (QR) Storage");
        public static readonly DicomUID EddyCurrentImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.601.1", "Eddy Current Image Storage");
        public static readonly DicomUID EddyCurrentMultiFrameImageStorage = new DicomUID("1.2.840.10008.5.1.4.1.1.601.2", "Eddy Current Multi-frame Image Storage");
        public static readonly DicomUID PatientRootQueryRetrieveInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.1.2.1.1", "Patient Root Query/Retrieve Information Model - FIND");
        public static readonly DicomUID PatientRootQueryRetrieveInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.1.2.1.2", "Patient Root Query/Retrieve Information Model - MOVE");
        public static readonly DicomUID PatientRootQueryRetrieveInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.1.2.1.3", "Patient Root Query/Retrieve Information Model - GET");
        public static readonly DicomUID StudyRootQueryRetrieveInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.1.2.2.1", "Study Root Query/Retrieve Information Model - FIND");
        public static readonly DicomUID StudyRootQueryRetrieveInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.1.2.2.2", "Study Root Query/Retrieve Information Model - MOVE");
        public static readonly DicomUID StudyRootQueryRetrieveInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.1.2.2.3", "Study Root Query/Retrieve Information Model - GET");
        public static readonly DicomUID CompositeInstanceRootRetrieveMOVE = new DicomUID("1.2.840.10008.5.1.4.1.2.4.2", "Composite Instance Root Retrieve - MOVE");
        public static readonly DicomUID CompositeInstanceRootRetrieveGET = new DicomUID("1.2.840.10008.5.1.4.1.2.4.3", "Composite Instance Root Retrieve - GET");
        public static readonly DicomUID CompositeInstanceRetrieveWithoutBulkDataGET = new DicomUID("1.2.840.10008.5.1.4.1.2.5.3", "Composite Instance Retrieve Without Bulk Data - GET");
        public static readonly DicomUID DefinedProcedureProtocolInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.20.1", "Defined Procedure Protocol Information Model - FIND");
        public static readonly DicomUID DefinedProcedureProtocolInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.20.2", "Defined Procedure Protocol Information Model - MOVE");
        public static readonly DicomUID DefinedProcedureProtocolInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.20.3", "Defined Procedure Protocol Information Model - GET");
        public static readonly DicomUID ModalityWorklistInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.31", "Modality Worklist Information Model - FIND");
        public static readonly DicomUID InstanceAvailabilityNotificationSOPClass = new DicomUID("1.2.840.10008.5.1.4.33", "Instance Availability Notification SOP Class");
        public static readonly DicomUID UnifiedProcedureStepPushSOPClass = new DicomUID("1.2.840.10008.5.1.4.34.6.1", "Unified Procedure Step - Push SOP Class");
        public static readonly DicomUID UnifiedProcedureStepWatchSOPClass = new DicomUID("1.2.840.10008.5.1.4.34.6.2", "Unified Procedure Step - Watch SOP Class");
        public static readonly DicomUID UnifiedProcedureStepPullSOPClass = new DicomUID("1.2.840.10008.5.1.4.34.6.3", "Unified Procedure Step - Pull SOP Class");
        public static readonly DicomUID UnifiedProcedureStepEventSOPClass = new DicomUID("1.2.840.10008.5.1.4.34.6.4", "Unified Procedure Step - Event SOP Class");
        public static readonly DicomUID RTBeamsDeliveryInstructionStorage = new DicomUID("1.2.840.10008.5.1.4.34.7", "RT Beams Delivery Instruction Storage");
        public static readonly DicomUID RTConventionalMachineVerification = new DicomUID("1.2.840.10008.5.1.4.34.8", "RT Conventional Machine Verification");
        public static readonly DicomUID RTIonMachineVerification = new DicomUID("1.2.840.10008.5.1.4.34.9", "RT Ion Machine Verification");
        public static readonly DicomUID RTBrachyApplicationSetupDeliveryInstructionStorage = new DicomUID("1.2.840.10008.5.1.4.34.10", "RT Brachy Application Setup Delivery Instruction Storage");
        public static readonly DicomUID GeneralRelevantPatientInformationQuery = new DicomUID("1.2.840.10008.5.1.4.37.1", "General Relevant Patient Information Query");
        public static readonly DicomUID BreastImagingRelevantPatientInformationQuery = new DicomUID("1.2.840.10008.5.1.4.37.2", "Breast Imaging Relevant Patient Information Query");
        public static readonly DicomUID CardiacRelevantPatientInformationQuery = new DicomUID("1.2.840.10008.5.1.4.37.3", "Cardiac Relevant Patient Information Query");
        public static readonly DicomUID HangingProtocolStorage = new DicomUID("1.2.840.10008.5.1.4.38.1", "Hanging Protocol Storage");
        public static readonly DicomUID HangingProtocolInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.38.2", "Hanging Protocol Information Model - FIND");
        public static readonly DicomUID HangingProtocolInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.38.3", "Hanging Protocol Information Model - MOVE");
        public static readonly DicomUID HangingProtocolInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.38.4", "Hanging Protocol Information Model - GET");
        public static readonly DicomUID ColorPaletteStorage = new DicomUID("1.2.840.10008.5.1.4.39.1", "Color Palette Storage");
        public static readonly DicomUID ColorPaletteQueryRetrieveInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.39.2", "Color Palette Query/Retrieve Information Model - FIND");
        public static readonly DicomUID ColorPaletteQueryRetrieveInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.39.3", "Color Palette Query/Retrieve Information Model - MOVE");
        public static readonly DicomUID ColorPaletteQueryRetrieveInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.39.4", "Color Palette Query/Retrieve Information Model - GET");
        public static readonly DicomUID ProductCharacteristicsQuerySOPClass = new DicomUID("1.2.840.10008.5.1.4.41", "Product Characteristics Query SOP Class");
        public static readonly DicomUID SubstanceApprovalQuerySOPClass = new DicomUID("1.2.840.10008.5.1.4.42", "Substance Approval Query SOP Class");
        public static readonly DicomUID GenericImplantTemplateStorage = new DicomUID("1.2.840.10008.5.1.4.43.1", "Generic Implant Template Storage");
        public static readonly DicomUID GenericImplantTemplateInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.43.2", "Generic Implant Template Information Model - FIND");
        public static readonly DicomUID GenericImplantTemplateInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.43.3", "Generic Implant Template Information Model - MOVE");
        public static readonly DicomUID GenericImplantTemplateInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.43.4", "Generic Implant Template Information Model - GET");
        public static readonly DicomUID ImplantAssemblyTemplateStorage = new DicomUID("1.2.840.10008.5.1.4.44.1", "Implant Assembly Template Storage");
        public static readonly DicomUID ImplantAssemblyTemplateInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.44.2", "Implant Assembly Template Information Model - FIND");
        public static readonly DicomUID ImplantAssemblyTemplateInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.44.3", "Implant Assembly Template Information Model - MOVE");
        public static readonly DicomUID ImplantAssemblyTemplateInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.44.4", "Implant Assembly Template Information Model - GET");
        public static readonly DicomUID ImplantTemplateGroupStorage = new DicomUID("1.2.840.10008.5.1.4.45.1", "Implant Template Group Storage");
        public static readonly DicomUID ImplantTemplateGroupInformationModelFIND = new DicomUID("1.2.840.10008.5.1.4.45.2", "Implant Template Group Information Model - FIND");
        public static readonly DicomUID ImplantTemplateGroupInformationModelMOVE = new DicomUID("1.2.840.10008.5.1.4.45.3", "Implant Template Group Information Model - MOVE");
        public static readonly DicomUID ImplantTemplateGroupInformationModelGET = new DicomUID("1.2.840.10008.5.1.4.45.4", "Implant Template Group Information Model - GET");
        public static readonly DicomUID VideoEndoscopicImageRealTimeCommunication = new DicomUID("1.2.840.10008.10.1", "Video Endoscopic Image Real-Time Communication");
        public static readonly DicomUID VideoPhotographicImageRealTimeCommunication = new DicomUID("1.2.840.10008.10.2", "Video Photographic Image Real-Time Communication");
        public static readonly DicomUID AudioWaveformRealTimeCommunication = new DicomUID("1.2.840.10008.10.3", "Audio Waveform Real-Time Communication");
        public static readonly DicomUID RenditionSelectionDocumentRealTimeCommunication = new DicomUID("1.2.840.10008.10.4", "Rendition Selection Document Real-Time Communication");

        #endregion

        #region Transfer Syntax

        public static readonly DicomUID ImplicitVRLittleEndian = new DicomUID("1.2.840.10008.1.2", "Implicit VR Little Endian");
        public static readonly DicomUID ExplicitVRLittleEndian = new DicomUID("1.2.840.10008.1.2.1", "Explicit VR Little Endian");
        public static readonly DicomUID DeflatedExplicitVRLittleEndian = new DicomUID("1.2.840.10008.1.2.1.99", "Deflated Explicit VR Little Endian");
        public static readonly DicomUID JPEGBaseline_Process1 = new DicomUID("1.2.840.10008.1.2.4.50", "JPEG Baseline (Process 1)");
        public static readonly DicomUID JPEGExtended_Process2And4 = new DicomUID("1.2.840.10008.1.2.4.51", "JPEG Extended (Process 2 & 4)");
        public static readonly DicomUID JPEGLossless_NonHierarchical_Process14 = new DicomUID("1.2.840.10008.1.2.4.57", "JPEG Lossless, Non-Hierarchical (Process 14)");
        public static readonly DicomUID JPEGLossless_NonHierarchical_FirstOrderPrediction_Process14_SelectionValue1 = new DicomUID("1.2.840.10008.1.2.4.70", "JPEG Lossless, Non-Hierarchical, First-Order Prediction (Process 14 [Selection Value 1])");
        public static readonly DicomUID JPEGLSLosslessImageCompression = new DicomUID("1.2.840.10008.1.2.4.80", "JPEG-LS Lossless Image Compression");
        public static readonly DicomUID JPEGLSLossy_NearLossless_ImageCompression = new DicomUID("1.2.840.10008.1.2.4.81", "JPEG-LS Lossy (Near-Lossless) Image Compression");
        public static readonly DicomUID JPEG2000ImageCompression_LosslessOnly = new DicomUID("1.2.840.10008.1.2.4.90", "JPEG 2000 Image Compression (Lossless Only)");
        public static readonly DicomUID JPEG2000ImageCompression = new DicomUID("1.2.840.10008.1.2.4.91", "JPEG 2000 Image Compression");
        public static readonly DicomUID JPEG2000Part2MultiComponentImageCompression_LosslessOnly = new DicomUID("1.2.840.10008.1.2.4.92", "JPEG 2000 Part 2 Multi-component Image Compression (Lossless Only)");
        public static readonly DicomUID JPEG2000Part2MultiComponentImageCompression = new DicomUID("1.2.840.10008.1.2.4.93", "JPEG 2000 Part 2 Multi-component Image Compression");
        public static readonly DicomUID JPIPReferenced = new DicomUID("1.2.840.10008.1.2.4.94", "JPIP Referenced");
        public static readonly DicomUID JPIPReferencedDeflate = new DicomUID("1.2.840.10008.1.2.4.95", "JPIP Referenced Deflate");
        public static readonly DicomUID MPEG2MainProfileMainLevel = new DicomUID("1.2.840.10008.1.2.4.100", "MPEG2 Main Profile / Main Level");
        public static readonly DicomUID MPEG2MainProfileHighLevel = new DicomUID("1.2.840.10008.1.2.4.101", "MPEG2 Main Profile / High Level");
        public static readonly DicomUID MPEG_4AVCH264HighProfileLevel41 = new DicomUID("1.2.840.10008.1.2.4.102", "MPEG-4 AVC/H.264 High Profile / Level 4.1");
        public static readonly DicomUID MPEG_4AVCH264BDCompatibleHighProfileLevel41 = new DicomUID("1.2.840.10008.1.2.4.103", "MPEG-4 AVC/H.264 BD-compatible High Profile / Level 4.1");
        public static readonly DicomUID MPEG_4AVCH264HighProfileLevel42For2DVideo = new DicomUID("1.2.840.10008.1.2.4.104", "MPEG-4 AVC/H.264 High Profile / Level 4.2 For 2D Video");
        public static readonly DicomUID MPEG_4AVCH264HighProfileLevel42For3DVideo = new DicomUID("1.2.840.10008.1.2.4.105", "MPEG-4 AVC/H.264 High Profile / Level 4.2 For 3D Video");
        public static readonly DicomUID MPEG_4AVCH264StereoHighProfileLevel42 = new DicomUID("1.2.840.10008.1.2.4.106", "MPEG-4 AVC/H.264 Stereo High Profile / Level 4.2");
        public static readonly DicomUID HEVCH265MainProfileLevel51 = new DicomUID("1.2.840.10008.1.2.4.107", "HEVC/H.265 Main Profile / Level 5.1");
        public static readonly DicomUID HEVCH265Main10ProfileLevel51 = new DicomUID("1.2.840.10008.1.2.4.108", "HEVC/H.265 Main 10 Profile / Level 5.1");
        public static readonly DicomUID RLELossless = new DicomUID("1.2.840.10008.1.2.5", "RLE Lossless");
        public static readonly DicomUID SMPTEST2110_20UncompressedProgressiveActiveVideo = new DicomUID("1.2.840.10008.1.2.7.1", "SMPTE ST 2110-20 Uncompressed Progressive Active Video");
        public static readonly DicomUID SMPTEST2110_20UncompressedInterlacedActiveVideo = new DicomUID("1.2.840.10008.1.2.7.2", "SMPTE ST 2110-20 Uncompressed Interlaced Active Video");
        public static readonly DicomUID SMPTEST2110_30PCMDigitalAudio = new DicomUID("1.2.840.10008.1.2.7.3", "SMPTE ST 2110-30 PCM Digital Audio");

        #endregion

        #region Well-known frame of reference

        public static readonly DicomUID TalairachBrainAtlasFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.1", "Talairach Brain Atlas Frame of Reference");
        public static readonly DicomUID SPM2T1FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.2", "SPM2 T1 Frame of Reference");
        public static readonly DicomUID SPM2T2FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.3", "SPM2 T2 Frame of Reference");
        public static readonly DicomUID SPM2PDFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.4", "SPM2 PD Frame of Reference");
        public static readonly DicomUID SPM2EPIFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.5", "SPM2 EPI Frame of Reference");
        public static readonly DicomUID SPM2FILT1FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.6", "SPM2 FIL T1 Frame of Reference");
        public static readonly DicomUID SPM2PETFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.7", "SPM2 PET Frame of Reference");
        public static readonly DicomUID SPM2TRANSMFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.8", "SPM2 TRANSM Frame of Reference");
        public static readonly DicomUID SPM2SPECTFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.9", "SPM2 SPECT Frame of Reference");
        public static readonly DicomUID SPM2GRAYFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.10", "SPM2 GRAY Frame of Reference");
        public static readonly DicomUID SPM2WHITEFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.11", "SPM2 WHITE Frame of Reference");
        public static readonly DicomUID SPM2CSFFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.12", "SPM2 CSF Frame of Reference");
        public static readonly DicomUID SPM2BRAINMASKFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.13", "SPM2 BRAINMASK Frame of Reference");
        public static readonly DicomUID SPM2AVG305T1FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.14", "SPM2 AVG305T1 Frame of Reference");
        public static readonly DicomUID SPM2AVG152T1FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.15", "SPM2 AVG152T1 Frame of Reference");
        public static readonly DicomUID SPM2AVG152T2FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.16", "SPM2 AVG152T2 Frame of Reference");
        public static readonly DicomUID SPM2AVG152PDFrameOfReference = new DicomUID("1.2.840.10008.1.4.1.17", "SPM2 AVG152PD Frame of Reference");
        public static readonly DicomUID SPM2SINGLESUBJT1FrameOfReference = new DicomUID("1.2.840.10008.1.4.1.18", "SPM2 SINGLESUBJT1 Frame of Reference");
        public static readonly DicomUID ICBM452T1FrameOfReference = new DicomUID("1.2.840.10008.1.4.2.1", "ICBM 452 T1 Frame of Reference");
        public static readonly DicomUID ICBMSingleSubjectMRIFrameOfReference = new DicomUID("1.2.840.10008.1.4.2.2", "ICBM Single Subject MRI Frame of Reference");
        public static readonly DicomUID IEC61217FixedCoordinateSystemFrameOfReference = new DicomUID("1.2.840.10008.1.4.3.1", "IEC 61217 Fixed Coordinate System Frame of Reference");

        #endregion

        #region Well-known SOP Instance

        public static readonly DicomUID HotIronColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.1", "Hot Iron Color Palette SOP Instance");
        public static readonly DicomUID PETColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.2", "PET Color Palette SOP Instance");
        public static readonly DicomUID HotMetalBlueColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.3", "Hot Metal Blue Color Palette SOP Instance");
        public static readonly DicomUID PET20StepColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.4", "PET 20 Step Color Palette SOP Instance");
        public static readonly DicomUID SpringColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.5", "Spring Color Palette SOP Instance");
        public static readonly DicomUID SummerColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.6", "Summer Color Palette SOP Instance");
        public static readonly DicomUID FallColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.7", "Fall Color Palette SOP Instance");
        public static readonly DicomUID WinterColorPaletteSOPInstance = new DicomUID("1.2.840.10008.1.5.8", "Winter Color Palette SOP Instance");
        public static readonly DicomUID StorageCommitmentPushModelSOPInstance = new DicomUID("1.2.840.10008.1.20.1.1", "Storage Commitment Push Model SOP Instance");
        public static readonly DicomUID ProceduralEventLoggingSOPInstance = new DicomUID("1.2.840.10008.1.40.1", "Procedural Event Logging SOP Instance");
        public static readonly DicomUID SubstanceAdministrationLoggingSOPInstance = new DicomUID("1.2.840.10008.1.42.1", "Substance Administration Logging SOP Instance");
        public static readonly DicomUID DisplaySystemSOPInstance = new DicomUID("1.2.840.10008.5.1.1.40.1", "Display System SOP Instance");
        public static readonly DicomUID UPSGlobalSubscriptionSOPInstance = new DicomUID("1.2.840.10008.5.1.4.34.5", "UPS Global Subscription SOP Instance");
        public static readonly DicomUID UPSFilteredGlobalSubscriptionSOPInstance = new DicomUID("1.2.840.10008.5.1.4.34.5.1", "UPS Filtered Global Subscription SOP Instance");

        #endregion

        #region DICOM UIDs as a Coding Scheme

        public static readonly DicomUID DICOMUIDRegistry = new DicomUID("1.2.840.10008.2.6.1", "DICOM UID Registry");

        #endregion

        #region Coding Scheme

        public static readonly DicomUID DICOMControlledTerminology = new DicomUID("1.2.840.10008.2.16.4", "DICOM Controlled Terminology");
        public static readonly DicomUID AdultMouseAnatomyOntology = new DicomUID("1.2.840.10008.2.16.5", "Adult Mouse Anatomy Ontology");
        public static readonly DicomUID UberonOntology = new DicomUID("1.2.840.10008.2.16.6", "Uberon Ontology");
        public static readonly DicomUID IntegratedTaxonomicInformationSystem_ITIS_TaxonomicSerialNumber_TSN = new DicomUID("1.2.840.10008.2.16.7", "Integrated Taxonomic Information System (ITIS) Taxonomic Serial Number (TSN)");
        public static readonly DicomUID MouseGenomeInitiative_MGI = new DicomUID("1.2.840.10008.2.16.8", "Mouse Genome Initiative (MGI)");
        public static readonly DicomUID PubChemCompoundCID = new DicomUID("1.2.840.10008.2.16.9", "PubChem Compound CID");
        public static readonly DicomUID ICD_11 = new DicomUID("1.2.840.10008.2.16.10", "ICD-11");
        public static readonly DicomUID NewYorkUniversityMelanomaClinicalCooperativeGroup = new DicomUID("1.2.840.10008.2.16.11", "New York University Melanoma Clinical Cooperative Group");
        public static readonly DicomUID MayoClinicNonRadiologicalImagesSpecificBodyStructureAnatomicalSurfaceRegionGuide = new DicomUID("1.2.840.10008.2.16.12", "Mayo Clinic Non-radiological Images Specific Body Structure Anatomical Surface Region Guide");
        public static readonly DicomUID ImageBiomarkerStandardisationInitiative = new DicomUID("1.2.840.10008.2.16.13", "Image Biomarker Standardisation Initiative");
        public static readonly DicomUID RadiomicsOntology = new DicomUID("1.2.840.10008.2.16.14", "Radiomics Ontology");

        #endregion

        #region Application Context Name

        public static readonly DicomUID DICOMApplicationContextName = new DicomUID("1.2.840.10008.3.1.1.1", "DICOM Application Context Name");

        #endregion

        #region Service Class

        public static readonly DicomUID StorageServiceClass = new DicomUID("1.2.840.10008.4.2", "Storage Service Class");
        public static readonly DicomUID UnifiedWorklistAndProcedureStepServiceClass = new DicomUID("1.2.840.10008.5.1.4.34.6", "Unified Worklist and Procedure Step Service Class");

        #endregion

        #region Meta SOP Class

        public static readonly DicomUID BasicGrayscalePrintManagementMetaSOPClass = new DicomUID("1.2.840.10008.5.1.1.9", "Basic Grayscale Print Management Meta SOP Class");
        public static readonly DicomUID BasicColorPrintManagementMetaSOPClass = new DicomUID("1.2.840.10008.5.1.1.18", "Basic Color Print Management Meta SOP Class");

        #endregion

        #region Well-known Printer SOP Instance

        public static readonly DicomUID PrinterSOPInstance = new DicomUID("1.2.840.10008.5.1.1.17", "Printer SOP Instance");
        public static readonly DicomUID PrinterConfigurationRetrievalSOPInstance = new DicomUID("1.2.840.10008.5.1.1.17.376", "Printer Configuration Retrieval SOP Instance");

        #endregion

        #region Application Hosting Model

        public static readonly DicomUID NativeDICOMModel = new DicomUID("1.2.840.10008.7.1.1", "Native DICOM Model");
        public static readonly DicomUID AbstractMultiDimensionalImageModel = new DicomUID("1.2.840.10008.7.1.2", "Abstract Multi-Dimensional Image Model");

        #endregion

        #region Mapping Resource

        public static readonly DicomUID DICOMContentMappingResource = new DicomUID("1.2.840.10008.8.1.1", "DICOM Content Mapping Resource");

        #endregion

        #region LDAP OID

        public static readonly DicomUID dicomDeviceName = new DicomUID("1.2.840.10008.15.0.3.1", "dicomDeviceName");
        public static readonly DicomUID dicomDescription = new DicomUID("1.2.840.10008.15.0.3.2", "dicomDescription");
        public static readonly DicomUID dicomManufacturer = new DicomUID("1.2.840.10008.15.0.3.3", "dicomManufacturer");
        public static readonly DicomUID dicomManufacturerModelName = new DicomUID("1.2.840.10008.15.0.3.4", "dicomManufacturerModelName");
        public static readonly DicomUID dicomSoftwareVersion = new DicomUID("1.2.840.10008.15.0.3.5", "dicomSoftwareVersion");
        public static readonly DicomUID dicomVendorData = new DicomUID("1.2.840.10008.15.0.3.6", "dicomVendorData");
        public static readonly DicomUID dicomAETitle = new DicomUID("1.2.840.10008.15.0.3.7", "dicomAETitle");
        public static readonly DicomUID dicomNetworkConnectionReference = new DicomUID("1.2.840.10008.15.0.3.8", "dicomNetworkConnectionReference");
        public static readonly DicomUID dicomApplicationCluster = new DicomUID("1.2.840.10008.15.0.3.9", "dicomApplicationCluster");
        public static readonly DicomUID dicomAssociationInitiator = new DicomUID("1.2.840.10008.15.0.3.10", "dicomAssociationInitiator");
        public static readonly DicomUID dicomAssociationAcceptor = new DicomUID("1.2.840.10008.15.0.3.11", "dicomAssociationAcceptor");
        public static readonly DicomUID dicomHostname = new DicomUID("1.2.840.10008.15.0.3.12", "dicomHostname");
        public static readonly DicomUID dicomPort = new DicomUID("1.2.840.10008.15.0.3.13", "dicomPort");
        public static readonly DicomUID dicomSOPClass = new DicomUID("1.2.840.10008.15.0.3.14", "dicomSOPClass");
        public static readonly DicomUID dicomTransferRole = new DicomUID("1.2.840.10008.15.0.3.15", "dicomTransferRole");
        public static readonly DicomUID dicomTransferSyntax = new DicomUID("1.2.840.10008.15.0.3.16", "dicomTransferSyntax");
        public static readonly DicomUID dicomPrimaryDeviceType = new DicomUID("1.2.840.10008.15.0.3.17", "dicomPrimaryDeviceType");
        public static readonly DicomUID dicomRelatedDeviceReference = new DicomUID("1.2.840.10008.15.0.3.18", "dicomRelatedDeviceReference");
        public static readonly DicomUID dicomPreferredCalledAETitle = new DicomUID("1.2.840.10008.15.0.3.19", "dicomPreferredCalledAETitle");
        public static readonly DicomUID dicomTLSCyphersuite = new DicomUID("1.2.840.10008.15.0.3.20", "dicomTLSCyphersuite");
        public static readonly DicomUID dicomAuthorizedNodeCertificateReference = new DicomUID("1.2.840.10008.15.0.3.21", "dicomAuthorizedNodeCertificateReference");
        public static readonly DicomUID dicomThisNodeCertificateReference = new DicomUID("1.2.840.10008.15.0.3.22", "dicomThisNodeCertificateReference");
        public static readonly DicomUID dicomInstalled = new DicomUID("1.2.840.10008.15.0.3.23", "dicomInstalled");
        public static readonly DicomUID dicomStationName = new DicomUID("1.2.840.10008.15.0.3.24", "dicomStationName");
        public static readonly DicomUID dicomDeviceSerialNumber = new DicomUID("1.2.840.10008.15.0.3.25", "dicomDeviceSerialNumber");
        public static readonly DicomUID dicomInstitutionName = new DicomUID("1.2.840.10008.15.0.3.26", "dicomInstitutionName");
        public static readonly DicomUID dicomInstitutionAddress = new DicomUID("1.2.840.10008.15.0.3.27", "dicomInstitutionAddress");
        public static readonly DicomUID dicomInstitutionDepartmentName = new DicomUID("1.2.840.10008.15.0.3.28", "dicomInstitutionDepartmentName");
        public static readonly DicomUID dicomIssuerOfPatientID = new DicomUID("1.2.840.10008.15.0.3.29", "dicomIssuerOfPatientID");
        public static readonly DicomUID dicomPreferredCallingAETitle = new DicomUID("1.2.840.10008.15.0.3.30", "dicomPreferredCallingAETitle");
        public static readonly DicomUID dicomSupportedCharacterSet = new DicomUID("1.2.840.10008.15.0.3.31", "dicomSupportedCharacterSet");
        public static readonly DicomUID dicomConfigurationRoot = new DicomUID("1.2.840.10008.15.0.4.1", "dicomConfigurationRoot");
        public static readonly DicomUID dicomDevicesRoot = new DicomUID("1.2.840.10008.15.0.4.2", "dicomDevicesRoot");
        public static readonly DicomUID dicomUniqueAETitlesRegistryRoot = new DicomUID("1.2.840.10008.15.0.4.3", "dicomUniqueAETitlesRegistryRoot");
        public static readonly DicomUID dicomDevice = new DicomUID("1.2.840.10008.15.0.4.4", "dicomDevice");
        public static readonly DicomUID dicomNetworkAE = new DicomUID("1.2.840.10008.15.0.4.5", "dicomNetworkAE");
        public static readonly DicomUID dicomNetworkConnection = new DicomUID("1.2.840.10008.15.0.4.6", "dicomNetworkConnection");
        public static readonly DicomUID dicomUniqueAETitle = new DicomUID("1.2.840.10008.15.0.4.7", "dicomUniqueAETitle");
        public static readonly DicomUID dicomTransferCapability = new DicomUID("1.2.840.10008.15.0.4.8", "dicomTransferCapability");

        #endregion

        #region Synchronization Frame of Reference

        public static readonly DicomUID UniversalCoordinatedTime = new DicomUID("1.2.840.10008.15.1.1", "Universal Coordinated Time");

        #endregion
    }
}
