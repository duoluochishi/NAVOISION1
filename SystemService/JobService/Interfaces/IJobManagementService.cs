using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;

namespace NV.CT.JobService.Interfaces;

public interface IJobManagementService
{
    public event EventHandler<JobTaskInfo>? NewJobEnqueued;

    public event EventHandler<JobTaskInfo>? CancelRunningJob;

    public bool EnqueueJob(BaseJobRequest jobRequest);

    public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType);

    public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType);

    public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType);

    public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus);

    public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest);

    public bool SetPrioirty(string jobId, JobTaskType jobType, PriorityType priorityType);

    public bool DeleteJob(string jobId, JobTaskType jobType);

    public bool CancelJob(string jobId, JobTaskType jobTaskType);

    public bool PauseJob(string jobId, JobTaskType jobType);

    public bool RunJob(string jobId, JobTaskType jobType);
}
