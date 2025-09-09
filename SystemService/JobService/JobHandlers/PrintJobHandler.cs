using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;

namespace NV.CT.JobService.JobHandlers
{
    public class PrintJobHandler : IJobHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public bool CanAccept(JobTaskInfo jobTaskInfo)
        {
            return jobTaskInfo.JobType is JobTaskType.PrintJob;
        }

        public PrintJobHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool EnqueueJobRequest(JobTaskInfo jobRequest)
        {
            var result = _serviceProvider.GetRequiredService<IJobQueueHandler>().EnqueueJobRequest(jobRequest);
            if (result)
            {
                Task.Run(() => { _serviceProvider.GetRequiredService<PrintJobProcessor>().EnqueueNewJob(jobRequest); });
            }

            return result;
        }
    }
}
