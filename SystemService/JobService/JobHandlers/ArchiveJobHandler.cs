using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;

namespace NV.CT.JobService.JobHandlers
{
    public class ArchiveJobHandler : IJobHandler
    {
        private readonly IJobQueueHandler _jobQueueHandler;

        public bool CanAccept(JobTaskInfo jobTaskInfo)
        {
            return jobTaskInfo.JobType is JobTaskType.ArchiveJob;
        }

        public ArchiveJobHandler(IJobQueueHandler jobQueueHandler)
        {
            _jobQueueHandler = jobQueueHandler;
        }

        public bool EnqueueJobRequest(JobTaskInfo jobRequest)
        {
            //Save job request to DB
            return _jobQueueHandler.EnqueueJobRequest(jobRequest);
        }
    }
}
