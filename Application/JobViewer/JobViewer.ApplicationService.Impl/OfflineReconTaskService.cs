using Grpc.Core;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.JobService.Contract;
using NV.CT.JobViewer.ApplicationService.Contract;

namespace NV.CT.JobViewer.ApplicationService.Impl
{
    public class OfflineReconTaskService : IOfflineReconTaskService
    {
        private readonly ILogger<OfflineReconTaskService>? _logger;
        private readonly IOfflineTaskService? _offlineService;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskCreated;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskWaiting;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskStarted;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskCanceled;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskAborted;
        public event EventHandler<string>? TaskRemoved;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskFinished;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? TaskDone;
        public event EventHandler<EventArgs<OfflineTaskInfo>>? ImageProgressChanged;

        public void SetReconStatus(int status, OfflineTaskInfo? reconTaskInfo)
        {
            //todo:是否需要实现待定，不是拋异常
            //throw new NotImplementedException();
        }

        public OfflineReconTaskService(ILogger<OfflineReconTaskService> logger, IOfflineTaskService offlineService)
        {
            _logger = logger;
            _offlineService = offlineService;

            _offlineService.ErrorOccured += OfflineService_ErrorOccured;
            _offlineService.TaskCreated += OfflineService_TaskCreated;
            _offlineService.TaskWaiting += OfflineService_TaskWaiting;
            _offlineService.TaskStarted += OfflineService_TaskStarted;
            _offlineService.TaskCanceled += OfflineService_TaskCanceled;
            _offlineService.TaskFinished += OfflineService_TaskFinished;
            _offlineService.TaskAborted += OfflineService_TaskAborted;
            _offlineService.TaskRemoved += OfflineService_TaskRemoved;
            _offlineService.ImageProgressChanged += OfflineService_ImageProgressChanged;
            _offlineService.TaskDone += OfflineService_TaskDone;
        }

        private void OfflineService_TaskCreated(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskCreated?.Invoke(this, e);
        }

        private void OfflineService_TaskWaiting(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskWaiting?.Invoke(this, e);
        }

        private void OfflineService_TaskStarted(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskStarted?.Invoke(this, e);
        }

        private void OfflineService_TaskCanceled(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskCanceled?.Invoke(this, e);
        }

        private void OfflineService_TaskFinished(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskFinished?.Invoke(this, e);
        }

        private void OfflineService_TaskAborted(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskAborted?.Invoke(this, e);
        }

        private void OfflineService_TaskRemoved(object? sender, string e)
        {
            TaskRemoved?.Invoke(this, e);
        }

        private void OfflineService_ImageProgressChanged(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            ImageProgressChanged?.Invoke(this, e);
        }

        private void OfflineService_TaskDone(object? sender, EventArgs<OfflineTaskInfo> e)
        {
            TaskDone?.Invoke(this, e);
        }

        private void OfflineService_ErrorOccured(object? sender, EventArgs<List<string>> e)
        {
            _logger.LogError($"error {string.Join(",", e.Data)}");
        }

        public List<OfflineTaskInfo> GetReconAllTasks()
        {
            try
            {
                var response = _offlineService.GetTasks();
                return response;
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"GetReconTaskStatus rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetReconTaskStatus exception: {ex.Message}");
            }
            return default(List<OfflineTaskInfo>);
        }

        public OfflineTaskInfo GetReconTaskStatus(string studyId, string scanId, string reconId)
        {
            try
            {
                var response = _offlineService.GetTask(reconId);
                return response;
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"GetReconTaskStatus rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetReconTaskStatus exception: {ex.Message}");
            }
            return default!;
        }

        public OfflineCommandResult StartReconTask(string? studyId, string? scanId, string? reconId)
        {
            try
            {
                return _offlineService.StartTask(reconId);
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"StartReconTask rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"StartReconTask rpc exception: {ex.Message}");
            }
            return default;
        }

        /// <summary>
        /// 取消重建任务
        /// </summary>
        /// <param name="studyId"></param>
        /// <param name="scanId"></param>
        /// <param name="reconId"></param>
        /// <returns></returns>
        public OfflineCommandResult CloseReconTask(string? studyId, string? scanId, string? reconId)
        {
            try
            {
                return _offlineService.StopTask(reconId);
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"CloseReconTask rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CloseReconTask exception: {ex.Message}");
            }
            return default;
        }

        /// <summary>
        /// 删除离线重建
        /// </summary>
        /// <param name="studyId"></param>
        /// <param name="scanId"></param>
        /// <param name="reconId"></param>
        /// <returns></returns>
        public OfflineCommandResult DestroyReconTask(string? studyId, string? scanId, string? reconId)
        {
            //todo:此方法已经移除，不再支持
            return default;
        }

        public OfflineCommandResult SetReconTaskIndex(string? studyId, string? scanId, string? reconId, int taskIndex)
        {
            //todo:此方法已经移除，不再支持
            return default;
        }

        public OfflineCommandResult SetReconTaskPriority(string? studyId, string? scanId, string? reconId, TaskPriority taskPriority)
        {
            //todo:此方法已经移除，不再支持
            return default;
        }

        public OfflineCommandResult IncreaseTaskPriority(string reconId)
        {
            try
            {
                _offlineService.IncreaseTaskPriority(reconId);
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"IncreaseTaskPriority rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"IncreaseTaskPriority exception: {ex.Message}");
            }
            return default;
        }

        public OfflineCommandResult DecreaseTaskPriority(string reconId)
        {
            try
            {
               _offlineService.DecreaseTaskPriority(reconId);
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"DecreaseTaskPriority rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DecreaseTaskPriority exception: {ex.Message}");
            }
            return default;
        }

        public void ToppingTaskPriority(string reconId)
        {
            try
            {
                _offlineService.PinTask(reconId);
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"PinTask rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PinTask exception: {ex.Message}");
            }
        }


        public OfflineCommandResult StartAllReconTasks(string studyId)
        {
            try
            {
                _offlineService.StartTasks();
            }
            catch (RpcException exRPC)
            {
                _logger.LogError(exRPC, $"StartAllReconTasks rpc exception: {exRPC.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"StartAllReconTasks exception: {ex.Message}");
            }
            return default;
        }


    }
}
