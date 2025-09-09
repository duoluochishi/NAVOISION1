using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.JobViewer.ApplicationService.Contract
{
    public interface IOfflineReconTaskService
    {
        List<OfflineTaskInfo> GetReconAllTasks();

        OfflineTaskInfo GetReconTaskStatus(string studyId, string scanId, string reconId);

        OfflineCommandResult StartReconTask(string? studyId, string? scanId, string? reconId);

        OfflineCommandResult CloseReconTask(string? studyId, string? scanId, string? reconId);

        OfflineCommandResult DestroyReconTask(string? studyId, string? scanId, string? reconId);

        /// <summary>
        /// todo:待移除
        /// </summary>
        /// <param name="studyId"></param>
        /// <param name="scanId"></param>
        /// <param name="reconId"></param>
        /// <param name="taskIndex"></param>
        /// <returns></returns>
        OfflineCommandResult SetReconTaskIndex(string? studyId, string? scanId, string? reconId, int taskIndex);

        /// <summary>
        /// todo:待移除
        /// </summary>
        /// <param name="studyId"></param>
        /// <param name="scanId"></param>
        /// <param name="reconId"></param>
        /// <param name="taskPriority"></param>
        /// <returns></returns>
        OfflineCommandResult SetReconTaskPriority(string? studyId, string? scanId, string? reconId, TaskPriority taskPriority);

        OfflineCommandResult IncreaseTaskPriority(string reconId);

        OfflineCommandResult DecreaseTaskPriority(string reconId);

        OfflineCommandResult StartAllReconTasks(string studyId);

        void ToppingTaskPriority(string reconId);

        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskCreated;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskWaiting;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskStarted;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskCanceled;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskAborted;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskFinished;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> TaskDone;
        event EventHandler<string> TaskRemoved;
        event EventHandler<CTS.EventArgs<OfflineTaskInfo>> ImageProgressChanged;
    }
}
