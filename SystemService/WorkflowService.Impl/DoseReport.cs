//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/4 13:48:49     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomIOD;
using NV.CT.DicomUtility.DicomModule;
using NV.CT.DicomUtility.DoseReportSR;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using System.IO;

namespace NV.CT.WorkflowService.Impl;

public class DoseReport
{
    private readonly ILogger<DoseReport> _logger;
    private readonly ISeriesService _seriesService;   
    private readonly ProductSettingInfo _productConfig;

    private static int DoseSRSeriesNum = 901;
    private static string DoseSRSeriesDescription = "Dose SR";
    private static int Scalar_MS2S = 1000;

    public DoseReport(ILogger<DoseReport> logger, ISeriesService seriesService,IAuthorization auth)
    {
        _logger = logger;
        _seriesService = seriesService;
        _productConfig = NV.MPS.Configuration.ProductConfig.ProductSettingConfig.ProductSetting;
    }

    public void CreateReport(StudyModel study, PatientModel patient, ProtocolModel protocol, List<ScanTaskModel> scanTasks)
    {
        var iod = new XRayRadiationDoseSRIOD();

        var seriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();

        FillPatientModule(patient, study, iod.PatientModule);
        FillStudyModule(study, iod.GeneralStudyModule);
        FillSRDocumentSeriesModule(iod.SRDocumentSeriesModule, seriesInstanceUID);
        FillGeneralEquipment(iod.GeneralEquipmentModule);
        FillEnhancedEquipment(iod.EnhancedGeneralEquipmentModule);
        FillSRDocumentGeneral(iod.SRDocumentGeneralModule);
        FillSOPCommon(iod.SOPCommonModule);
        
        var instanceData = CreateDoseReportData(study, patient, protocol, scanTasks);
        var filePath = Path.Combine(RuntimeConfig.Console.ReportData.Path, "..", study.StudyInstanceUID, $"{DoseSRSeriesNum}_{seriesInstanceUID}");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var fileName = Path.Combine(filePath, "StructureReport.dcm");
        File.WriteAllText(Path.Combine(filePath, "StructureReport.json"), JsonConvert.SerializeObject(new { Patient = patient, Study = study, Scans = scanTasks, IOD = iod, Data = instanceData }));
        _logger.LogDebug($"DoseInfo.CreateStructureReport study id : {study.Id}");
        StructureReportGenerator.Instance.GenerateDoseReportSR(iod, instanceData, fileName);
        InsertSeries(study, patient, protocol, seriesInstanceUID, fileName);
    }


    private void InsertSeries(StudyModel study, PatientModel patient, ProtocolModel protocol, string seriesInstanceUID, string fileName)
    {
        var series = new SeriesModel();
        series.Id = Guid.NewGuid().ToString();
        series.InternalStudyId = study.Id;
        series.BodyPart = $"{protocol.BodyPart}";
        series.ProtocolName = protocol.Descriptor.Name;

        series.SeriesNumber = DoseSRSeriesNum.ToString() ;

        series.SeriesInstanceUID = seriesInstanceUID;
        series.ScanId = string.Empty;
        series.ReconId = string.Empty;
        series.ImageCount = 1;
        series.ReconEndDate = DateTime.Now;
        series.SeriesType = "Dose SR";
        series.SeriesPath = fileName;
        series.SeriesDescription = DoseSRSeriesDescription;

        series.FrameOfReferenceUID = string.Empty;
        series.PatientPosition = string.Empty;

        series.ImageType = "Dose SR";
        series.WindowWidth = string.Empty;
        series.WindowLevel = string.Empty;

        _seriesService.Add(series);
    }

    private void FillPatientModule(PatientModel patient, StudyModel study, PatientModule module)
    {
        module.PatientName = patient.PatientName;
        module.PatientBirthDate = AgeHelper.GetBirthday((NV.CT.CTS.Enums.AgeType)study.AgeType, study.Age);
        module.PatientID = patient.PatientId;
        module.PatientSex = (PatientSexCS)Enum.Parse(typeof(PatientSexCS), patient.PatientSex.ToString().Substring(0, 1));
    }

    private void FillStudyModule(StudyModel study, GeneralStudyModule module)
    {
        module.StudyInstanceUID = study.StudyInstanceUID;
        module.StudyDate = study.StudyDate;
        module.StudyTime = study.StudyTime;
        module.StudyID = !string.IsNullOrEmpty(study.StudyId) ? study.StudyId : string.Empty;
        module.AccessionNumber = study.AccessionNo;
    }

    private void FillSRDocumentSeriesModule(SRDocumentSeriesModule module, string instanceUID)
    {
        module.Modality = ModalityCS.SR;
        module.SeriesNumber = DoseSRSeriesNum;
        module.SeriesInstanceUID = instanceUID;
        module.SeriesDescription = DoseSRSeriesDescription;
    }

    private void FillGeneralEquipment(GeneralEquipmentModule module)
    {
        module.Manufacturer = _productConfig.Manufacturer;
    }

    private void FillEnhancedEquipment(EnhancedGeneralEquipmentModule module)
    {
        //TODO: 后期修改，走配置
        module.Manufacturer = _productConfig.Manufacturer;
        module.SoftwareVersions = _productConfig.SoftwareVersion;
        module.ManufacturerModelName = _productConfig.ManufacturerModel;
        module.DeviceSerialNumber = _productConfig.DeviceSerialNumber;
    }

    private void FillSRDocumentGeneral(SRDocumentGeneralModule module)
    {
        module.ContentDate = DateTime.Now;
        module.ContentTime = DateTime.Now;
        module.VerificationFlag = VerificationFlagCS.UNVERIFIED;
        module.CompletionFlag = CompletionFlagCS.COMPLETE;
    }

    private void FillSOPCommon(SOPCommonModule module)
    {
        //TODO: 后期修改，走配置
        module.SpecificCharacterSet = Constants.SPECIFIC_CHARACTER_SET;
        module.SOPInstanceUID = UIDHelper.CreateSOPInstanceUID();
    }

    private DoseReportSRData CreateDoseReportData(StudyModel study, PatientModel patient, ProtocolModel protocol, List<ScanTaskModel> scanTasks)
    {
        //TODO: 参数数据需进一步确认
        //临时, _productConfig.DeviceSerialNumber => 99999
        var deviceSerialNumber = _productConfig.DeviceSerialNumber;
        deviceSerialNumber = "99999";
        var data = new DoseReportSRData
        {
            DeviceObserverUid = string.Join(".", _productConfig.ImplementationClassUID, deviceSerialNumber),
            StartDateTime = study.ExamStartTime.HasValue ? study.ExamStartTime.Value : DateTime.Now,
            EndDateTime = study.ExamEndTime.HasValue ? study.ExamEndTime.Value : DateTime.Now,
            StudyInstanceUID = study.StudyInstanceUID,
            TotalNumberOfIrrEvent = 0,
            DoseLengthProductTotal = 0,
            Comment = string.Empty
        };
        var deviceRole = new CTDeviceRoleParticipant
        {
            ModelName = _productConfig.ManufacturerModel,
            Manufacturer = _productConfig.Manufacturer,
            SerialNumber = _productConfig.DeviceSerialNumber,
            ObserverUID = string.Join(".", _productConfig.ImplementationClassUID, deviceSerialNumber)
        };

        var protocolEntities = ProtocolHelper.Expand(protocol);        

        foreach(var protocolEntity in protocolEntities)
        {
            if (protocolEntity.Scan.Status != CTS.Enums.PerformStatus.Performed) continue;

            var scanTask = scanTasks.FirstOrDefault(s => s.ScanId == protocolEntity.Scan.Descriptor.Id);
            var scanModel = protocolEntity.Scan;

            if (scanTask is null || string.IsNullOrEmpty(scanTask.ActuralParameters)) continue;

            var scanDoseInfo = JsonConvert.DeserializeObject<RealDoseInfo>(scanTask.ActuralParameters);

            var acquisitionData = new CTAcquisitionData
            {
                AquisitionProtocol = protocolEntity.Scan.Descriptor.Name,
                BodyPart = GetMappedBodyPart(protocolEntity.Scan.BodyPart),//人体部位
                AcquisitionType = protocolEntity.Scan.ScanOption,
                IrradiationEventUID = UIDHelper.CreateIrradiationEventUID(),
                IsContrast = scanModel.IsEnhanced,//是否增强
                ExposureTime = (scanDoseInfo is not null) ? Math.Round(scanDoseInfo.TotalExposureTime / Scalar_MS2S ,4) : 0,
                ScanningLength = (scanDoseInfo is not null) ? Math.Round(scanDoseInfo.ScanLength,4) : 0,
                NominalSingleCollimationWidth = (scanDoseInfo is not null) ? Math.Round(scanDoseInfo.SingleCollimationWidth,4) : 0,
                TotalSingleCollimationWidth = (scanDoseInfo is not null) ? Math.Round(scanDoseInfo.TotalCollimationWidth,4) : 0,
                NumberOfXRaySources = (scanDoseInfo is not null && scanDoseInfo.TubeDoses is not null) ? scanDoseInfo.TubeDoses.Count : 0,
                PitchFactor = Math.Round(protocolEntity.Scan.Pitch / 100.0,2),
                CTDeviceRoleParticipant = deviceRole,                
            };

            data.TotalNumberOfIrrEvent += 1;
            data.DoseLengthProductTotal += (scanDoseInfo is not null) ? scanDoseInfo.DLP : 0;

            acquisitionData.CTDoseInfo = GetCTDoseInfo(scanModel, scanDoseInfo,protocol.IsAdult);

            foreach (var tubeDose in scanDoseInfo?.TubeDoses)
            {
                var xraySource = new XRaySourceParam
                {
                    IdentificationXRaySource = tubeDose.Number.ToString(),
                    KVP = Math.Round(tubeDose.KVP, 4),
                    TubeCurrent = Math.Round(tubeDose.MeanMA, 4),
                    MaxTubeCurrent = Math.Round(tubeDose.MaxMA, 4),
                    ExposureTimePerRotate = Math.Round(tubeDose.ExposureTimePerRotate / Scalar_MS2S,4)
                };
                acquisitionData.XRaySourceParams.Add(xraySource);
            }
            data.CTAcquisitionDatas.Add(acquisitionData);
        }
        data.DoseLengthProductTotal = Math.Round(data.DoseLengthProductTotal, 4);
        return data;
    }

    private CTDoseInfo GetCTDoseInfo(ScanModel scanModel, RealDoseInfo realDoseInfo,bool isAdult)
    {
        var doseInfo = new CTDoseInfo();
        doseInfo.MeanCTDIvol = Math.Round(realDoseInfo.CTDIvol, 4);
        doseInfo.CTDIwPhantomType = (int)realDoseInfo.PhantomType;
        doseInfo.DLP = Math.Round(realDoseInfo.DLP,4);

        doseInfo.DoseCheckAlert = GetDoseAlertInfo(scanModel,isAdult);

        doseInfo.DoseCheckNotification = GetDoseNotificationInfo(scanModel, isAdult);

        return doseInfo;
    }

    private DoseCheckAlert GetDoseAlertInfo(ScanModel scanModel, bool isAdult)
    {
        //剂量警告应始终开启
        var doseAlert = new DoseCheckAlert();

        doseAlert.CTDIvolAlertConfigured = true;//剂量警告CTDI应始终开启

        if (isAdult)
        {
            doseAlert.CTDIvolAlertValue = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertCTDIThreshold.Value;
        }
        else
        {
            doseAlert.CTDIvolAlertValue = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertCTDIThreshold.Value;
        }
        doseAlert.AccumulatedCTDIvolForwardEstimate = scanModel.AccumulatedDoseEstimatedCTDI;
        if (doseAlert.AccumulatedCTDIvolForwardEstimate > doseAlert.CTDIvolAlertValue)
        {
            doseAlert.ReasonForProceeding = scanModel.DoseAlertReason;
            doseAlert.PersonName = scanModel.DoseAlertOperator;
            doseAlert.PersonRole = 0;
        }

        if (isAdult)
        {
            doseAlert.DLPAlertConfigured = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertDLPThreshold.Value != 0;
            if (doseAlert.DLPAlertConfigured)
            {
                doseAlert.DLPAlertValue = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertDLPThreshold.Value;
            }
        }
        else
        {
            doseAlert.DLPAlertConfigured = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertDLPThreshold.Value != 0;
            if (doseAlert.DLPAlertConfigured)
            {
                doseAlert.DLPAlertValue = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertDLPThreshold.Value;
            }
        }
        if (doseAlert.DLPAlertConfigured)
        {
            doseAlert.AccumulatedDLPForwardEstimate = scanModel.AccumulatedDoseEstimatedDLP;
            if (doseAlert.AccumulatedDLPForwardEstimate > doseAlert.DLPAlertValue)
            {
                doseAlert.ReasonForProceeding = scanModel.DoseAlertReason;
                doseAlert.PersonName = scanModel.DoseAlertOperator;
                doseAlert.PersonRole = 0;
            }
        }


        return doseAlert;
    }

    private DoseCheckNotification GetDoseNotificationInfo(ScanModel scanModel, bool isAdult)
    {

        var doseNoti = new DoseCheckNotification();
        if (UserConfig.DoseSettingConfig.DoseSetting.NotificationEnabled.Value)
        {
            if (scanModel.DoseNotificationCTDI != 0)
            {
                doseNoti.CTDIvolNotificationConfigured = true;
                doseNoti.CTDIolNotificationValue = scanModel.DoseNotificationCTDI;
                doseNoti.CTDIvolForwardEstimate = scanModel.DoseEstimatedCTDI;

                if (doseNoti.CTDIvolForwardEstimate > doseNoti.CTDIolNotificationValue)
                {
                    doseNoti.PersonName = scanModel.DoseNotificationOperator;
                    doseNoti.ReasonForProceeding = scanModel.DoseNotificationReason;
                    doseNoti.PersonRole = 0;
                }
            }
            if (scanModel.DoseNotificationDLP != 0)
            {
                doseNoti.DLPNotificationConfigured = true;
                doseNoti.DLPNotificationValue = scanModel.DoseNotificationDLP;
                doseNoti.DLPForwardEstimate = scanModel.DoseEstimatedDLP;
                if (doseNoti.DLPForwardEstimate > doseNoti.DLPNotificationValue)
                {
                    doseNoti.PersonName = scanModel.DoseNotificationOperator;
                    doseNoti.ReasonForProceeding = scanModel.DoseNotificationReason;
                    doseNoti.PersonRole = 0;
                }
            }
        }
        return doseNoti;
    }

    private BodyPart GetMappedBodyPart(NV.CT.CTS.Enums.BodyPart part)
    {
        switch (part)
        {
            case CTS.Enums.BodyPart.Spine:
                return BodyPart.SPINE;
            case CTS.Enums.BodyPart.Shoulder:
                return BodyPart.SHOULDER;
            case CTS.Enums.BodyPart.Pelvis:
                return BodyPart.PELVIS;
            case CTS.Enums.BodyPart.Neck:
                return BodyPart.NECK;
            case CTS.Enums.BodyPart.Head:
                return BodyPart.HEAD;
            case CTS.Enums.BodyPart.Leg:
                return BodyPart.LEG;
            case CTS.Enums.BodyPart.Arm:
                return BodyPart.ARM;
            case CTS.Enums.BodyPart.Abdomen:
                return BodyPart.ABDOMEN;
            case CTS.Enums.BodyPart.Breast:
                return BodyPart.BREAST;
            default:
                return BodyPart.WHOLEBODY;
        }

    }
}
