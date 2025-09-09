//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/18 11:35:42    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomCodeItems
{
    public class ConceptNameCodeItems
    {
        public static readonly DicomCodeItem XRayRadiationDoseReport_DCM = new DicomCodeItem("113701", "DCM", "X-Ray Radiation Dose Report");
        public static readonly DicomCodeItem LanguageOfContentItemAndDescendants_DCM = new DicomCodeItem("121049", "DCM", "Language of ContentItem and Descendants");
        public static readonly DicomCodeItem CountryOfLanguage_DCM = new DicomCodeItem("121046","DCM","Country of Language");
        public static readonly DicomCodeItem ProcedureReported_DCM = new DicomCodeItem("121058", "DCM", "Procedure Reported");
        public static readonly DicomCodeItem HasIntent_SCT = new DicomCodeItem("363703001", "SCT", "Has Intent");
        public static readonly DicomCodeItem HasIntent_SRT = new DicomCodeItem("G-C0E8", "SRT", "Has Intent");


        public static readonly DicomCodeItem ObserverType_DCM = new DicomCodeItem("121005", "DCM", "Observer Type");
        public static readonly DicomCodeItem DeviceObserverUID_SCT = new DicomCodeItem("121012", "SCT", "Device Observer UID");

        public static readonly DicomCodeItem StartOfXRayIrradiation_DCM = new DicomCodeItem("113809", "DCM", "Start of X-Ray Irradiation");
        public static readonly DicomCodeItem EndOfXRayIrradiation_DCM = new DicomCodeItem("113810", "DCM", "End of X-Ray Irradiation");
        public static readonly DicomCodeItem ScopeOfAccumulation_DCM = new DicomCodeItem("113705", "DCM", "Scope of Accumulation");

        public static readonly DicomCodeItem CTAccumulatedDoseData_DCM = new DicomCodeItem("113811", "DCM", "CT Accumulated Dose Data");
        public static readonly DicomCodeItem TotalNumberOfIrradiationEvents_DCM = new DicomCodeItem("113812", "DCM", "Total Number of Irradiation Events");
        public static readonly DicomCodeItem CTDoseLengthProductTotal_DCM = new DicomCodeItem("113813", "DCM", "CT Dose Length Product Total");
        public static readonly DicomCodeItem CTDIwPhantomType_DCM = new DicomCodeItem("113835", "DCM", "CTDIw Phantom Type");
        public static readonly DicomCodeItem Comment_DCM = new DicomCodeItem("121106", "DCM", "Comment");
        public static readonly DicomCodeItem DeviceRoleInProcedure_DCM = new DicomCodeItem("113876", "DCM", "Device Role in Procedure");
        public static readonly DicomCodeItem DeviceManufacturer_DCM = new DicomCodeItem("113878", "DCM", "Device Manufacturer");
        public static readonly DicomCodeItem DeviceModelName_DCM = new DicomCodeItem("113879", "DCM", "Device Model Name");
        public static readonly DicomCodeItem DeviceSerialNumber_DCM = new DicomCodeItem("113880", "DCM", "Device Serial Number");


        public static readonly DicomCodeItem CTAcquisition_DCM = new DicomCodeItem("113819", "DCM", "CT Acquisition");
        public static readonly DicomCodeItem AcquisitionProtocol_DCM = new DicomCodeItem("125203", "DCM", "Acquisition Protocol");
        public static readonly DicomCodeItem TargetRegion_DCM = new DicomCodeItem("123014", "DCM", "Target Region");
        public static readonly DicomCodeItem CTAcquisitionType_DCM = new DicomCodeItem("113820", "DCM", "CT Acquisition Type");
        public static readonly DicomCodeItem ProcedureContext_SCT = new DicomCodeItem("408730004", "SCT", "Procedure Context");
        public static readonly DicomCodeItem IrradiationEventUID_DCM = new DicomCodeItem("113769", "DCM", "Irradiation Event UID");
        public static readonly DicomCodeItem DateTimeStarted_DCM = new DicomCodeItem("111526", "DCM", "DateTime Started");
        public static readonly DicomCodeItem CTAcquisitionParameters_DCM = new DicomCodeItem("113822", "DCM", "CT Acquisition Parameters");
        public static readonly DicomCodeItem ExposureTime_DCM = new DicomCodeItem("113824", "DCM", "Exposure Time");

        public static readonly DicomCodeItem ScanningLength_DCM = new DicomCodeItem("113825", "DCM", "Scanning Length");
        public static readonly DicomCodeItem LengthOfReconstructableVolume_DCM = new DicomCodeItem("113893", "DCM", "Length of Reconstructable Volume");
        public static readonly DicomCodeItem ExposedRange_DCM = new DicomCodeItem("113899", "DCM", "Exposed Range");
        public static readonly DicomCodeItem FrameOfReferenceUID_DCM = new DicomCodeItem("112227", "DCM", "Frame of Reference UID");

        public static readonly DicomCodeItem NominalSingleCollimationWidth_DCM = new DicomCodeItem("113826", "DCM", "Nominal Single Collimation Width");
        public static readonly DicomCodeItem NominalTotalCollimationWidth_DCM = new DicomCodeItem("113827", "DCM", "Nominal Total Collimation Width");
        public static readonly DicomCodeItem PitchFactor_DCM = new DicomCodeItem("113828", "DCM", "Pitch Factor");
        public static readonly DicomCodeItem NumberOfXRaySources_DCM = new DicomCodeItem("113823", "DCM", "Number of X-Ray Sources");
        public static readonly DicomCodeItem CTXRaySourceParameters_DCM = new DicomCodeItem("113831", "DCM", "CT X-Ray Source Parameters");
        public static readonly DicomCodeItem IdentificationOfTheXRaySource_DCM = new DicomCodeItem("113832", "DCM", "Identification of the X-Ray Source");
        public static readonly DicomCodeItem KVP_DCM = new DicomCodeItem("113733", "DCM", "KVP");
        public static readonly DicomCodeItem MaximumXRayTubeCurrent_DCM = new DicomCodeItem("113833", "DCM", "Maximum X-Ray Tube Current");
        public static readonly DicomCodeItem XRayTubeCurrent_DCM = new DicomCodeItem("113734", "DCM", "X-Ray Tube Current");
        public static readonly DicomCodeItem ExposureTimePerRotation_DCM = new DicomCodeItem("113834", "DCM", "Exposure Time per Rotation");
        public static readonly DicomCodeItem CTDose_DCM = new DicomCodeItem("113829", "DCM", "CT Dose");
        public static readonly DicomCodeItem MeanCTDIvol_DCM = new DicomCodeItem("113830", "DCM", "Mean CTDIvol");
        public static readonly DicomCodeItem DLP_DCM = new DicomCodeItem("113838", "DCM", "DLP");


        public static readonly DicomCodeItem DoseCheckAlertDetails_DCM = new DicomCodeItem("113900", "DCM", "Dose Check Alert Details");
        public static readonly DicomCodeItem DLPAlertValueConfigured_DCM = new DicomCodeItem("113901", "DCM", "DLP Alert Value Configured");
        public static readonly DicomCodeItem CTDIvolAlertValueConfigured_DCM = new DicomCodeItem("113902", "DCM", "CTDIvol Alert Value Configured");
        public static readonly DicomCodeItem DLPAlertValue_DCM = new DicomCodeItem("113903", "DCM", "DLP Alert Value");
        public static readonly DicomCodeItem CTDIvolAlertValue_DCM = new DicomCodeItem("113904", "DCM", "CTDIvol Alert Value");
        public static readonly DicomCodeItem AccumulatedDLPForwardEstimate_DCM = new DicomCodeItem("113905", "DCM", "Accumulated DLP Forward Estimate");
        public static readonly DicomCodeItem AccumulatedCTDIvolForwardEstimate_DCM = new DicomCodeItem("113906", "DCM", "Accumulated CTDIvol Forward Estimate");
        public static readonly DicomCodeItem ReasonForProceeding_DCM = new DicomCodeItem("113907", "DCM", "Reason for Proceeding");
        public static readonly DicomCodeItem PersonName_DCM = new DicomCodeItem("113870", "DCM", "Person Name");
        public static readonly DicomCodeItem PersonRoleInProcedure_DCM = new DicomCodeItem("113875", "DCM", "Person Role in Procedure");
        public static readonly DicomCodeItem DoseCheckNotificationDetails_DCM = new DicomCodeItem("113908", "DCM", "Dose Check Notification Details");
        public static readonly DicomCodeItem DLPNotificationValueConfigured_DCM = new DicomCodeItem("113909", "DCM", "DLP Notification Value Configured");
        public static readonly DicomCodeItem CTDIvolNotificationValueConfigured_DCM = new DicomCodeItem("113910", "DCM", "CTDIvol Notification Value Configured");
        public static readonly DicomCodeItem DLPNotificationValue_DCM = new DicomCodeItem("113911", "DCM", "DLP Notification Value");
        public static readonly DicomCodeItem CTDIvolNotificationValue_DCM = new DicomCodeItem("113912", "DCM", "CTDIvol Notification Value");
        public static readonly DicomCodeItem DLPForwardEstimate_DCM = new DicomCodeItem("113913", "DCM", "DLP Forward Estimate");
        public static readonly DicomCodeItem CTDIvolForwardEstimate_DCM = new DicomCodeItem("113914", "DCM", "CTDIvol Forward Estimate");
        public static readonly DicomCodeItem StudyInstanceUID_DCM = new DicomCodeItem("110180", "DCM", "Study Instance UID");
        public static readonly DicomCodeItem DeviceName_DCM = new DicomCodeItem("113877", "DCM", "Device Name");
        public static readonly DicomCodeItem SourceOfDoseInformation_DCM = new DicomCodeItem("113854", "DCM", "Source of Dose Information");



    }
}
