using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.JobService.Contract;
using NV.CT.Protocol.Models;
using NV.MPS.Communication;

namespace NV.CT.ClientProxy.JobService;

public class OfflineTaskService : IOfflineTaskService
{
    private readonly JobClientProxy _clientProxy;

    public OfflineTaskService(JobClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public OfflineTaskInfo GetTask(string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.GetTask),
            Data = reconId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineTaskInfo>();
            return res;
        }

        return default;
    }

    public OfflineCommandResult CreateTask(string studyId, string scanId, string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.CreateTask),
            Data = Tuple.Create(studyId, scanId, reconId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public OfflineCommandResult CreatePostProcessTask(string studyId, string scanId, string reconId, string seriesId, string seriesDescription, string imagePath, List<PostProcessModel> postProcesses)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.CreatePostProcessTask),
            Data = Tuple.Create(studyId, scanId, reconId, seriesId, seriesDescription, imagePath, postProcesses).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public OfflineCommandResult StartTask(string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.StartTask),
            Data = reconId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public void StartAutoRecons(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.StartAutoRecons),
            Data = studyId
        });
    }

    public OfflineCommandResult StopTask(string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.StopTask),
            Data = reconId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public void PinTask(string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.PinTask),
            Data = reconId
        });
    }

    public OfflineCommandResult IncreaseTaskPriority(string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.IncreaseTaskPriority),
            Data = reconId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public OfflineCommandResult DecreaseTaskPriority(string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.DecreaseTaskPriority),
            Data = reconId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public void DeleteTask(string reconId)
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.DeleteTask),
            Data = reconId
        });
    }

    public OfflineCommandResult CreateTasks(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.CreateTasks),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<OfflineCommandResult>();
            return res;
        }

        return default;
    }

    public List<OfflineTaskInfo> GetTasks()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.GetTasks),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<OfflineTaskInfo>>();
            return res;
        }

        return default;
    }

    public void StartTasks()
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.StartTasks),
            Data = string.Empty
        });
    }

    public void StopTasks()
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.StopTasks),
            Data = string.Empty
        });
    }

    public (OfflineDiskInfo SystemDisk, OfflineDiskInfo AppDisk, OfflineDiskInfo DataDisk) GetDiskInfo()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineTaskService).Namespace,
            SourceType = nameof(IOfflineTaskService),
            ActionName = nameof(IOfflineTaskService.GetDiskInfo),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<(OfflineDiskInfo SystemDisk, OfflineDiskInfo AppDisk, OfflineDiskInfo DataDisk)>();
            return res;
        }

        return default;
    }

#pragma warning disable 67
    public event EventHandler<EventArgs<List<string>>>? ErrorOccured;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? ImageSaved;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? ImageProgressChanged;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? ProgressChanged;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskCreated;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskWaiting;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskStarted;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskCanceled;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskAborted;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskFinished;
    public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskDone;
    public event EventHandler<string> TaskRemoved;
#pragma warning restore 67
}
