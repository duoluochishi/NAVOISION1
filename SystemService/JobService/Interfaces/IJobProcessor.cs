using NV.CT.Job.Contract.Model;

namespace NV.CT.JobService.Interfaces;

public interface IJobProcessor
{
    public Task ProcessJobAsync(JobTaskInfo job, CancellationToken cancellationToken);
}
