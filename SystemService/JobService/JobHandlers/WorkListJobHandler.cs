using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;

namespace NV.CT.JobService.JobHandlers
{
    public class WorkListJobHandler : IJobHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public bool CanAccept(JobTaskInfo jobTaskInfo)
        {
            return jobTaskInfo.JobType is JobTaskType.WorklistJob;
        }

        public WorkListJobHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool EnqueueJobRequest(JobTaskInfo jobRequest)
        {
            Task.Run(() => { _serviceProvider.GetRequiredService<WorklistJobProcessor>().EnqueueNewJob(jobRequest); });           
            return true;
        }
    }
}
