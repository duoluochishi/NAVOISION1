using AutoMapper;
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.Alg.ScanReconCalculation.Recon.Target;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.PostProcess;
using NV.CT.FacadeProxy.Common.Models.PostProcess.Abstract;
using NV.CT.JobService.Contract;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using System.IO;

namespace NV.CT.JobService;

public class OfflineTaskService : IOfflineTaskService
{
    private readonly ILogger<OfflineTaskService> _logger;
    private readonly IMapper _mapper;
    private readonly IOfflineTaskProxyService _offlineProxyService;
    private readonly IStudyService _studyService;
    private readonly IReconTaskService _reconTaskService;
    private readonly ISeriesService _seriesService;
    private readonly IAuthorization _authorizationService;

    public OfflineTaskService(ILogger<OfflineTaskService> logger, IMapper mapper, IOfflineTaskProxyService offlineProxyService, IStudyService studyService, IReconTaskService reconTaskService, ISeriesService seriesService, IAuthorization authorizationService)
    {
        _logger = logger;
        _mapper = mapper;
        _studyService = studyService;
        _reconTaskService = reconTaskService;
        _seriesService = seriesService;
        _offlineProxyService = offlineProxyService;
        _authorizationService = authorizationService;

        _offlineProxyService.TaskStatusChanged += OnProxyService_TaskStatusChanged;
        _offlineProxyService.TaskDone += OnProxyService_TaskDone;
        _offlineProxyService.ImageProgressChanged += OnProxyService_ImageProgressChanged;
        _offlineProxyService.ProgressChanged += OnProxyService_ProgressChanged;
    }

    public (OfflineDiskInfo SystemDisk, OfflineDiskInfo AppDisk, OfflineDiskInfo DataDisk) GetDiskInfo()
    {
        return _offlineProxyService.GetDiskInfo();
    }

    public OfflineTaskInfo GetTask(string reconId)
    {
        var result = _offlineProxyService.GetTask(reconId);
        if (result is null)
        {
            _logger.LogInformation($"OfflineTaskService.GetTask, no data: {reconId}");
        }
        return result;
    }

    public OfflineCommandResult IncreaseTaskPriority(string reconId)
    {
        return _offlineProxyService.IncreaseTaskPriority(reconId).Result;
    }

    public OfflineCommandResult DecreaseTaskPriority(string reconId)
    {
        return _offlineProxyService.DecreaseTaskPriority(reconId).Result;
    }

    public OfflineCommandResult CreateTask(string studyId, string scanId, string reconId)
    {
        if (string.IsNullOrEmpty(studyId) || string.IsNullOrEmpty(scanId) || string.IsNullOrEmpty(reconId))
        {
            //TODO: 待处理（含日志）
            _logger.LogError($"OfflineTaskService.CreateReconTask, invalid arguments: {JsonConvert.SerializeObject(new { StudyId = studyId, ScanId = scanId, ReconId = reconId })}");
            return Task.FromResult(new OfflineCommandResult { Status = CommandExecutionStatus.Failure }).Result;
        }

        _logger.LogDebug($"OfflineTaskService.CreateReconTask, arguments: {JsonConvert.SerializeObject(new { StudyId = studyId, ScanId = scanId, ReconId = reconId })}");

        var offlineRecons = _offlineProxyService.GetReconTasks();

        var examInfo = _studyService.Get(studyId);
        if (string.IsNullOrEmpty(examInfo.Study.Protocol))
        {
            _logger.LogError($"OfflineTaskService.CreateReconTask, invalid study: {studyId}");
            return Task.FromResult(new OfflineCommandResult { Status = CommandExecutionStatus.Failure }).Result;
        }

        if (offlineRecons is not null)
        {
            //存在，直接启动
            var offlineTask = offlineRecons.FirstOrDefault(r => r.ReconId == reconId && r.IsOfflineRecon == true);
            if (offlineTask is not null)
            {
                _offlineProxyService.DeleteTask(reconId);
            }
        }

        var instanceProtocol = ProtocolHelper.Deserialize(examInfo.Study.Protocol);
        var protocolEntities = ProtocolHelper.Expand(instanceProtocol);
        var currentEntities = protocolEntities.FirstOrDefault(p => p.Scan.Descriptor.Id == scanId);
        var currentRecon = currentEntities.Scan.Children.FirstOrDefault(p => p.Descriptor.Id == reconId);

        var offlineReconTask = CreateTask(GetPatient(examInfo.Patient, examInfo.Study), GetStudy(examInfo.Study), instanceProtocol, currentEntities, currentRecon).Result;
        if (offlineReconTask.Status == CommandExecutionStatus.Success)
        {
            InsertReconTask(examInfo, currentEntities, currentRecon);
            StartTask(currentRecon.Descriptor.Id);
        }
        return offlineReconTask;
    }

    public OfflineCommandResult StartTask(string reconId)
    {
        return _offlineProxyService.StartTask(reconId).Result;
    }

    public OfflineCommandResult StopTask(string reconId)
    {
        var offlineTask = _offlineProxyService.StopTask(reconId).Result;
        return offlineTask;
    }

    public OfflineCommandResult CreatePostProcessTask(string studyId, string scanId, string originalReconId, string originalSeriesId, string seriesDescription, string imagePath, List<PostProcessModel> postProcesses)
    {
        _logger.LogInformation($"OfflineTaskService.CreatePostProcessTask, argument: {JsonConvert.SerializeObject(new { StudyId = studyId, ScanId = scanId, ReconId = originalReconId, SeriesId = originalSeriesId, SeriesDescription = seriesDescription, ImagePath = imagePath, PostProcesses = postProcesses })}");

        if (string.IsNullOrEmpty(studyId) || /*string.IsNullOrEmpty(seriesId) || string.IsNullOrEmpty(seriesDescription) ||*/ string.IsNullOrEmpty(imagePath) || postProcesses is null || postProcesses.Count == 0)
        {
            _logger.LogError($"OfflineTaskService.CreatePostProcessTask, invalid argument: {JsonConvert.SerializeObject(new { StudyId = studyId, ReconId = originalReconId, SeriesId = originalSeriesId, ImagePath = imagePath, PostProcesses = postProcesses })}");
            return new OfflineCommandResult { Status = CommandExecutionStatus.Failure };
        }

        var examInfo = _studyService.Get(studyId);
        if (examInfo.Study is null || examInfo.Patient is null)
        {
            _logger.LogError($"OfflineTaskService.CreatePostProcessTask, invalid study or patient: {studyId}");
            return new OfflineCommandResult { Status = CommandExecutionStatus.Failure };
        }

        var latestSeriesNumber = GetLatestSeriesNumber(studyId, scanId, originalReconId, originalSeriesId);

        var info = new ScanReconParam {
            Patient = GetPatient(examInfo.Patient, examInfo.Study),
            Study = GetStudy(examInfo.Study)
        };
        if (!string.IsNullOrEmpty(scanId))
        {
            //todo:是否需要通过协议获取相关信息
            info.ScanParameter = new ScanParam {
                ScanUID = scanId
            };
        }

        var reconSeries = new ReconSeriesParam
        {
            ReconID = IdGenerator.Next(5),
            SeriesNumber = latestSeriesNumber,
            SeriesDescription = seriesDescription,
            SeriesInstanceUID = UIDHelper.CreateSeriesInstanceUID(),
            SOPClassUID = UIDHelper.CreateSOPClassUID(),
            SOPClassUIDHeader = IdGenerator.Next(7),
            PostProcessEnable = true,
            PostProcessInfos = GetPostProcesses(postProcesses),
        };

        info.ReconSeriesParams = new ReconSeriesParam[] { reconSeries };
        _logger.LogInformation($"OfflineTaskService.CreatePostProcessTask, argument: {JsonConvert.SerializeObject(info)}");
        var offlineProcessTask = _offlineProxyService.CreatePostProcessTask(imagePath, info).Result;
        if (offlineProcessTask.Status == CommandExecutionStatus.Success)
        {
            InsertReconTask(studyId, examInfo.Patient.Id, reconSeries.ReconID, originalReconId, seriesDescription, latestSeriesNumber, imagePath, postProcesses);
            StartTask(reconSeries.ReconID);
        }

        return offlineProcessTask;
    }

    public OfflineCommandResult CreateTasks(string studyId)
    {
        if (string.IsNullOrEmpty(studyId))
        {
            //TODO: 待处理（含日志）
            _logger.LogError($"OfflineTaskService.CreateAllTasks, invalid argument: {studyId}");
            return Task.FromResult(new OfflineCommandResult { Status = CommandExecutionStatus.Failure }).Result;
        }

        _logger.LogDebug($"OfflineTaskService.CreateAllTasks, argument: ({studyId})");

        var examInfo = _studyService.Get(studyId);
        if (string.IsNullOrEmpty(examInfo.Study.Protocol))
        {
            _logger.LogError($"OfflineTaskService.CreateAllTasks, invalid study ({studyId})");
            return Task.FromResult(new OfflineCommandResult { Status = CommandExecutionStatus.Failure }).Result;
        }
        var instanceProtocol = ProtocolHelper.Deserialize(examInfo.Study.Protocol);
        var protocolEntities = ProtocolHelper.Expand(instanceProtocol);
        var tempPatient = GetPatient(examInfo.Patient, examInfo.Study);
        var tempStudy = GetStudy(examInfo.Study);

        var offlineRecons = _offlineProxyService.GetReconTasks();

        var reconAll = new List<string>();

        foreach (var itemInfo in protocolEntities)
        {
            if (itemInfo.Scan.Status != PerformStatus.Performed || itemInfo.Scan.ScanImageType == ScanImageType.Topo) continue;

            var rtdRecon = itemInfo.Scan.Children.FirstOrDefault(recon => recon.IsRTD);

            if (rtdRecon is null || rtdRecon.Status != PerformStatus.Performed) continue;

            foreach (var reconInfo in itemInfo.Scan.Children)
            {
                if (reconInfo.IsRTD || reconInfo.Status != PerformStatus.Unperform) continue;

                if (offlineRecons is not null)
                {
                    //如果已经创建，则直接启动
                    var tempTask = offlineRecons.FirstOrDefault(r => r.ReconId == reconInfo.Descriptor.Id);
                    if (tempTask is not null)
                    {
                        _offlineProxyService.DeleteTask(reconInfo.Descriptor.Id);
                    }
                }

                var offlineTask = CreateTask(tempPatient, tempStudy, instanceProtocol, itemInfo, reconInfo).Result;
                reconAll.Add(reconInfo.Descriptor.Id);
                if (offlineTask.Status == CommandExecutionStatus.Success)
                {
                    InsertReconTask(examInfo, itemInfo, reconInfo);
                }
                Task.Delay(100).Wait();
            }
        }

        _logger.LogInformation($"OfflineTaskService.CreateAllTasks, start tasks: {studyId}, {string.Join(", ", reconAll)}");
        foreach (var reconItem in reconAll)
        {
            _offlineProxyService.StartTask(reconItem);
            Task.Delay(100).Wait();
        }

        return Task.FromResult(new OfflineCommandResult { Status = CommandExecutionStatus.Success }).Result;
    }

    public void PinTask(string reconId)
    {
        var offlineTask = _offlineProxyService.GetTask(reconId);
        if (offlineTask is not null)
        {
            _offlineProxyService.PinTask(reconId);
        }
    }

    public void DeleteTask(string reconId)
    {
        var offlineTask = _offlineProxyService.GetTask(reconId);

        if (offlineTask is not null)
        {
            _offlineProxyService.DeleteTask(reconId);
            TaskRemoved?.Invoke(this, reconId);
        }
    }

    public void StartAutoRecons(string studyId)
    {
        _logger.LogDebug($"OfflineTaskService.StartAutoRecons, argument: {studyId}");
        if (string.IsNullOrEmpty(studyId))
        {
            _logger.LogError($"OfflineTaskService.StartAutoRecons, invalid argument: {studyId}");
            return;
        }

        var examInfo = _studyService.Get(studyId);
        if (string.IsNullOrEmpty(examInfo.Study.Protocol))
        {
            _logger.LogError($"OfflineTaskService.StartAutoRecons, invalid study: {studyId}");
            return;
        }
        var instanceProtocol = ProtocolHelper.Deserialize(examInfo.Study.Protocol);
        var protocolEntities = ProtocolHelper.Expand(instanceProtocol);
        var tempPatient = GetPatient(examInfo.Patient, examInfo.Study);
        var tempStudy = GetStudy(examInfo.Study);

        var offlineRecons = _offlineProxyService.GetReconTasks();

        foreach (var itemInfo in protocolEntities)
        {
            if (itemInfo.Scan.Status != PerformStatus.Performed || itemInfo.Scan.ScanImageType == ScanImageType.Topo) continue;

            var rtdRecon = itemInfo.Scan.Children.FirstOrDefault(recon => recon.IsRTD);

            if (rtdRecon is null || rtdRecon.Status != PerformStatus.Performed) continue;

            foreach (var reconInfo in itemInfo.Scan.Children)
            {
                if (reconInfo.IsRTD || reconInfo.Status != PerformStatus.Unperform || !reconInfo.IsAutoRecon) continue;

                if (offlineRecons is not null)
                {
                    //如果已经创建，则直接启动
                    var tempTask = offlineRecons.FirstOrDefault(r => r.ReconId == reconInfo.Descriptor.Id);
                    if (tempTask is not null)
                    {
                        _offlineProxyService.DeleteTask(reconInfo.Descriptor.Id);
                    }
                }

                var offlineTask = CreateTask(tempPatient, tempStudy, instanceProtocol, itemInfo, reconInfo).Result;
                if (offlineTask.Status == CommandExecutionStatus.Success)
                {
                    InsertReconTask(examInfo, itemInfo, reconInfo);
                    StartTask(reconInfo.Descriptor.Id);
                }
                Task.Delay(100).Wait();
            }
        }
    }

    public List<OfflineTaskInfo> GetTasks()
    {
        var offlineRecons = _offlineProxyService.GetReconTasks();

        var offlineProcesses = _offlineProxyService.GetPostProcessTasks();

        _logger.LogDebug($"GetTasks, OfflineRecon: {offlineRecons?.Count}, OfflineProcesses : {offlineProcesses?.Count}");

        var offlineTasks = new List<OfflineTaskInfo>();

        if (offlineRecons is not null && offlineRecons.Count > 0)
        {
            foreach (var offlineRecon in offlineRecons)
            {
                try
                {
                    var (patient, study) = _studyService.GetWithUID(offlineRecon.StudyUID);
                    if (patient is null || study is null)
                    {
                        _logger.LogWarning($"GetTasks, study is not exists: {JsonConvert.SerializeObject(new { StudyUID = offlineRecon.StudyUID, ScanUID = offlineRecon.ScanId, ReconId = offlineRecon.ReconId, SeriesUID = offlineRecon.SeriesUID })}");
                        continue;
                    }
                    offlineRecon.PatientName = patient.PatientName;
                    offlineRecon.PatientId = patient.PatientId;
                    var reconTask = _reconTaskService.Get2(study.Id, offlineRecon.ScanId, offlineRecon.ReconId);
                    if (reconTask is not null)
                    {
                        offlineRecon.SeriesDescription = reconTask.SeriesDescription;
                        offlineRecon.Status = (OfflineTaskStatus)(reconTask.TaskStatus);
                        offlineRecon.ReconTaskDateTime = reconTask.ReconStartDate;
                    }
                    offlineTasks.Add(offlineRecon.Clone());
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetTasks, OfflineRecon exception: {JsonConvert.SerializeObject(new { StudyUID = offlineRecon.StudyUID, ScanUID = offlineRecon.ScanId, ReconId = offlineRecon.ReconId, SeriesUID = offlineRecon.SeriesUID })}, {ex.Message}");
                }
            }
        }

        if (offlineProcesses is not null && offlineProcesses.Count > 0)
        {
            foreach (var offlineProcess in offlineProcesses)
            {
                try
                {
                    var (patient, study) = _studyService.GetWithUID(offlineProcess.StudyUID);
                    if (patient is null || study is null)
                    {
                        _logger.LogWarning($"GetTasks, study is not exists: {JsonConvert.SerializeObject(new { StudyUID = offlineProcess.StudyUID, ScanUID = offlineProcess.ScanId, ReconId = offlineProcess.ReconId, SeriesUID = offlineProcess.SeriesUID })}");
                        continue;
                    }
                    offlineProcess.PatientName = patient.PatientName;
                    offlineProcess.PatientId = patient.PatientId;

                    var reconTask = _reconTaskService.Get2(study.Id, offlineProcess.ScanId, offlineProcess.ReconId);

                    offlineProcess.SeriesDescription = reconTask.SeriesDescription;
                    offlineProcess.Status = (OfflineTaskStatus)(reconTask.TaskStatus);
                    offlineProcess.ReconTaskDateTime = reconTask.ReconStartDate;
                    offlineTasks.Add(offlineProcess.Clone());
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetTasks, OfflinePostProcesses exception: {JsonConvert.SerializeObject(new { StudyUID = offlineProcess.StudyUID, ScanUID = offlineProcess.ScanId, ReconId = offlineProcess.ReconId, SeriesUID = offlineProcess.SeriesUID })}, {ex.Message}");
                }
            }
        }

        return offlineTasks;
    }

    public void StartTasks()
    {
        _offlineProxyService.StartReconTasks();
    }

    public void StopTasks()
    {
        _offlineProxyService.StopReconTasks();
    }

    public event EventHandler<EventArgs<List<string>>>? ErrorOccured;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? ImageProgressChanged;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? ProgressChanged;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskCreated;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskWaiting;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskStarted;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskCanceled;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskAborted;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskFinished;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskDone;

    public event EventHandler<string>? TaskRemoved;

    private Task<OfflineCommandResult> CreateTask(Patient patient, Study study, ProtocolModel protocol, (FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan) itemInfo, ReconModel reconInfo)
    {
        var parameters = new ScanReconParam
        {
            Patient = patient,
            Study = study,
            ScanParameter = GetScanParameter(protocol, itemInfo.Scan),
            ReconSeriesParams = GetReconSeriesParameters(protocol.Descriptor.Name, itemInfo.Frame, itemInfo.Scan, reconInfo)
        };

        _logger.LogInformation($"OfflineTaskService.CreateReconTask, arguments: {JsonConvert.SerializeObject(parameters)}");
        return _offlineProxyService.CreateReconTask(parameters);
    }

    private Patient GetPatient(PatientModel patientModel, StudyModel studyModel)
    {
        //todo:临时代码，显示版本
        var patient = new Patient
        {
            AdditionalHistory = string.Empty, // "Additional Patient History",
            ContrastAller = string.Empty,
            EthnicGroup = string.Empty,
            MedicalAlerts = string.Empty,
            OtherPatientIDs = string.Empty,
            PatientStatus = string.Empty,
            PregnancyStatus = PregnantStatus.Unknown
        };
        patient.ID = patientModel.PatientId;
        patient.Comment = studyModel.Comments;
        patient.Name = patientModel.PatientName;
        patient.Sex = (PatientSex)((int)patientModel.PatientSex);
        patient.Size = studyModel.PatientSize;
        patient.Weight = studyModel.PatientWeight;
        patient.Age = $"{studyModel.Age.ToString("D3")}{((AgeType)(studyModel.AgeType)).ToString().Substring(0, 1)}";
        patient.BirthDate = studyModel.Birthday;
        patient.BirthTime = studyModel.Birthday;
        return patient;
    }

    private Study GetStudy(StudyModel studyModel)
    {
        var study = new Study
        {
            NameOfPhysiciansReadingStudy = string.Empty,
            PhysiciansOfRecord = string.Empty,
            ReferringPhysiciansName = string.Empty,
            RequestedProcedureDescription = string.Empty,
            RequestedProcedureID = string.Empty,
            RequestedProcedurePriority = RequestedProcedurePriority.MEDIUM,
            RequestingPhysician = string.Empty,
            RequestingService = string.Empty,
            SpecialNeeds = string.Empty,
        };
        study.StudyID = studyModel.StudyId;
        study.StudyInstanceUID = studyModel.StudyInstanceUID;
        study.AccessionNumber = studyModel.AccessionNo;
        study.StudyDate = studyModel.StudyDate;
        study.StudyTime = studyModel.StudyTime;
        study.StudyDescription = studyModel.StudyDescription;

        var loginName = _authorizationService.GetCurrentUser()?.UserName;
        loginName = string.IsNullOrEmpty(loginName) ? string.Empty : loginName;

        study.OperatorsName = loginName;

        return study;
    }

    public ScanParam GetScanParameter(ProtocolModel protocol, ScanModel scanEntity)
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
        parameter.BodyCategory = _mapper.Map<FacadeProxy.Common.Enums.ScanEnums.BodyCategory>(protocol.BodySize);

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

        parameter.FunctionMode = scanEntity.FunctionMode;
        parameter.Focal = scanEntity.FocalType;

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

        parameter.mAs = scanEntity.mAs;
        parameter.CTDIvol = scanEntity.CTDIvol;

        return parameter;
    }

    private ReconSeriesParam[] GetReconSeriesParameters(string protocolName, FrameOfReferenceModel frameModel, ScanModel scanModel, ReconModel reconModel)
    {
        var reconInfo = new ReconSeriesParam();
        reconInfo.ReconID = reconModel.Descriptor.Id;
        reconInfo.ReconType = reconModel.ReconType;
        reconInfo.PreBinning = reconModel.PreBinning;

        //处理重建部位和扫描部位不一致
        if (reconModel.BodyPart.HasValue)
        {
            reconInfo.ReconBodyPart = _mapper.Map<FacadeProxy.Common.Enums.BodyPart>(reconModel.BodyPart.Value);
        }

        //todo:确认是否走上层处理
        if (reconModel.IVRTVCoef != 0)
        {
            reconInfo.IVRTVCoef = reconModel.IVRTVCoef;
        }
        else
        {
            //todo: 读取配置
            var coefficent = SystemConfig.GetTVCoefficientInfo(scanModel.BodyPart.ToString(), reconModel.WindowType.ToString());
            reconInfo.IVRTVCoef = coefficent.Factor;
        }

        reconInfo.AirCorrectionMode = reconModel.AirCorrectionMode;
        reconInfo.FilterType = reconModel.FilterType;

        reconInfo.BoneAritifactEnable = reconModel.BoneAritifactEnable;
        reconInfo.InterpType = reconModel.InterpType;
        reconInfo.MetalAritifactEnable = reconModel.MetalAritifactEnable;
        reconInfo.SliceThickness = reconModel.SliceThickness;

        reconInfo.WindmillArtifactReduceEnable = reconModel.WindmillArtifactEnable;
        reconInfo.ConeAngleArtifactReduceEnable = reconModel.ConeAngleArtifactEnable;

        reconInfo.PreDenoiseType = reconModel.PreDenoiseType;
        reconInfo.PreDenoiseCoef = reconModel.PreDenoiseCoef;
        reconInfo.PostDenoiseType = reconModel.PostDenoiseType;
        reconInfo.PostDenoiseCoef = reconModel.PostDenoiseCoef;

        reconInfo.WindowCenter = reconModel.WindowCenter;
        reconInfo.WindowWidth = reconModel.WindowWidth;

        reconInfo.CenterFirstX = reconModel.CenterFirstX;
        reconInfo.CenterFirstY = reconModel.CenterFirstY;
        reconInfo.CenterFirstZ = reconModel.CenterFirstZ;
        reconInfo.CenterLastX = reconModel.CenterLastX;
        reconInfo.CenterLastY = reconModel.CenterLastY;
        reconInfo.CenterLastZ = reconModel.CenterLastZ;

        reconInfo.ScatterAlgorithm = reconModel.ScatterAlgorithm;
        reconInfo.RingAritifactEnable = reconModel.RingAritifactEnable;
        reconInfo.RingCorrectCoef = reconModel.RingCorrectionCoef;
        reconInfo.SmoothZEnable = reconModel.SmoothZEnable;
        reconInfo.MinTablePosition = reconModel.MinTablePosition;
        reconInfo.MaxTablePosition = reconModel.MaxTablePosition;
        reconInfo.TwoPassEnable = reconModel.TwoPassEnable;

        reconInfo.IsTargetRecon = reconModel.IsTargetRecon;

        reconInfo.FoVDirectionHorX = reconModel.FOVDirectionHorizontalX;
        reconInfo.FoVDirectionHorY = reconModel.FOVDirectionHorizontalY;
        reconInfo.FoVDirectionHorZ = reconModel.FOVDirectionHorizontalZ;
        reconInfo.FoVDirectionVertX = reconModel.FOVDirectionVerticalX;
        reconInfo.FoVDirectionVertY = reconModel.FOVDirectionVerticalY;
        reconInfo.FoVDirectionVertZ = reconModel.FOVDirectionVerticalZ;
        reconInfo.FoVLengthHor = reconModel.FOVLengthHorizontal;
        reconInfo.FoVLengthVert = reconModel.FOVLengthVertical;
        reconInfo.ImageMatrixHor = reconModel.ImageMatrixHorizontal;
        reconInfo.ImageMatrixVert = reconModel.ImageMatrixVertical;
        reconInfo.ImageIncrement = reconModel.ImageIncrement;
        reconInfo.SeriesDescription = string.IsNullOrEmpty(reconModel.SeriesDescription.Trim()) ? reconModel.DefaultSeriesDescription.Trim() : reconModel.SeriesDescription.Trim();
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
        reconInfo.ProtocolName = protocolName;
        reconInfo.SeriesNumber = reconModel.SeriesNumber;
        reconInfo.IsHDRecon = reconModel.IsHDRecon;

        if (!reconModel.IsRTD)
        {
            var roiInput = GetTargetReconInput(reconModel);

            if (roiInput != null)
            {
                var roiOutput = TargetReconCalculator.Instance.GetTargetReconParams(roiInput);
                if (roiOutput != null)
                {
                    reconInfo.MinTablePosition = roiOutput.TablePositionMin;
                    reconInfo.MaxTablePosition = roiOutput.TablePositionMax;

                    if (roiOutput.IsTargetRecon)
                    {
                        reconInfo.IsTargetRecon = roiOutput.IsTargetRecon;
                        reconInfo.ROIFovCenterX = roiOutput.roiFovCenterX;
                        reconInfo.ROIFovCenterY = roiOutput.roiFovCenterY;
                    }
                }
            }
        }

        var postProcesses = GetPostProcesses(reconModel.PostProcesses);

        if (postProcesses is not null && postProcesses.Count > 0)
        {
            reconInfo.PostProcessEnable = true;
            reconInfo.PostProcessInfos = postProcesses;
        }

        return (new List<ReconSeriesParam> { reconInfo }).ToArray();
    }

    private List<BasePostProcessInfo> GetPostProcesses(List<PostProcessModel> postProcesses)
    {
        if (postProcesses == null || postProcesses.Count == 0) return default!;
        var processes = new List<BasePostProcessInfo>();
        foreach (var itemProcess in postProcesses)
        {
            //todo:待处理后处理参数
            switch (itemProcess.Type)
            {
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.Sharp:
                    var sharpProcess = new SharpPostProcessInfo();
                    sharpProcess.SharpLevel = GetPostProcessParameter<int>(itemProcess.Parameters, ProtocolParameterNames.POST_PROCESS_ARGUMENT_SHARP_LEVEL);
                    processes.Add(sharpProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.MotionArtifactReduce:
                    var motionProcess = new MotionArtifactReducePostProcessInfo();
                    motionProcess.MotionArtifactReduceLevel = GetPostProcessParameter<int>(itemProcess.Parameters, ProtocolParameterNames.POST_PROCESS_ARGUMENT_MOTION_ARTIFACT_REDUCE_LEVEL);
                    processes.Add(motionProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.PitchArtifactReduce:
                    var pitchProcess = new PitchArtifactReducePostProcessInfo();
                    pitchProcess.PitchArtifactReduceLevel = GetPostProcessParameter<int>(itemProcess.Parameters, ProtocolParameterNames.POST_PROCESS_ARGUMENT_PITCH_ARTIFACT_REDUCE_LEVEL);
                    processes.Add(pitchProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.Denoise:
                    var denoiseProcess = new DenoisePostProcessInfo();
                    denoiseProcess.DenoiseType = GetPostProcessParameter<PostDenoiseType>(itemProcess.Parameters, ProtocolParameterNames.POST_PROCESS_ARGUMENT_DENOISE_TYPE);
                    denoiseProcess.DenoiseLevel = GetPostProcessParameter<int>(itemProcess.Parameters, ProtocolParameterNames.POST_PROCESS_ARGUMENT_DENOISE_LEVEL);
                    processes.Add(denoiseProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.WindmillArtifactReduce:
                    var windmillProcess = new WindmillArtifactReducePostProcessInfo();
                    processes.Add(windmillProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.StreakArtifactReduce:
                    var streakProcess = new StreakArtifactReducePostProcessInfo();
                    processes.Add(streakProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.SparseArtifactReduce10:
                    var sparse10Process = new SparseArtifactReduce10PostProcessInfo();
                    processes.Add(sparse10Process);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.SparseArtifactReduce20:
                    var sparse20Process = new SparseArtifactReduce20PostProcessInfo();
                    sparse20Process.SparseArtifactReduce20Level = GetPostProcessParameter<int>(itemProcess.Parameters, ProtocolParameterNames.POST_PROCESS_ARGUMENT_SPARSE_ARTIFACT_REDUCE20_LEVEL);
                    processes.Add(sparse20Process);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.SkullArtifactReduce:
                    var skullProcess = new SkullArtifactReducePostProcessInfo();
                    processes.Add(skullProcess);
                    break;
                case FacadeProxy.Common.Enums.PostProcessEnums.PostProcessType.ConeAngleArtifactReduce:
                    var coneAngleProcess = new ConeAngleArtifactReducePostProcessInfo();
                    coneAngleProcess.ReconFilterType = GetPostProcessParameter<FilterType>(itemProcess.Parameters, ProtocolParameterNames.RECON_FILTER_TYPE);
                    processes.Add(coneAngleProcess);
                    break;
                default:
                    break;
            }
        }
        return processes;
    }

    private T GetPostProcessParameter<T>(List<ParameterModel> parameters, string parameterName)
    {
        if (parameters is null || parameters.Count == 0) return default!;
        if (string.IsNullOrEmpty(parameterName)) return default!;
        var info = parameters.FirstOrDefault(p => p.Name == parameterName);
        if (info is null) return default!;
        return ParameterConverter.Convert<T>(info.Value);
    }

    private TargetReconInput GetTargetReconInput(ReconModel reconModel)
    {
        //填充TargetRecon ROI相关的参数。
        var pp = reconModel.Parent.PatientPosition.HasValue ? reconModel.Parent.PatientPosition.Value : PatientPosition.HFS;
        var roiReconInput = new TargetReconInput(reconModel.Parent.ScanOption, pp,
            SystemConfig.DetectorConfig.Detector.Width.Value, (int)reconModel.Parent.CollimatorSliceWidth,
            reconModel.Parent.ReconVolumeStartPosition, reconModel.Parent.ReconVolumeEndPosition,
            reconModel.CenterFirstX, reconModel.CenterFirstY, reconModel.CenterFirstZ,
            reconModel.CenterLastX, reconModel.CenterLastY, reconModel.CenterLastZ,
            reconModel.FOVLengthHorizontal, reconModel.FOVLengthVertical,
            (int)reconModel.Parent.SmallAngleDeleteLength, (int)reconModel.Parent.LargeAngleDeleteLength);

        return roiReconInput;
    }

    private int GetLatestSeriesNumber(string studyId, string scanId, string originalReconId, string originalSeriesId)
    {
        if (!string.IsNullOrEmpty(originalReconId))
        {
            return GetLatestSeriesNumber(studyId, scanId, originalReconId);
        }
        else
        {
            return GetLatestSeriesNumber(studyId, originalSeriesId);
        }
    }

    private int GetLatestSeriesNumber(string studyId, string scanId, string originalReconId)
    {
        var originalReconInfo = _reconTaskService.Get2(studyId, scanId, originalReconId);

        if (originalReconInfo is null)
        {
            // 此处应该不存在
            return GlobalSeriesNumber.PostProcessIndex_Max * 1000 + 999;
        }

        var originalSeriesNumber = originalReconInfo.SeriesNumber % 1000;

        //获取基于同一个离线重建的的最后一个后处理
        var latestSeriesNumber = _reconTaskService.GetLatestSeriesNumber(studyId, originalSeriesNumber);

        var prefix = latestSeriesNumber / 1000;
        if (prefix == 0)
        {
            return (GlobalSeriesNumber.PostProcessIndex_Min + 1) * 1000 + originalSeriesNumber;
        }
        else
        {
            prefix++;
            if (prefix > GlobalSeriesNumber.PostProcessIndex_Max)
            {
                return (GlobalSeriesNumber.PostProcessIndex_Min + 1) * 1000 + originalSeriesNumber;
            }
            else
            {
                return prefix * 1000 + originalSeriesNumber;
            }
        }
    }

    private int GetLatestSeriesNumber(string studyId, string originalSeriesId)
    {
        var originalSeriesInfo = _seriesService.GetSeriesById(originalSeriesId);

        //不存在处理
        if (originalSeriesInfo is null || string.IsNullOrEmpty(originalSeriesInfo.SeriesNumber))
        {
            return GlobalSeriesNumber.PostProcessIndex_Max * 1000 + 999;
        }

        //补长度
        var tempSeriesNumber = $"000{originalSeriesInfo.SeriesNumber}";

        var suffix = tempSeriesNumber.Substring(tempSeriesNumber.Length - 3);

        //解析是否数字串
        int tempSuffix = 0;

        int.TryParse(suffix, out tempSuffix);

        //todo:待处理重复问题（需细化）

        return GlobalSeriesNumber.PostProcessIndex_Max * 1000 + tempSuffix;
    }

    private void InsertReconTask(string studyId, string patientId, string reconId,string oriReconId, string seriesDescription, int seriesNumber, string imagePath, List<PostProcessModel> postProcesses)
    {
        var parameters = new List<ParameterModel> {
            new ParameterModel { Name="ReferenceImagePath", Value=imagePath  }
        };

        var reconTask = new ReconTaskModel
        {
            Id = Guid.NewGuid().ToString(),
            FrameOfReferenceUid = string.Empty,
            ScanId = string.Empty,
            ReconId = reconId,
            InternalStudyId = studyId,
            InternalPatientId = patientId,
            IsRTD = false,
            IssuingParameters = JsonConvert.SerializeObject(new { Parameters = parameters, PostProcesses = postProcesses }),
            SeriesNumber = seriesNumber,
            SeriesDescription = seriesDescription,
            Description = string.Empty,
            WindowWidth = "0",
            WindowLevel = "0",
            TaskStatus = (int)OfflineTaskStatus.None,
            Creator = string.Empty,
            CreateTime = DateTime.Now
        };

        _reconTaskService.Insert(reconTask);
    }

    private void InsertReconTask((StudyModel Study, PatientModel Patient) examInfo, (FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan) protocolEntity, ReconModel reconEntity)
    {
        var reconTask = new ReconTaskModel
        {
            Id = Guid.NewGuid().ToString(),
            FrameOfReferenceUid = protocolEntity.Frame.Descriptor.Id,
            ScanId = protocolEntity.Scan.Descriptor.Id,
            ReconId = reconEntity.Descriptor.Id,
            InternalStudyId = examInfo.Study.Id,
            InternalPatientId = examInfo.Patient.Id,
            IsRTD = reconEntity.IsRTD,
            IssuingParameters = JsonConvert.SerializeObject(reconEntity.Parameters),
            SeriesNumber = reconEntity.SeriesNumber,
            SeriesDescription = string.IsNullOrEmpty(reconEntity.SeriesDescription) ? reconEntity.DefaultSeriesDescription : reconEntity.SeriesDescription,
            Description = reconEntity.Descriptor.Name,
            WindowWidth = reconEntity.WindowWidth[0].ToString(),
            WindowLevel = reconEntity.WindowCenter[0].ToString(),
            TaskStatus = (int)OfflineTaskStatus.None,
            Creator = string.Empty,
            CreateTime = DateTime.Now
        };
        _reconTaskService.Insert(reconTask);
    }

    private void OnProxyService_ErrorOccured(object? sender, CTS.EventArgs<List<string>> e)
    {
        _logger.LogInformation($"OfflineTaskService.ErrorOccured, event arguments: {JsonConvert.SerializeObject(new { CreateTime = DateTime.Now.ToFullString(), Thread = Thread.CurrentThread.ManagedThreadId, ErrorCodes = e.Data })}");
        ErrorOccured?.Invoke(this, new CTS.EventArgs<List<string>>(e.Data));
    }

    private void OnProxyService_TaskStatusChanged(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskService.TaskStatusChanged, event arguments: {JsonConvert.SerializeObject(e.Data)}");

        var offlineTaskInfo = ResetOfflineTaskInfo(e.Data);

        if (offlineTaskInfo is null)
        {
            _logger.LogWarning($"OfflineTaskService.TaskStatusChanged, recon of non user mode: {JsonConvert.SerializeObject(new { StudyId = e.Data.StudyUID, ScanId = e.Data.ScanId, ReconId = e.Data.ReconId, PatientId = e.Data.PatientId })}");
            return;
        }

        switch (e.Data.Status)
        {
            case OfflineTaskStatus.Created:
                TaskCreated?.Invoke(this, new CTS.EventArgs<OfflineTaskInfo>(offlineTaskInfo));
                break;
            case OfflineTaskStatus.Waiting:
                TaskWaiting?.Invoke(this, new CTS.EventArgs<OfflineTaskInfo>(offlineTaskInfo));
                break;
            case OfflineTaskStatus.Executing:
                TaskStarted?.Invoke(this, new CTS.EventArgs<OfflineTaskInfo>(offlineTaskInfo));
                break;
            case OfflineTaskStatus.Cancelled:
                TaskCanceled?.Invoke(this, new CTS.EventArgs<OfflineTaskInfo>(offlineTaskInfo));
                break;
            case OfflineTaskStatus.Finished:
                TaskFinished?.Invoke(this, new CTS.EventArgs<OfflineTaskInfo>(offlineTaskInfo));
                break;
            case OfflineTaskStatus.Error:
                TaskAborted?.Invoke(this, new CTS.EventArgs<OfflineTaskInfo>(offlineTaskInfo));
                break;
            default:
                break;
        }
    }

    private void OnProxyService_ImageProgressChanged(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
    {
        ImageProgressChanged?.Invoke(this, e);
    }

    private void OnProxyService_TaskDone(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineReconService.ReconDone, event arguments: {JsonConvert.SerializeObject(e.Data)}");
        var offlineTaskInfo = ResetOfflineTaskInfo(e.Data);
        if (offlineTaskInfo is null)
        {
            _logger.LogDebug($"OfflineReconService.ReconDone no data, parameters: {JsonConvert.SerializeObject(e.Data)}, converted parameter: {JsonConvert.SerializeObject(offlineTaskInfo)}");
            return;
        }

        if (offlineTaskInfo.IsOfflineRecon)
        {
            var filePath = Path.Combine(RuntimeConfig.Console.ImageData.Path, "..", offlineTaskInfo.StudyUID, $"{offlineTaskInfo.SeriesNumber}_{offlineTaskInfo.SeriesUID}");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            //图像文件拷贝（非开发环境，需要拷贝）
            var fileNames = Directory.GetFiles(offlineTaskInfo.ImagePath, "*.dcm").ToList();
            foreach (var fileName in fileNames)
            {
                var fileInfo = new FileInfo(fileName);
                File.Copy(fileName, Path.Combine(filePath, fileInfo.Name));
            }
            offlineTaskInfo.ImagePath = filePath;
        }
        else
        {
            var examInfo = _studyService.GetWithUID(offlineTaskInfo.StudyUID);
            var reconTask = _reconTaskService.Get2(examInfo.Study.Id, offlineTaskInfo.ScanId, offlineTaskInfo.ReconId);

            var newSeriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();
            offlineTaskInfo.SeriesUID = newSeriesInstanceUID;
            offlineTaskInfo.SeriesDescription = reconTask.SeriesDescription;

            var filePath = Path.Combine(RuntimeConfig.Console.ImageData.Path, "..", offlineTaskInfo.StudyUID, $"{offlineTaskInfo.SeriesNumber}_{offlineTaskInfo.SeriesUID}");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            //图像文件拷贝（非开发环境，需要拷贝）
            var fileNames = Directory.GetFiles(offlineTaskInfo.ImagePath, "*.dcm").ToList();

            foreach (var fileName in fileNames)
            {
                var fileInfo = new FileInfo(fileName);
                File.Copy(fileName, Path.Combine(filePath, fileInfo.Name));
            }
            offlineTaskInfo.ImagePath = filePath;
        }

        _logger.LogDebug($"OfflineReconService.ReconDone raised!");
        TaskDone?.Invoke(this, new EventArgs<OfflineTaskInfo>(offlineTaskInfo));
    }

    private void OnProxyService_ProgressChanged(object? sender, EventArgs<OfflineTaskInfo> e)
    {
        ProgressChanged?.Invoke(this, e);
    }

    private OfflineTaskInfo ResetOfflineTaskInfo(OfflineTaskInfo taskInfo)
    {
        var offlineTaskInfo = taskInfo.Clone();

        var (patient, study) = _studyService.GetWithUID(offlineTaskInfo.StudyUID);
        if (patient is null || study is null) return default!;

        taskInfo.PatientName = patient.PatientName;

        var reconTask = _reconTaskService.Get2(study.Id, offlineTaskInfo.ScanId, offlineTaskInfo.ReconId);

        if (reconTask is null) return default!;

        offlineTaskInfo.SeriesDescription = reconTask.SeriesDescription;

        return offlineTaskInfo;
    }
}
