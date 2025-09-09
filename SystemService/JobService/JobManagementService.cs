//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.JobService.JobHandlers;

namespace NV.CT.JobService
{
    public class JobManagementService : IJobManagementService
    {
        private readonly ILogger<JobManagementService> _logger;
        private readonly IJobQueueHandler _jobQueueHandler;
        private readonly IServiceProvider _serviceProvider;

        private readonly List<IJobHandler> _jobHandlers;

        public event EventHandler<JobTaskInfo>? NewJobEnqueued;

        public event EventHandler<JobTaskInfo>? CancelRunningJob;

        public JobManagementService(ILogger<JobManagementService> logger,
                                    IJobQueueHandler jobQueueHandler,
                                    IServiceProvider serviceProvider)
        {
            _logger = logger;
            _jobQueueHandler = jobQueueHandler;
            _jobHandlers = new List<IJobHandler>();
            _serviceProvider = serviceProvider;

            this.Init();
        }

        private void Init()
        {
            _jobHandlers.Add(_serviceProvider.GetRequiredService<ArchiveJobHandler>());
            _jobHandlers.Add(_serviceProvider.GetRequiredService<ExportJobHandler>());
            _jobHandlers.Add(_serviceProvider.GetRequiredService<ImportJobHandler>());
            _jobHandlers.Add(_serviceProvider.GetRequiredService<PrintJobHandler>());
            _jobHandlers.Add(_serviceProvider.GetRequiredService<WorkListJobHandler>());
        }

        public bool EnqueueJob(BaseJobRequest jobRequest)
        {
            if (jobRequest is null)
            {
                return false;
            }

            // TODO 后续补充幂等性验证逻辑：判断是否是同1个请求，如果是则不予处理。
            var jobTaskInfo = new JobTaskInfo()
            {
                Id = jobRequest.Id,
                WorkflowId = jobRequest.WorkflowId,
                InternalPatientID = jobRequest.InternalPatientID,
                InternalStudyID = jobRequest.InternalStudyID,
                JobType = jobRequest.JobTaskType,
                JobStatus = JobTaskStatus.Queued,
                Creator = jobRequest.Creator,
                CreateTime = DateTime.Now,
                Parameter = jobRequest.Parameter,
            };

            //对于频次太高的任务类型不予打印跟踪日志，比如WorkListJob
            if (jobTaskInfo.JobType != JobTaskType.WorklistJob)
            {
                this._logger.LogTrace($"JobManagementService EnqueueJob for jobId:{jobTaskInfo.Id} with jobType:{jobTaskInfo.JobType.ToString()}");
            }           

            foreach (var handler in _jobHandlers)
            {
                if (handler.CanAccept(jobTaskInfo))
                {
                    return handler.EnqueueJobRequest(jobTaskInfo);
                }            
            }

            return false;
        }

        public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType)
        {
           return _jobQueueHandler.FetchNextAvailableJob(jobType);
        }

        public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType)
        {
            return _jobQueueHandler.GetJobById(jobId, jobType);
        }

        public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType)
        {
            return _jobQueueHandler.FetchAvailableJobById(jobId, jobType);
        }

        public bool DeleteJob(string jobId, JobTaskType jobType)
        {
           return _jobQueueHandler.DeleteJob(jobId, jobType);
        }

        public bool CancelJob(string jobId, JobTaskType jobTaskType)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return false;
            }

            var jobTaskInfo = new JobTaskInfo()
            {
                Id = jobId,
                WorkflowId = jobId,
                JobType = jobTaskType,
            };

            this._logger.LogTrace($"JobManagementService CancelJob for jobId:{jobId}");
            Task.Run(() => { this.CancelRunningJob?.Invoke(this, jobTaskInfo); }); 
            
            return true;
        }

        public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest)
        {
            return _jobQueueHandler.GetJobsByTypeAndStatus(queryJobRequest);
        }

        public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus)
        {
            return _jobQueueHandler.GetCountOfJobs(jobType, jobTaskStatus);
        }

        public bool SetPrioirty(string jobId, JobTaskType jobType, PriorityType priorityType)
        {
            return _jobQueueHandler.SetPrioirty(jobId, jobType, priorityType);
        }


        public bool PauseJob(string jobId, JobTaskType jobType)
        {
            return _jobQueueHandler.PauseJob(jobId, jobType);
        }

        public bool RunJob(string jobId, JobTaskType jobType)
        {
            return _jobQueueHandler.RunJob(jobId, jobType);
        }

    }
}
