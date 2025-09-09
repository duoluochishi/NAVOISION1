using NV.CT.CTS.Models;
using NV.CT.Protocol.Models;

namespace NV.CT.JobService.Contract;

public interface IOfflineTaskService
{
    (OfflineDiskInfo SystemDisk, OfflineDiskInfo AppDisk, OfflineDiskInfo DataDisk) GetDiskInfo();

    OfflineTaskInfo GetTask(string reconId);

    OfflineCommandResult DecreaseTaskPriority(string reconId);

    OfflineCommandResult IncreaseTaskPriority(string reconId);

    void DeleteTask(string reconId);

    OfflineCommandResult CreateTask(string studyId, string scanId, string reconId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="studyId">必传</param>
    /// <param name="scanId">可空：string.Empty</param>
    /// <param name="reconId">必传</param>
    /// <param name="seriesId">可空：string.Empty</param>
    /// <param name="imagePath">必传</param>
    /// <param name="postProcesses">必传</param>
    /// <returns></returns>
    OfflineCommandResult CreatePostProcessTask(string studyId, string scanId, string originalReconId, string originalSeriesId, string seriesDescription, string imagePath, List<PostProcessModel> postProcesses);

    void StartAutoRecons(string studyId);

    OfflineCommandResult StartTask(string reconId);

    OfflineCommandResult StopTask(string reconId);

    void PinTask(string reconId);

    OfflineCommandResult CreateTasks(string studyId);

    List<OfflineTaskInfo> GetTasks();

    void StartTasks();

    void StopTasks();

    event EventHandler<CTS.EventArgs<List<string>>> ErrorOccured;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> ImageProgressChanged;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> ProgressChanged;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskCreated;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskWaiting;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskStarted;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskCanceled;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskAborted;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskFinished;
    event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskDone;

    event EventHandler<string> TaskRemoved;
}
