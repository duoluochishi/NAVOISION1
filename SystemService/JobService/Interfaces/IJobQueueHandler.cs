using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;

namespace NV.CT.JobService.Interfaces;

public interface IJobQueueHandler
{

    public bool EnqueueJobRequest(JobTaskInfo jobTask);

    public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType);

    public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType);

    public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType);

    public bool SetPrioirty(string jobId, JobTaskType jobType, PriorityType priorityType);

    public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus);

    public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest);

    public bool DeleteJob(string jobId, JobTaskType jobType);

    public bool PauseJob(string jobId, JobTaskType jobType);

    public bool RunJob(string jobId, JobTaskType jobType);

    void UpdateTaskStatusByJobId(string jobId, JobTaskStatus taskStatus);

    void SendJobTaskMessage(string jobId, MessageType messageType, MessageSource messageSource, JobTaskStatus jobTaskStatus, string messageContent, int processedCount, int totalCount);
}
