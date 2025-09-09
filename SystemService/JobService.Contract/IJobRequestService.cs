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
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.Job.Contract.Model;

namespace NV.CT.JobService.Contract
{
    public interface IJobRequestService
    {
        public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType);

        public JobTaskCommandResult EnqueueJobRequest(BaseJobRequest jobRequest);

        public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType);

        public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType);

        public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest);

        public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus);

        public bool MoveToTopPriority(string jobId, JobTaskType jobType);

        public bool DeleteJob(string jobId, JobTaskType jobType);

        public bool CancelJob(string jobId, JobTaskType jobType);

        public bool PauseJob(string jobId, JobTaskType jobType);

        public bool RunJob(string jobId, JobTaskType jobType);
    }
}
