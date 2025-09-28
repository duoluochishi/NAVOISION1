using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;

namespace NV.CT.JobService.JobHandlers
{
    public class ExportJobHandler : IJobHandler
    {
        private readonly IJobQueueHandler _jobQueueHandler;

        public bool CanAccept(JobTaskInfo jobTaskInfo)
        {
            return jobTaskInfo.JobType is JobTaskType.ExportJob;
        }

        public ExportJobHandler(IJobQueueHandler jobQueueHandler)
        {
            _jobQueueHandler = jobQueueHandler;
        }

        public bool EnqueueJobRequest(JobTaskInfo jobRequest)
        {
            return _jobQueueHandler.EnqueueJobRequest(jobRequest);
        }
    }
}
