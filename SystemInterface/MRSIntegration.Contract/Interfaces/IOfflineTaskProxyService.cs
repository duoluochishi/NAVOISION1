using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IOfflineTaskProxyService
{
    event EventHandler<EventArgs<OfflineTaskInfo>> TaskStatusChanged;

    event EventHandler<EventArgs<OfflineTaskInfo>> TaskDone;

    event EventHandler<EventArgs<OfflineTaskInfo>> ImageProgressChanged;

    event EventHandler<EventArgs<OfflineTaskInfo>> ProgressChanged;

    Task<OfflineCommandResult> CreateReconTask(ScanReconParam parameters, TaskPriority priority = TaskPriority.Middle);

    Task<OfflineCommandResult> CreatePostProcessTask(string originalImagePath, ScanReconParam parameters, TaskPriority priority = TaskPriority.Middle);

    Task<OfflineCommandResult> StartTask(string taskId);

    Task<OfflineCommandResult> StopTask(string taskId);

    Task<OfflineCommandResult> PinTask(string taskId);

    Task<OfflineCommandResult> DeleteTask(string taskId);

    OfflineTaskInfo GetTask(string taskId);

    Task<OfflineCommandResult> IncreaseTaskPriority(string taskId);

    Task<OfflineCommandResult> DecreaseTaskPriority(string taskId);

    List<OfflineTaskInfo> GetReconTasks();

    Task<OfflineCommandResult> StartReconTasks();

    Task<OfflineCommandResult> StopReconTasks();

    List<OfflineTaskInfo> GetPostProcessTasks();

    Task<OfflineCommandResult> StartPostProcessTasks();

    Task<OfflineCommandResult> StopPostProcessTasks();

    (OfflineDiskInfo SystemDisk, OfflineDiskInfo AppDisk, OfflineDiskInfo DataDisk) GetDiskInfo();
}
