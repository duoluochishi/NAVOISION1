//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/4 13:49:16     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using DoseReport.DicomModule;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DicomUtility.BlackImage;
using NV.CT.DicomUtility.BlackImageForSR;
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

public class BlackImageReport
{
    private readonly ILogger<BlackImageReport> _logger;
    private readonly ISeriesService _seriesService;
    private readonly ProductSettingInfo _productConfig;
    private readonly IAuthorization _authorization;

    public BlackImageReport(ILogger<BlackImageReport> logger, ISeriesService seriesService, IAuthorization authorization)
    {
        _logger = logger;
        _seriesService = seriesService;
        _authorization = authorization;
        _productConfig = NV.MPS.Configuration.ProductConfig.ProductSettingConfig.ProductSetting;
    }

    public void CreateReport(StudyModel study, PatientModel patient, ProtocolModel protocol, List<ScanTaskModel> scanTasks)
    {
        var iod = new SecondaryCaptureImageIOD();

        var seriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();

        FillPatientModule(iod.PatientModule, patient);
        FillStudyModule(iod.GeneralStudyModule, study);
        FillGeneralSeriesModule(iod.GeneralSeriesModule, seriesInstanceUID);
        FillGeneralEquipment(iod.GeneralEquipmentModule);
        FillGeneralImageModule(iod.GeneralImageModule);
        FillImagePixelModule(iod.ImagePixelModule);
        FillSOPCommonForSC(iod.SOPCommonModule);
        FillCTImageForSC(iod.CTImageModule);
        FillImagePlaneForSC(iod.ImagePlaneModule);
        FillVOILUTForSC(iod.VOILUTModule);


        var instanceData = CreateBlackImageReportData(study, patient, protocol, scanTasks);
        var filePath = Path.Combine(RuntimeConfig.Console.ReportData.Path, "..", study.StudyInstanceUID, $"902_{seriesInstanceUID}");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var fileName = Path.Combine(filePath, "BlackImageReport.dcm");

        File.WriteAllText(Path.Combine(filePath, "BlackImageReport.json"), JsonConvert.SerializeObject(new { Patient = patient, Study = study, Scans = scanTasks, IOD = iod, Data = instanceData }));
        _logger.LogDebug($"DoseInfo.CreateBlackImageReport study id : {study.Id}");
        BlackImageGenerator.Instance.GenerateBlackImage(iod, instanceData, fileName);
        InsertSeries(study, patient, protocol, seriesInstanceUID, fileName);
    }

    private void InsertSeries(StudyModel study, PatientModel patient, ProtocolModel protocol, string seriesInstanceUID, string fileName)
    {
        var series = new SeriesModel();
        series.Id = Guid.NewGuid().ToString();
        series.InternalStudyId = study.Id;
        series.BodyPart = $"{protocol.BodyPart}";
        series.ProtocolName = protocol.Descriptor.Name;

        series.SeriesNumber = "902";

        series.SeriesInstanceUID = seriesInstanceUID;
        series.ScanId = string.Empty;
        series.ReconId = string.Empty;
        series.ImageCount = 1;
        series.ReconEndDate = DateTime.Now;
        series.SeriesType = "DoseReport";
        series.SeriesPath = fileName;
        series.SeriesDescription = "Dose Report";

        series.FrameOfReferenceUID = string.Empty;
        series.PatientPosition = string.Empty;

        series.ImageType = "DoseReport";
        series.WindowWidth = string.Empty;
        series.WindowLevel = string.Empty;

        _seriesService.Add(series);
    }

    private void FillPatientModule(PatientModule module, PatientModel patient)
    {
        module.PatientName = patient.PatientName;
        module.PatientBirthDate = DateTime.Now;
        module.PatientID = patient.PatientId;
        module.PatientSex = (PatientSexCS)Enum.Parse(typeof(PatientSexCS), patient.PatientSex.ToString().Substring(0, 1));
    }

    private void FillStudyModule(GeneralStudyModule module, StudyModel study)
    {
        module.StudyInstanceUID = study.StudyInstanceUID;
        module.StudyDate = study.StudyDate;
        module.StudyTime = study.StudyTime;
        module.StudyID = !string.IsNullOrEmpty(study.StudyId) ? study.StudyId : string.Empty; ;
        module.AccessionNumber = study.AccessionNo;
    }

    private void FillGeneralSeriesModule(GeneralSeriesModule seriesModule, string instanceUID)
    {
        seriesModule.Modality = ModalityCS.CT;
        seriesModule.SeriesInstanceUID = instanceUID;
        seriesModule.SeriesNumber = 902;
        seriesModule.SeriesDescription = "Dose Report";
    }

    private void FillGeneralImageModule(GeneralImageModule module)
    {
        module.InstanceNumber = 1;
        module.ImageType = @"ORIGINAL\PRIMARY\OTHER";
    }

    private void FillImagePixelModule(ImagePixelModule module)
    {
        module.SamplesPerPixel = 1;
        module.PhotometricInterpretation = PhotometricInterpretationCS.MONOCHROME2;
        module.Rows = 512;
        module.Columns = 512;
        module.BitsAllocated = 16;
        module.BitsStored = 12;
        module.HighBit = 11;
        module.PixelRepresentation = 0;
    }

    private void FillGeneralEquipment(GeneralEquipmentModule module)
    {
        //todo:走配置
        module.Manufacturer = _productConfig.Manufacturer;
    }

    private void FillSOPCommonForSC(SOPCommonModule module)
    {
        //todo:走配置
        module.SOPClassUID = UIDHelper.CreateSOPClassUID();
        module.SpecificCharacterSet = Constants.SPECIFIC_CHARACTER_SET;
        module.SOPInstanceUID = UIDHelper.CreateSOPInstanceUID();
    }

    private void FillCTImageForSC(CTImageModule module)
    {
        module.ImageType = @"ORIGINAL\PRIMARY\OTHER";
        module.SamplesPerPixel = 1;
        module.PhotometricInterpretation = PhotometricInterpretationCS.MONOCHROME2;
        module.BitsAllocated = 16;
        module.BitsStored = 12;
        module.HighBit = 11;
        module.RescaleIntercept = 0;
        module.RescaleSlope = 1;
    }

    private void FillImagePlaneForSC(ImagePlaneModule module)
    {
        module.PixelSpacing[0] = 1;
        module.PixelSpacing[1] = 1;
        module.ImagePosition[0] = 0;
        module.ImagePosition[1] = 0;
        module.ImagePosition[2] = 0;
        module.ImageOrientation[0] = 1;
        module.ImageOrientation[1] = 0;
        module.ImageOrientation[2] = 0;
        module.ImageOrientation[3] = 0;
        module.ImageOrientation[4] = 1;
        module.ImageOrientation[5] = 0;
    }

    private void FillVOILUTForSC(VOILUTModule module)
    {
        module.WindowCenter = new double[] { 128 };
        module.WindowWidth = new double[] { 256 };
    }

    private BlackImageConentInfo CreateBlackImageReportData(StudyModel study, PatientModel patient, ProtocolModel protocol, List<ScanTaskModel> scanTasks)
    {
        BlackImageConentInfo info = new BlackImageConentInfo()
        {
            StudyID = study.StudyId,
            TotalDLP = 0,
            StudyTime = study.StudyTime,
            StudyInstanceUID = study.StudyInstanceUID
        };

        var protocolEntities = ProtocolHelper.Expand(protocol);
        var index = 0;
        foreach (var protocolEntity in protocolEntities)
        {
            if (protocolEntity.Scan.Status != CTS.Enums.PerformStatus.Performed) continue;

            var scanTask = scanTasks.FirstOrDefault(s => s.ScanId == protocolEntity.Scan.Descriptor.Id);

            if (scanTask is null || string.IsNullOrEmpty(scanTask.ActuralParameters)) continue;

            index++;
            var scanDoseInfo = JsonConvert.DeserializeObject<RealDoseInfo>(scanTask.ActuralParameters);

            //todo:数据源待确认
            var acqDoseInfo = new ACQDoseInfoForBlackImage
            {
                Index = index,
                SeriesDescription = $"{scanTask.Description}",
                ScanMode = protocolEntity.Scan.ScanOption.ToString().Replace("NV", ""),
                RotateTime = (scanDoseInfo is not null) ? scanDoseInfo.TotalExposureTime : 0,
                CTDIvol = (scanDoseInfo is not null) ? scanDoseInfo.CTDIvol : 0,
                DLP = (scanDoseInfo is not null) ? scanDoseInfo.DLP : 0,
                PhantomType = (scanDoseInfo is not null) ?
                DoseReportSRHelper.GetPhantomTypeDescription((int)scanDoseInfo.PhantomType).Item3 : string.Empty
            };

            if (scanDoseInfo is not null && scanDoseInfo.TubeDoses is not null && scanDoseInfo.TubeDoses.Count > 0)
            {
                var tubeLength = scanDoseInfo.TubeDoses.Count;
                //使用实际曝光参数中的MeanMA与单圈曝光时间，螺旋扫描需要考虑pitch。
                switch(protocolEntity.Scan.ScanOption)
                {
                    case ScanOption.Surview:        //定位像使用协议中ma与曝光时间的积
                        acqDoseInfo.MAs = protocolEntity.Scan.Milliampere[0] * UnitConvert.Microsecond2Second(protocolEntity.Scan.ExposureTime*1.0);
                        break;
                    case ScanOption.DualScout:
                        acqDoseInfo.MAs = (protocolEntity.Scan.Milliampere[0] + protocolEntity.Scan.Milliampere[1]) * UnitConvert.Microsecond2Second(protocolEntity.Scan.ExposureTime*1.0); 
                        break;
                    case ScanOption.Helical:    
                        acqDoseInfo.MAs = scanDoseInfo.TubeDoses.Sum(t => t.MeanMA * t.ExposureTimePerRotate /1000.0) * 100 / protocolEntity.Scan.Pitch;
                        break;
                    default:        //
                        acqDoseInfo.MAs = scanDoseInfo.TubeDoses.Sum(t => t.MeanMA * t.ExposureTimePerRotate / 1000.0);
                        break;
                }
                //acqDoseInfo.KV = scanDoseInfo.TubeDoses.Sum(t => t.KVP) / tubeLength;
                acqDoseInfo.KV = protocolEntity.Scan.Kilovolt[0];
                //todo:待确认参数
                acqDoseInfo.Cycles = 1;
            }

            info.TotalDLP += acqDoseInfo.DLP;

            info.AcqDoseInfos.Add(acqDoseInfo);
        }

        #region 代码实现后废弃
        //ACQDoseInfoForBlackImage acq1 = new ACQDoseInfoForBlackImage()
        //{
        //    Index = 1,
        //    SeriesDescription = "This is a test",
        //    ScanMode = "Preview",
        //    MAs = 1.01010101,
        //    KV = 1.0101010101,
        //    Cycles = 1,
        //    RotateTime = 1,
        //    CTDIvol = 1.0101010101,
        //    DLP = 101.0101,
        //    PhantomType = "Preview"
        //};

        //ACQDoseInfoForBlackImage acq2 = new ACQDoseInfoForBlackImage()
        //{
        //    Index = 2,
        //    SeriesDescription = "MMMMMMMMMMMMM",
        //    ScanMode = "Spiral",
        //    MAs = 2.01010101,
        //    KV = 2.0101010101,
        //    Cycles = 2,
        //    RotateTime = 2,
        //    CTDIvol = 2.0101010101,
        //    DLP = 222.0101,
        //    PhantomType = "Spiral"
        //};

        //ACQDoseInfoForBlackImage acq3 = new ACQDoseInfoForBlackImage()
        //{
        //    Index = 3,
        //    SeriesDescription = "This is a long sentence for test",
        //    ScanMode = "Sequence",
        //    MAs = 3.01010101,
        //    KV = 3.0101010101,
        //    Cycles = 3,
        //    RotateTime = 3,
        //    CTDIvol = 3.0101010101,
        //    DLP = 333.0101,
        //    PhantomType = "Sequence"
        //};
        //info.AcqDoseInfos.Add(acq1);
        //info.AcqDoseInfos.Add(acq2);
        //info.AcqDoseInfos.Add(acq3);
        #endregion

        return info;
    }
}