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
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.JobService.Interfaces;

namespace NV.CT.JobService
{
    public class JobRequestService : IJobRequestService
    {
        private readonly ILogger<JobRequestService> _logger;
        private readonly IJobManagementService _jobManagementService;

        public JobRequestService(ILogger<JobRequestService> logger,
                                 IJobManagementService jobManagementService)
        {
            _logger = logger;
            _jobManagementService = jobManagementService;
        }

        public JobTaskCommandResult EnqueueJobRequest(BaseJobRequest jobRequest)
        {
            var result = this._jobManagementService.EnqueueJob(jobRequest);
            return new JobTaskCommandResult { Status = result ? CommandExecutionStatus.Success : CommandExecutionStatus.Failure };
        }

        public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType)
        {
            return _jobManagementService.GetJobById(jobId, jobType);
        }

        public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType)
        {
            return _jobManagementService.FetchNextAvailableJob(jobType);
        }

        public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType)
        {
            return _jobManagementService.FetchAvailableJobById(jobId, jobType);
        }

        public bool DeleteJob(string jobId, JobTaskType jobType)
        {
            return _jobManagementService.DeleteJob(jobId, jobType);
        }

        public bool CancelJob(string jobId, JobTaskType jobTaskType)
        {
            return _jobManagementService.CancelJob(jobId, jobTaskType);
        }

        public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest)
        {
            return _jobManagementService.GetJobsByTypeAndStatus(queryJobRequest);
        }

        public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus)
        {
            return _jobManagementService.GetCountOfJobs(jobType, jobTaskStatus);
        }

        public bool MoveToTopPriority(string jobId, JobTaskType jobType)
        {
            return _jobManagementService.SetPrioirty(jobId, jobType, PriorityType.Highest);
        }

        public bool PauseJob(string jobId, JobTaskType jobType)
        {
            return _jobManagementService.PauseJob(jobId, jobType);
        }

        public bool RunJob(string jobId, JobTaskType jobType)
        {
            return _jobManagementService.RunJob(jobId, jobType);
        }

    }
}
