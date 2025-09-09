using NV.CT.Job.Contract.Model;

namespace NV.CT.JobService.Interfaces;

public interface IJobHandler
{
    bool CanAccept(JobTaskInfo jobTaskInfo);

    bool EnqueueJobRequest(JobTaskInfo jobTask);
}
