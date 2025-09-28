using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.JobService.JobHandlers;
using System.Collections.Concurrent;

namespace NV.CT.JobService
{
    public class JobManagementService : IJobManagementService, IHostedService, IDisposable
    {
        private readonly ILogger<JobManagementService> _logger;
        private readonly IJobQueueHandler _jobQueueHandler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IJobHandler> _jobHandlers;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningJobs = new();
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public JobManagementService(
            ILogger<JobManagementService> logger,
            IJobQueueHandler jobQueueHandler,
            IServiceProvider serviceProvider,
            IEnumerable<IJobHandler> jobHandlers)
        {
            _logger = logger;
            _jobQueueHandler = jobQueueHandler;
            _serviceProvider = serviceProvider;
            _jobHandlers = jobHandlers;
        }

        public bool EnqueueJob(BaseJobRequest jobRequest)
        {
            if (jobRequest is null) return false;

            var jobTaskInfo = new JobTaskInfo
            {
                Id = jobRequest.Id,
                WorkflowId = jobRequest.WorkflowId,
                InternalPatientID = jobRequest.InternalPatientID,
                InternalStudyID = jobRequest.InternalStudyID,
                JobType = jobRequest.JobTaskType,
                JobStatus = JobTaskStatus.Queued,
                Creator = jobRequest.Creator,
                CreateTime = DateTime.Now,
                Parameter = jobRequest.Parameter,
            };

            if (jobTaskInfo.JobType != JobTaskType.WorklistJob)
            {
                _logger.LogTrace($"Enqueueing job {jobTaskInfo.Id} of type {jobTaskInfo.JobType}");
            }

            var handler = _jobHandlers.FirstOrDefault(h => h.CanAccept(jobTaskInfo));
            if (handler != null)
            {
                return handler.EnqueueJobRequest(jobTaskInfo);
            }

            _logger.LogWarning($"No handler found for job type {jobTaskInfo.JobType}");
            return false;
        }

        public bool CancelJob(string jobId, JobTaskType jobTaskType)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            _logger.LogInformation($"Attempting to cancel job {jobId}");
            if (_runningJobs.TryGetValue(jobId, out var cts))
            {
                cts.Cancel();
                _logger.LogInformation($"Cancellation requested for job {jobId}");
                return true;
            }

            _logger.LogWarning($"Could not cancel job {jobId}: Not found in running jobs.");
            return false;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Job Management Service is starting.");
            _executingTask = Task.Run(() => ExecuteAsync(_stoppingCts.Token), cancellationToken);
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = _jobQueueHandler.FetchNextAvailableJob(JobTaskType.All);
                    if (job != null)
                    {
                        var processor = GetProcessorForJob(job.JobType);
                        if (processor != null)
                        {
                            var cts = new CancellationTokenSource();
                            if (_runningJobs.TryAdd(job.Id, cts))
                            {
                                _logger.LogInformation($"Processing job {job.Id} of type {job.JobType}");
                                await processor.ProcessJobAsync(job, cts.Token);
                                _runningJobs.TryRemove(job.Id, out _);
                            }
                            else
                            {
                                _logger.LogWarning($"Job {job.Id} is already running.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"No processor found for job type {job.JobType}.");
                        }
                    }
                    else
                    {
                        await Task.Delay(1000, stoppingToken); // Wait before polling again
                    }
                }
                catch (OperationCanceledException)
                {
                    // This is expected on shutdown, no need to log an error.
                    _logger.LogInformation("Job management service is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the job management service execution loop.");
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Job Management Service is stopping.");
            if (_executingTask == null) return;

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private IJobProcessor? GetProcessorForJob(JobTaskType jobType)
        {
            return jobType switch
            {
                JobTaskType.ArchiveJob => _serviceProvider.GetRequiredService<ArchiveJobProcessor>(),
                JobTaskType.ExportJob => _serviceProvider.GetRequiredService<ExportJobProcessor>(),
                JobTaskType.ImportJob => _serviceProvider.GetRequiredService<ImportJobProcessor>(),
                JobTaskType.PrintJob => _serviceProvider.GetRequiredService<PrintJobProcessor>(),
                JobTaskType.WorklistJob => _serviceProvider.GetRequiredService<WorklistJobProcessor>(),
                _ => null
            };
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _stoppingCts.Dispose();
        }

        #region Unchanged Methods
        public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType) => _jobQueueHandler.FetchNextAvailableJob(jobType);
        public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType) => _jobQueueHandler.GetJobById(jobId, jobType);
        public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType) => _jobQueueHandler.FetchAvailableJobById(jobId, jobType);
        public bool DeleteJob(string jobId, JobTaskType jobType) => _jobQueueHandler.DeleteJob(jobId, jobType);
        public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest) => _jobQueueHandler.GetJobsByTypeAndStatus(queryJobRequest);
        public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus) => _jobQueueHandler.GetCountOfJobs(jobType, jobTaskStatus);
        public bool SetPrioirty(string jobId, JobTaskType jobType, PriorityType priorityType) => _jobQueueHandler.SetPrioirty(jobId, jobType, priorityType);
        public bool PauseJob(string jobId, JobTaskType jobType) => _jobQueueHandler.PauseJob(jobId, jobType);
        public bool RunJob(string jobId, JobTaskType jobType) => _jobQueueHandler.RunJob(jobId, jobType);
        #endregion
    }
}
