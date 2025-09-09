using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.EventArguments.OfflineMachine;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.OfflineMachine;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class OfflineTaskProxyService : IOfflineTaskProxyService
{
    private readonly IMapper _mapper;
    private readonly ILogger<OfflineTaskProxyService> _logger;
    private readonly IOfflineProxyService _offlineProxy;

    public event EventHandler<EventArgs<OfflineTaskInfo>> TaskStatusChanged;

    public event EventHandler<EventArgs<OfflineTaskInfo>> TaskDone;

    public event EventHandler<EventArgs<OfflineTaskInfo>> ImageProgressChanged;

    public event EventHandler<EventArgs<OfflineTaskInfo>> ProgressChanged;

    public OfflineTaskProxyService(IMapper mapper, ILogger<OfflineTaskProxyService> logger, IOfflineProxyService offlineProxy)
    {
        _mapper = mapper;
        _logger = logger;
        _offlineProxy = offlineProxy;

        _offlineProxy.TaskStatusChanged += OnOfflineProxy_TaskStatusChanged;
        _offlineProxy.ImageSaved += OnOfflineProxy_ImageSaved;
        _offlineProxy.ProgressChanged += OnOfflineProxy_ProgressChanged;
    }

    private void OnOfflineProxy_TaskStatusChanged(object sender, OfflineMachineTaskStatusChangedEventArgs offlineTaskStatusArgs)
    {
        if (offlineTaskStatusArgs.TaskType == FacadeProxy.Common.Enums.OfflineMachineEnums.OfflineMachineTaskType.Calibration) return;

        var taskParameters = JsonConvert.DeserializeObject<ScanReconParam>(offlineTaskStatusArgs.Parameters, PostProcessJsonSetting.Instance.Settings);
        var offlineTaskInfo = new OfflineTaskInfo {
            TaskId = offlineTaskStatusArgs.TaskID,
            IsOfflineRecon = offlineTaskStatusArgs.TaskType == FacadeProxy.Common.Enums.OfflineMachineEnums.OfflineMachineTaskType.OfflineRecon,
            Status = _mapper.Map<OfflineTaskStatus>(offlineTaskStatusArgs.TaskStatus),
            TaskStatus = offlineTaskStatusArgs.TaskStatus,
            PatientId = taskParameters.Patient.ID,
            StudyUID = taskParameters.Study.StudyInstanceUID,
            ScanId = taskParameters.ScanParameter.ScanUID,
            ReconId = taskParameters.ReconSeriesParams.FirstOrDefault().ReconID,
            SeriesUID = taskParameters.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID
        };

        if (taskParameters.ReconSeriesParams.FirstOrDefault().SeriesNumber.HasValue)
        {
            offlineTaskInfo.SeriesNumber = taskParameters.ReconSeriesParams.FirstOrDefault().SeriesNumber.Value;
        }
        else
        {
            //todo:应该不存在
            offlineTaskInfo.SeriesNumber = 99999;
        }

        if (!string.IsNullOrEmpty(offlineTaskStatusArgs.ErrorCode))
        {
            offlineTaskInfo.ErrorCodes = new List<string> { offlineTaskStatusArgs.ErrorCode };
        }
        TaskStatusChanged?.Invoke(this, new EventArgs<OfflineTaskInfo>(offlineTaskInfo));
    }

    private void OnOfflineProxy_ProgressChanged(object? sender, OfflineMachineTaskProgressChangedEventArgs e)
    {
        if (e.TaskType == FacadeProxy.Common.Enums.OfflineMachineEnums.OfflineMachineTaskType.Calibration) return;

        var taskParameters = JsonConvert.DeserializeObject<ScanReconParam>(e.Parameters, PostProcessJsonSetting.Instance.Settings);
        var offlineTaskInfo = new OfflineTaskInfo
        {
            TaskId = e.TaskID,
            IsOfflineRecon = e.TaskType == FacadeProxy.Common.Enums.OfflineMachineEnums.OfflineMachineTaskType.OfflineRecon,
            ProgressStep = e.ProgressStep,
            TotalStep = e.TotalStep,
            PatientId = taskParameters.Patient.ID,
            StudyUID = taskParameters.Study.StudyInstanceUID,
            ScanId = taskParameters.ScanParameter.ScanUID,
            ReconId = taskParameters.ReconSeriesParams.FirstOrDefault().ReconID,
            SeriesUID = taskParameters.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID
        };

        ProgressChanged?.Invoke(this, new EventArgs<OfflineTaskInfo>(offlineTaskInfo));
    }


    private void OnOfflineProxy_ImageSaved(object sender, DicomImageSavedInfoReceivedEventArgs imageSavedArgs)
    {
        if (imageSavedArgs.TaskType == FacadeProxy.Common.Enums.OfflineMachineEnums.OfflineMachineTaskType.Calibration) return;

        var taskParameters = JsonConvert.DeserializeObject<ScanReconParam>(imageSavedArgs.Parameters, PostProcessJsonSetting.Instance.Settings);
        var offlineTaskInfo = new OfflineTaskInfo
        {
            TaskId = imageSavedArgs.TaskID,
            IsOfflineRecon = imageSavedArgs.TaskType == FacadeProxy.Common.Enums.OfflineMachineEnums.OfflineMachineTaskType.OfflineRecon,
            IsOver = imageSavedArgs.IsFinished,
            Progress = imageSavedArgs.ActualSavedCount * 100.0f / imageSavedArgs.ExpectedTotalCount,
            LastImage = imageSavedArgs.DicomImagePath,
            ImagePath = imageSavedArgs.DicomImageDirectory,
            PatientId = taskParameters.Patient.ID,
            StudyUID = taskParameters.Study.StudyInstanceUID,
            ScanId = taskParameters.ScanParameter.ScanUID,
            ReconId = taskParameters.ReconSeriesParams.FirstOrDefault().ReconID,
            SeriesUID = taskParameters.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID
        };

        if (taskParameters.ReconSeriesParams.FirstOrDefault().SeriesNumber.HasValue)
        {
            offlineTaskInfo.SeriesNumber = taskParameters.ReconSeriesParams.FirstOrDefault().SeriesNumber.Value;
        }
        else
        {
            //todo:应该不存在
            offlineTaskInfo.SeriesNumber = 99999;
        }

        if (!imageSavedArgs.IsFinished)
        {
            ImageProgressChanged?.Invoke(this, new EventArgs<OfflineTaskInfo>(offlineTaskInfo));
        }
        else
        {
            offlineTaskInfo.Status = OfflineTaskStatus.Finished;
            TaskDone?.Invoke(this, new EventArgs<OfflineTaskInfo>(offlineTaskInfo));
        }
    }

    public Task<OfflineCommandResult> CreateReconTask(ScanReconParam parameters, TaskPriority priority = TaskPriority.Middle)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.CreateOfflineReconTask(parameters, priority);
                return new OfflineCommandResult
                {
                    TaskId = result.TaskID,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.CreateTask(ReconTask), argument: ({JsonConvert.SerializeObject(parameters)}).");
                return new OfflineCommandResult
                {
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> CreatePostProcessTask(string originalImagePath, ScanReconParam parameters, TaskPriority priority = TaskPriority.Middle)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.CreatePostProcessTask(originalImagePath, parameters, priority);
                return new OfflineCommandResult
                {
                    TaskId = result.TaskID,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.CreateTask(PostProcessTask), argument: {JsonConvert.SerializeObject(parameters)}).");
                return new OfflineCommandResult
                {
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> StartTask(string taskId)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.StartTask(taskId);
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.StartTask failed, argument: {taskId}.");
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> StopTask(string taskId)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.StopTask(taskId);
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.StopTask failed, argument: {taskId}.");
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> PinTask(string taskId)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.PinTask(taskId);
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.PinTask failed, argument: {taskId}.");
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> DeleteTask(string taskId)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.DeleteTask(taskId);
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.DeleteTask failed, argument: {taskId}.");
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.DeleteTask")) }
                };
            }
        });
    }

    public OfflineTaskInfo GetTask(string taskId)
    {
        try
        {
            var taskInfo = OfflineMachineTaskProxy.Instance.GetTask(taskId);
            if (taskInfo is null) return default!;

            if (taskInfo is OfflineReconTask)
            {
                var offlineRecon = taskInfo as OfflineReconTask;
                return new OfflineTaskInfo
                {
                    TaskId = taskInfo.TaskID,
                    Status = _mapper.Map<OfflineTaskStatus>(taskInfo.TaskStatus),
                    PatientId = offlineRecon.ScanReconParam.Patient.ID,
                    StudyUID = offlineRecon.ScanReconParam.Study.StudyInstanceUID,
                    ScanId = offlineRecon.ScanReconParam.ScanParameter.ScanUID,
                    ReconId = offlineRecon.ScanReconParam.ReconSeriesParams.FirstOrDefault().ReconID,
                    SeriesUID = offlineRecon.ScanReconParam.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID,
                    IsOfflineRecon = true
                };
            }
            else if(taskInfo is PostProcessTask)
            {
                var offlineProcess = taskInfo as PostProcessTask;
                return new OfflineTaskInfo
                {
                    TaskId = taskInfo.TaskID,
                    Status = _mapper.Map<OfflineTaskStatus>(taskInfo.TaskStatus),
                    PatientId = offlineProcess.ScanReconParam.Patient.ID,
                    StudyUID = offlineProcess.ScanReconParam.Study.StudyInstanceUID,
                    ScanId = offlineProcess.ScanReconParam.ScanParameter.ScanUID,
                    ReconId = offlineProcess.ScanReconParam.ReconSeriesParams.FirstOrDefault().ReconID,
                    SeriesUID = offlineProcess.ScanReconParam.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID,
                    IsOfflineRecon = false
                };
            }

            return default!;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"OfflineMachineTaskProxy.GetTask(Recon or PostProcess) failed, argument: {taskId}.");
            return default!;
        }
    }

    public Task<OfflineCommandResult> IncreaseTaskPriority(string taskId)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.IncreaseTaskPriority(taskId);
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.IncreaseTaskPriority failed, argument: {taskId}.");
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> DecreaseTaskPriority(string taskId)
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.DecreaseTaskPriority(taskId);
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.DecreaseTaskPriority failed, argument: {taskId}.");
                return new OfflineCommandResult
                {
                    TaskId = taskId,
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public List<OfflineTaskInfo> GetReconTasks()
    {
        try
        {
            var reconTasks = OfflineMachineTaskProxy.Instance.GetAllOfflineReconTasks();

            if (reconTasks is null || reconTasks.Count() == 0) return default!;

            return reconTasks.Select(t => new OfflineTaskInfo
            {
                TaskId = t.TaskID,
                Status = _mapper.Map<OfflineTaskStatus>(t.TaskStatus),
                PatientId = t.ScanReconParam.Patient.ID,
                StudyUID = t.ScanReconParam.Study.StudyInstanceUID,
                ScanId = t.ScanReconParam.ScanParameter.ScanUID,
                ReconId = t.ScanReconParam.ReconSeriesParams.FirstOrDefault().ReconID,
                SeriesUID = t.ScanReconParam.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID,
                IsOfflineRecon = true
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"OfflineMachineTaskProxy.GetAllOfflineReconTasks failed.");
            return default!;
        }
    }

    public List<OfflineTaskInfo> GetPostProcessTasks()
    {
        try
        {
            var processTasks = OfflineMachineTaskProxy.Instance.GetAllPostProcessTasks();

            if (processTasks is null || processTasks.Count() == 0) return default!;

            return processTasks.Select(t => new OfflineTaskInfo {
                TaskId = t.TaskID,
                Status = _mapper.Map<OfflineTaskStatus>(t.TaskStatus),
                PatientId = t.ScanReconParam.Patient.ID,
                StudyUID = t.ScanReconParam.Study.StudyInstanceUID,
                ScanId = t.ScanReconParam.ScanParameter.ScanUID,
                ReconId = t.ScanReconParam.ReconSeriesParams.FirstOrDefault().ReconID,
                SeriesUID = t.ScanReconParam.ReconSeriesParams.FirstOrDefault().SeriesInstanceUID,
                IsOfflineRecon = false
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"OfflineMachineTaskProxy.GetAllOfflineReconTasks failed.");
            return default!;
        }
    }

    public Task<OfflineCommandResult> StartReconTasks()
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.StartAllOfflineReconTasks();
                return new OfflineCommandResult
                {
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.StartAllOfflineReconTasks failed.");
                return new OfflineCommandResult
                {
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> StopReconTasks()
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.StopAllOfflineReconTasks();
                return new OfflineCommandResult
                {
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.StopAllOfflineReconTasks failed.");
                return new OfflineCommandResult
                {
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StopAllOfflineReconTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> StartPostProcessTasks()
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.StartAllPostProcessTasks();
                return new OfflineCommandResult
                {
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.StartAllPostProcessTasks failed.");
                return new OfflineCommandResult
                {
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StartAllPostProcessTasks")) }
                };
            }
        });
    }

    public Task<OfflineCommandResult> StopPostProcessTasks()
    {
        return Task.Run(() => {
            try
            {
                var result = OfflineMachineTaskProxy.Instance.StopAllPostProcessTasks();
                return new OfflineCommandResult {
                    Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                    Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"OfflineMachineTaskProxy.StopAllPostProcessTasks failed.");
                return new OfflineCommandResult
                {
                    Status = CommandExecutionStatus.Failure,
                    Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "OfflineMachineTaskProxy.StopAllPostProcessTasks")) }
                };
            }
        });
    }

    public (OfflineDiskInfo SystemDisk, OfflineDiskInfo AppDisk, OfflineDiskInfo DataDisk) GetDiskInfo()
    {
        var offlineDisks = OfflineMachineTaskProxy.Instance.GetOfflineMachineDiskInfo();
        var diskSystem = _mapper.Map<OfflineDiskInfo>(offlineDisks.SystemDisk);
        var diskApp = _mapper.Map<OfflineDiskInfo>(offlineDisks.AppDisk);
        var diskData = _mapper.Map<OfflineDiskInfo>(offlineDisks.DataDisk);
        return (diskSystem, diskApp, diskData);
    }
}
