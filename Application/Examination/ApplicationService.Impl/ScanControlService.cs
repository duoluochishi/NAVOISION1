//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;

namespace NV.CT.Examination.ApplicationService.Impl;

public class ScanControlService : IScanControlService
{
    private readonly ILogger<ScanControlService> _logger;
    private readonly IMapper _mapper;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IStudyHostService _studyHost;
    private readonly IRealtimeReconProxyService _realtimeProxyService;
    private readonly IScanStatusService _scanStatusService;
    private readonly IMeasurementStatusService _measurementStatusService;
    private readonly IAuthorization _authorizationService;

    public ScanControlService(ILogger<ScanControlService> logger, IMapper mapper, IStudyHostService studyHost, IProtocolHostService protocolHostService, IScanStatusService scanStatusService, IMeasurementStatusService measurementStatusService, IRealtimeReconProxyService realtimeProxyService, IAuthorization authorizationService)
    {
        _logger = logger;
        _mapper = mapper;
        _studyHost = studyHost;
        _protocolHostService = protocolHostService;
        _scanStatusService = scanStatusService;
        _realtimeProxyService = realtimeProxyService;
        _measurementStatusService = measurementStatusService;
        _authorizationService = authorizationService;
    }

    public RealtimeCommandResult CancelMeasurement()
    {
        var cmdResult = _realtimeProxyService.AbortScan(FacadeProxy.Common.Enums.AbortCause.UserAbort, true);
        if (cmdResult.Status != CommandExecutionStatus.Success)
        {
            _logger.LogInformation("ScanControlService.AbortScan result: {0}", JsonConvert.SerializeObject(cmdResult));
        }
        _scanStatusService.CancelMeasurement();
        return cmdResult;
    }

    public RealtimeCommandResult StartMeasurement(string measurementId)
    {
        var activeItem = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Descriptor.Id == measurementId);

        var parameters = activeItem.Measurement.Children.Select(scanEntity => new ScanReconParam
        {
            Patient = GetPatient(),
            Study = GetStudy(),
            ScanParameter = GetScanParameter(scanEntity),
            ReconSeriesParams = GetReconSeriesParameters(activeItem.Frame, scanEntity)
        }).ToList();

        _scanStatusService.CurrentScan = activeItem.Measurement.Children.FirstOrDefault();
        _scanStatusService.CurrentMeasurement = activeItem.Measurement;

        var recons = new List<ReconModel>();
        foreach (var scanItem in activeItem.Measurement.Children)
        {
            var reconItems = scanItem.Children.Where(t => t.IsRTD).ToList();

            if (reconItems is not null)
            {
                recons.AddRange(reconItems);
            }
        }

        var cmdResult = _realtimeProxyService.StartScan(parameters);
        if (cmdResult.Status != CommandExecutionStatus.Success)
        {
            _logger.LogInformation("ScanControlService.StarScan result: {0}", cmdResult is null ? "NULL" : JsonConvert.SerializeObject(cmdResult));
            _measurementStatusService.RaiseMeasurementLoadingFailed(measurementId);
        }
        else
        {
            _measurementStatusService.RaiseMeasurementLoaded(measurementId);
        }

        return cmdResult;
    }

    private Patient GetPatient()
    {
        //todo:临时代码，显示版本
        var patient = new Patient
        {
            AdditionalHistory = string.Empty,  //VersionHelper.GetSoftwareVersion(), // "Additional Patient History",
            ContrastAller = string.Empty,
            EthnicGroup = string.Empty,
            MedicalAlerts = string.Empty,
            OtherPatientIDs = string.Empty,
            PatientStatus = string.Empty,
            PregnancyStatus = PregnantStatus.Unknown,
        };
        patient.ID = _studyHost.Instance.PatientID;
        patient.Comment = _studyHost.Instance.Comments;
        patient.Name = _studyHost.Instance.PatientName;
        patient.Sex = (PatientSex)((int)_studyHost.Instance.PatientSex);
        patient.Size = _studyHost.Instance.PatientSize;
        patient.Weight = _studyHost.Instance.PatientWeight;
        patient.Age = $"{_studyHost.Instance.Age.ToString("D3")}{((AgeType)(_studyHost.Instance.AgeType)).ToString().Substring(0, 1)}";
        patient.BirthDate = _studyHost.Instance.Birthday;
        patient.BirthTime = _studyHost.Instance.Birthday;
        return patient;
    }

    private Study GetStudy()
    {
        var study = new Study
        {
            NameOfPhysiciansReadingStudy = string.Empty,
            PhysiciansOfRecord = string.Empty,
            ReferringPhysiciansName = string.Empty,
            RequestedProcedureDescription = string.Empty,
            RequestedProcedureID = string.Empty,
            RequestedProcedurePriority = RequestedProcedurePriority.HIGH,
            RequestingPhysician = string.Empty,
            RequestingService = string.Empty,
            SpecialNeeds = string.Empty,
        };
        study.StudyID = _studyHost.Instance.StudyID; //StudyID有可能未空：系统内部此值基本上为空，外部来源可能会有值
        study.StudyInstanceUID = _studyHost.Instance.StudyInstanceUID;
        study.AccessionNumber = _studyHost.Instance.AccessionNo;
        study.StudyDate = _studyHost.Instance.StudyDate;
        study.StudyTime = _studyHost.Instance.StudyTime;

        if (!string.IsNullOrEmpty(_studyHost.Instance.StudyDescription))
        {
            study.StudyDescription = _studyHost.Instance.StudyDescription;
        }
        else
        {
            study.StudyDescription = _protocolHostService.Instance.Descriptor.Name;
        }

        var loginName = _authorizationService.GetCurrentUser()?.UserName;
        loginName = string.IsNullOrEmpty(loginName) ? string.Empty : loginName;

        study.OperatorsName = loginName;

        return study;
    }

    private ScanParam GetScanParameter(ScanModel scanEntity)
    {
        var parameter = new ScanParam();

        parameter.ScanBinning = scanEntity.Binning;
        parameter.PreVoiceDelayTime = scanEntity.IsVoiceSupported ? scanEntity.PreVoiceDelayTime : 0;
        parameter.PreVoiceID = scanEntity.IsVoiceSupported ? scanEntity.PreVoiceId : 0;
        parameter.PostVoiceID = scanEntity.IsVoiceSupported ? scanEntity.PostVoiceId : 0;

        parameter.ScanUID = scanEntity.Descriptor.Id;
        parameter.AutoScan = scanEntity.AutoScan;
        parameter.ScanNumber = (uint)scanEntity.ScanNumber;
        for (var itemIndex = 0; itemIndex < 8; itemIndex++)
        {
            parameter.kV[itemIndex] = scanEntity.Kilovolt[itemIndex];
            parameter.mA[itemIndex] = scanEntity.Milliampere[itemIndex] * 1000;
        }
        parameter.FrameTime = scanEntity.FrameTime;
        parameter.FramesPerCycle = scanEntity.FramesPerCycle;
        
        parameter.ScanOption = scanEntity.ScanOption;
        parameter.ScanMode = scanEntity.ScanMode;
        parameter.ExposureTriggerMode = scanEntity.ExposureTrigger;

        if (scanEntity.ScanOption is ScanOption.Surview or ScanOption.DualScout)
        {
            parameter.PreOffsetFrameTime = (uint)SystemConfig.AcquisitionConfig.Acquisition.TopoOffsetFrameTime.Value;
        }
        else
        {
            parameter.PreOffsetFrameTime = scanEntity.FrameTime;
        }

        parameter.AutoDeleteNum = scanEntity.AutoDeleteNum;
        parameter.TotalFrames = scanEntity.TotalFrames;

        parameter.RawDataType = scanEntity.RawDataType;

        parameter.ExposureDelayTime = scanEntity.ExposureDelayTime;
        parameter.ExposureTime = scanEntity.ExposureTime;
        parameter.ExposureMode = scanEntity.ExposureMode;

        parameter.CollimatorSliceWidth = scanEntity.CollimatorSliceWidth;
        parameter.CollimatorOpenMode = scanEntity.CollimatorOpenMode;
        parameter.CollimatorZ = scanEntity.CollimatorZ;

        parameter.BowtieEnable = scanEntity.BowtieEnable;

        parameter.ExposureStartPosition = scanEntity.ExposureStartPosition;
        parameter.ExposureEndPosition = scanEntity.ExposureEndPosition;

        parameter.ReconVolumeStartPosition = scanEntity.ReconVolumeStartPosition;
        parameter.ReconVolumeEndPosition = scanEntity.ReconVolumeEndPosition;

        parameter.TableDirection = scanEntity.TableDirection;
        parameter.TableStartPosition = scanEntity.TableStartPosition;
        parameter.TableEndPosition = scanEntity.TableEndPosition;

        parameter.TableHeight = scanEntity.TableHeight;
        parameter.TableAcceleration = scanEntity.TableAcceleration;
        parameter.TableAccelerationTime = scanEntity.TableAccelerationTime;
        parameter.TableSpeed = scanEntity.TableSpeed;
        parameter.TableFeed = scanEntity.TableFeed;

        parameter.GantryDirection = scanEntity.GantryDirection;
        parameter.GantryStartPosition = scanEntity.GantryStartPosition;
        parameter.GantryEndPosition = scanEntity.GantryEndPosition;
        parameter.GantryAcceleration = scanEntity.GantryAcceleration;
        parameter.GantryAccelerationTime = scanEntity.GantryAccelerationTime;
        parameter.GantrySpeed = scanEntity.GantrySpeed;

        parameter.Gain = scanEntity.Gain;
        parameter.BodyPart = _mapper.Map<FacadeProxy.Common.Enums.BodyPart>(scanEntity.BodyPart);
        parameter.BodyCategory = _mapper.Map<FacadeProxy.Common.Enums.ScanEnums.BodyCategory>(_protocolHostService.Instance.BodySize);

        //球管位置及对应球管编号（定位像、双定位像）
        parameter.TubePositions[0] = scanEntity.TubePositions[0];
        parameter.TubePositions[1] = scanEntity.TubePositions[1];
        parameter.TubeNumbers[0] = scanEntity.TubeNumbers[0];
        parameter.TubeNumbers[1] = scanEntity.TubeNumbers[1];

        //TODO:此参数带上后，图片数量会成倍增加，待确认
        //parameter.DoseCurve = scanEntity.DoseCurve;
        parameter.Pitch = scanEntity.Pitch;
        parameter.PreOffsetFrames = scanEntity.PreOffsetFrames;
        parameter.PostOffsetFrames = scanEntity.PostOffsetFrames;
        parameter.Loops = scanEntity.Loops;
        parameter.LoopTime = scanEntity.LoopTime;
        parameter.ScanFOV = scanEntity.ScanFOV;

        parameter.RDelay = scanEntity.RDelay;
        parameter.TDelay = scanEntity.TDelay;
        parameter.SpotDelay = scanEntity.SpotDelay;

        parameter.Focal = scanEntity.FocalType;
        parameter.FunctionMode = scanEntity.FunctionMode;

        parameter.SmallAngleDeleteLength = scanEntity.SmallAngleDeleteLength;
        parameter.LargeAngleDeleteLength = scanEntity.LargeAngleDeleteLength;

        //默认开启
        parameter.CollimatorOffsetEnable = true;

        parameter.ActiveExposureSourceCount = scanEntity.ActiveExposureSourceCount == 0 ? 24 : scanEntity.ActiveExposureSourceCount;

        //todo:规则待定
        if (scanEntity.ScanOption is ScanOption.Surview or ScanOption.DualScout or ScanOption.NVTestBolus or ScanOption.NVTestBolusBase or ScanOption.TestBolus)
        {
            parameter.AllowErrorXRaySourceCount = 0;
        }
        else
        {
            //todo:临时调整；
            if (parameter.ActiveExposureSourceCount == 24 || parameter.ActiveExposureSourceCount == 16)
            {
                parameter.AllowErrorXRaySourceCount = scanEntity.AllowErrorTubeCount;
            }
            else
            {
                parameter.AllowErrorXRaySourceCount = 0;
            }
        }

        parameter.mAs = 0;
        parameter.CTDIvol = scanEntity.DoseEffectiveCTDI;

        return parameter;
    }

    private ReconSeriesParam[] GetReconSeriesParameters(FrameOfReferenceModel frameModel, ScanModel scanModel)
    {
        var reconModels = scanModel.Children.Where(recon => recon.IsRTD).ToList();
        if (reconModels is null || reconModels.Count == 0)
        {
            return default!;
        }

        var reconInfoes = new List<ReconSeriesParam>();

        foreach (var recon in reconModels)
        {
            var reconInfo = new ReconSeriesParam();
            reconInfo.ReconID = recon.Descriptor.Id;
            reconInfo.ReconType = recon.ReconType;
            reconInfo.PreBinning = recon.PreBinning;

            //处理重建部位和扫描部位不一致
            if (recon.BodyPart.HasValue)
            {
                reconInfo.ReconBodyPart = _mapper.Map<FacadeProxy.Common.Enums.BodyPart>(recon.BodyPart.Value);
            }

            //todo:确认是否走上层处理
            if (recon.IVRTVCoef != 0)
            {
                reconInfo.IVRTVCoef = recon.IVRTVCoef;
            }
            else
            {
                //todo: 读取配置
                var coefficent = SystemConfig.GetTVCoefficientInfo(scanModel.BodyPart.ToString(), recon.WindowType.ToString());
                reconInfo.IVRTVCoef = coefficent.Factor;
            }

            reconInfo.AirCorrectionMode = recon.AirCorrectionMode;
            reconInfo.FilterType = recon.FilterType;
            reconInfo.InterpType = recon.InterpType;

            reconInfo.BoneAritifactEnable = recon.BoneAritifactEnable;
            reconInfo.MetalAritifactEnable = recon.MetalAritifactEnable;
            reconInfo.WindmillArtifactReduceEnable = recon.WindmillArtifactEnable;
            reconInfo.ConeAngleArtifactReduceEnable = recon.ConeAngleArtifactEnable;

            reconInfo.SliceThickness = recon.SliceThickness;

            reconInfo.PreDenoiseType = recon.PreDenoiseType;
            reconInfo.PreDenoiseCoef = recon.PreDenoiseCoef;
            reconInfo.PostDenoiseType = recon.PostDenoiseType;
            reconInfo.PostDenoiseCoef = recon.PostDenoiseCoef;

            reconInfo.WindowCenter = recon.WindowCenter;
            reconInfo.WindowWidth = recon.WindowWidth;
            reconInfo.CenterFirstX = recon.CenterFirstX;
            reconInfo.CenterFirstY = recon.CenterFirstY;
            reconInfo.CenterFirstZ = recon.CenterFirstZ;
            reconInfo.CenterLastX = recon.CenterLastX;
            reconInfo.CenterLastY = recon.CenterLastY;
            reconInfo.CenterLastZ = recon.CenterLastZ;

            if (recon.Parent.ScanOption == ScanOption.NVTestBolus)
            {
                reconInfo.NVTestBolusBaseImagePath = recon.TestBolusBaseImagePath;
            }

            reconInfo.ScatterAlgorithm = recon.ScatterAlgorithm;
            reconInfo.RingAritifactEnable = recon.RingAritifactEnable;
            reconInfo.RingCorrectCoef = recon.RingCorrectionCoef;
            reconInfo.SmoothZEnable = recon.SmoothZEnable;
            reconInfo.MinTablePosition = recon.MinTablePosition;
            reconInfo.MaxTablePosition = recon.MaxTablePosition;
            reconInfo.TwoPassEnable = recon.TwoPassEnable;

            reconInfo.IsTargetRecon = recon.IsTargetRecon;

            reconInfo.FoVDirectionHorX = recon.FOVDirectionHorizontalX;
            reconInfo.FoVDirectionHorY = recon.FOVDirectionHorizontalY;
            reconInfo.FoVDirectionHorZ = recon.FOVDirectionHorizontalZ;
            reconInfo.FoVDirectionVertX = recon.FOVDirectionVerticalX;
            reconInfo.FoVDirectionVertY = recon.FOVDirectionVerticalY;
            reconInfo.FoVDirectionVertZ = recon.FOVDirectionVerticalZ;
            reconInfo.FoVLengthHor = recon.FOVLengthHorizontal;
            reconInfo.FoVLengthVert = recon.FOVLengthVertical;
            reconInfo.ImageMatrixHor = recon.ImageMatrixHorizontal;
            reconInfo.ImageMatrixVert = recon.ImageMatrixVertical;
            reconInfo.ImageIncrement = recon.ImageIncrement;
            reconInfo.SeriesDescription = string.IsNullOrEmpty(recon.SeriesDescription.Trim()) ? recon.DefaultSeriesDescription.Trim() : recon.SeriesDescription.Trim();
            reconInfo.AcquisitionDate = DateTime.Now;
            reconInfo.AcquisitionTime = DateTime.Now;
            reconInfo.SeriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();
            reconInfo.SOPClassUID = UIDHelper.CreateSOPClassUID();
            reconInfo.SOPClassUIDHeader = IdGenerator.Next(7);
            reconInfo.SpecificCharacterSet = Constants.SPECIFIC_CHARACTER_SET;
            reconInfo.Modality = "CT";
            reconInfo.SeriesDate = DateTime.Now;
            reconInfo.SeriesTime = DateTime.Now;
            reconInfo.FrameOfReferenceUID = frameModel.Descriptor.Id;
            reconInfo.PatientPosition = frameModel.PatientPosition;
            reconInfo.ProtocolName = _protocolHostService.Instance.Descriptor.Name;
            reconInfo.SeriesNumber = recon.SeriesNumber;
            reconInfo.IsHDRecon = recon.IsHDRecon;

            if (recon.CycleROIs.Any())
            {
                reconInfo.NVTestBolusROIs = new List<FacadeProxy.Common.Models.ScanRecon.CircleROI>();
                reconInfo.NVTestBolusROIs.AddRange(recon.CycleROIs.Select(t => new FacadeProxy.Common.Models.ScanRecon.CircleROI
                {
                    CenterX = t.CenterX,
                    CenterY = t.CenterY,
                    Radius = t.Radius
                }));
            }

            reconInfoes.Add(reconInfo);
        }
        return reconInfoes.ToArray();
    }
}