using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.Transfer;
using NV.CT.DicomUtility.Transfer.CEchoSCU;
using NV.CT.DicomUtility.Transfer.PrintSCU;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.MessageService.Contract;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace NV.CT.JobService;

public class PrintJobProcessor : IJobProcessor
{
    private readonly ILogger<PrintJobProcessor> _logger;
    private readonly IMessageService _messageService;
    private readonly IStudyService _studyService;
    private readonly ISeriesService _seriesService;
    private readonly IJobQueueHandler _jobQueueHandler;
    private readonly IEchoVerificationHandler _echoVerificationHandler;
    private readonly MessageType _currentMessageType = MessageType.PrintJobResponse;
    private const string MESSAGE_CANCELLED = "Cancelled";

    public PrintJobProcessor(
        ILogger<PrintJobProcessor> logger,
        IMessageService messageService,
        IStudyService studyService,
        ISeriesService seriesService,
        IJobQueueHandler jobQueueHandler)
    {
        _logger = logger;
        _messageService = messageService;
        _studyService = studyService;
        _seriesService = seriesService;
        _jobQueueHandler = jobQueueHandler;
        _echoVerificationHandler = new EchoVerificationHandler();
    }

    public async Task ProcessJobAsync(JobTaskInfo job, CancellationToken cancellationToken)
    {
        var jobParameter = JsonConvert.DeserializeObject<PrintJobRequest>(job.Parameter);
        if (jobParameter == null)
        {
            _logger.LogWarning($"Could not deserialize job parameter for job {job.Id}");
            return;
        }

        _logger.LogTrace($"PrintJobProcessor starting for job {job.Id}");

        var (isSuccess, errorMessage) = _echoVerificationHandler.VerifyEcho(jobParameter.Host, jobParameter.Port, jobParameter.CallingAE, jobParameter.CalledAE);
        if (!isSuccess)
        {
            _logger.LogWarning($"Print C-Echo failed for job {job.Id}: {errorMessage}");
            UpdatePrintStatus(job.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Failed);
            _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.PrintJob, JobTaskStatus.Failed, errorMessage, 0, jobParameter.ImagePathList.Count);
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var printSCUExecutor = new PrintSCUExecutor(_logger, cts);
        var processedStatusList = new ConcurrentBag<JobTaskStatus>();

        printSCUExecutor.ExecuteStatusInfoChanged += (sender, e) =>
        {
            if (e.Status == ExecuteStatus.Succeeded)
            {
                processedStatusList.Add(JobTaskStatus.Completed);
            }
        };

        try
        {
            await ProcessImages(printSCUExecutor, jobParameter, cts.Token, processedStatusList);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"Print job {job.Id} was canceled.");
            UpdatePrintStatus(job.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Cancelled);
            _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.PrintJob, JobTaskStatus.Cancelled, MESSAGE_CANCELLED, 0, jobParameter.ImagePathList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during print job {job.Id}.");
            UpdatePrintStatus(job.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Failed);
            _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.PrintJob, JobTaskStatus.Failed, ex.Message, 0, jobParameter.ImagePathList.Count);
        }
        finally
        {
            printSCUExecutor.ExecuteStatusInfoChanged -= (sender, e) =>
            {
                if (e.Status == ExecuteStatus.Succeeded)
                {
                    processedStatusList.Add(JobTaskStatus.Completed);
                }
            };
        }
    }

    private async Task ProcessImages(PrintSCUExecutor printSCUExecutor, PrintJobRequest jobParameter, CancellationToken cancellationToken, ConcurrentBag<JobTaskStatus> processedStatusList)
    {
        UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Processing);
        _jobQueueHandler.SendJobTaskMessage(jobParameter.Id, _currentMessageType, MessageSource.PrintJob, JobTaskStatus.Processing, string.Empty, 0, jobParameter.ImagePathList.Count);

        var dicomNode = new DicomNode(jobParameter.Host, jobParameter.Port, jobParameter.CallingAE, jobParameter.CalledAE);
        var printJob = new PrintJob(jobParameter.Id, jobParameter.IsColor, jobParameter.NumberOfCopies);
        int processedCount = 0;

        foreach (string imagePath in jobParameter.ImagePathList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            printJob.FastAddImage(new Bitmap(imagePath), jobParameter.Orientation, jobParameter.PageSize);
            await printSCUExecutor.PrintAsync(dicomNode, printJob);

            processedCount++;
            _jobQueueHandler.SendJobTaskMessage(jobParameter.Id, _currentMessageType, MessageSource.PrintJob, JobTaskStatus.Processing, string.Empty, processedCount, jobParameter.ImagePathList.Count);
        }

        var finalStatus = processedStatusList.Any(s => s == JobTaskStatus.Completed) ? JobTaskStatus.Completed : JobTaskStatus.Failed;
        UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, finalStatus);
        _jobQueueHandler.SendJobTaskMessage(jobParameter.Id, _currentMessageType, MessageSource.PrintJob, finalStatus, string.Empty, processedStatusList.Count, jobParameter.ImagePathList.Count);

        _logger.LogTrace($"Print job {jobParameter.Id} finished with status {finalStatus}.");
    }

    private void UpdatePrintStatus(string jobId, string studyId, string seriesId, JobTaskStatus jobTaskStatus)
    {
        _jobQueueHandler.UpdateTaskStatusByJobId(jobId, jobTaskStatus);

        var seriesList = _seriesService.GetSeriesByStudyId(studyId);
        if (seriesList == null || !seriesList.Any()) return;

        var currentSeries = seriesList.FirstOrDefault(s => s.Id == seriesId);
        if (currentSeries != null)
        {
            currentSeries.PrintStatus = (int)jobTaskStatus;
            _seriesService.UpdatePrintStatus(new[] { currentSeries });
        }

        var studyStatus = jobTaskStatus;
        if (seriesList.Any(s => s.PrintStatus == (int)JobTaskStatus.Processing))
        {
            studyStatus = JobTaskStatus.Processing;
        }

        var study = _studyService.GetStudiesByIds(new[] { studyId }).FirstOrDefault();
        if (study != null)
        {
            study.PrintStatus = (int)studyStatus;
            _studyService.UpdatePrintStatus(new[] { study });
        }
    }
}