//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.Transfer;
using NV.CT.DicomUtility.Transfer.CEchoSCU;
using NV.CT.DicomUtility.Transfer.PrintSCU;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.MessageService.Contract;
using System.Collections.Concurrent;

namespace NV.CT.JobService;

public class PrintJobProcessor : IHostedService, IJobProcessor
{
    private readonly IMapper _mapper;
    private readonly ILogger<PrintJobProcessor> _logger;
    private readonly IJobManagementService _jobManagementService;
    private readonly IMessageService _messageService;
    private readonly IStudyService _studyService;
    private readonly ISeriesService _seriesService;
    private readonly IJobQueueHandler _jobQueueHandler;
    private readonly JobTaskType _currentJobTaskType = JobTaskType.PrintJob;
    private readonly MessageType _currentMessageType = MessageType.PrintJobResponse;
    private readonly IEchoVerificationHandler _echoVerificationHandler;
    private ConcurrentBag<JobTaskStatus> _processedStatusList; //用于记录本次任务中的各个子项执行结果状态
    private int _totalCountOfItems = 0; //用于记录本次任务中的子项总个数
    private int _processedCount = 0; //用于记录本次任务中当前已处理的子项个数
    private const string MESSAGE_CANCELLED = "Cancelled";

    private volatile CancellationTokenSource _tokenSource;
    private volatile Task _task;

    public PrintJobProcessor(IMapper mapper,
                             ILogger<PrintJobProcessor> logger,
                             IJobManagementService jobManagementService,
                             IMessageService messageService,
                             IStudyService studyService,
                             ISeriesService seriesService,
                             IJobQueueHandler jobQueueHandler)
    {
        this._mapper = mapper;
        this._logger = logger;
        this._jobManagementService = jobManagementService;
        this._messageService = messageService;
        this._studyService = studyService;
        this._seriesService = seriesService;
        this._echoVerificationHandler = new EchoVerificationHandler();

        this._processedStatusList = new ConcurrentBag<JobTaskStatus>();
        //this._jobManagementService.NewJobEnqueued += OnNewJobEnqueued;
        this._jobManagementService.CancelRunningJob += OnCancelRunningJob;
        this._jobQueueHandler = jobQueueHandler;
    }


    public void EnqueueNewJob(JobTaskInfo jobTaskInfo)
    {
        //如果不是PrintJob，则不予处理。
        if (jobTaskInfo.JobType != this._currentJobTaskType)
        {
            return;
        }

        //通知有新任务加入
        this._jobQueueHandler.SendJobTaskMessage(jobTaskInfo.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Queued, string.Empty, 0, 1);

        this.TryRunNextJob();
    }

    private void OnCancelRunningJob(object? sender, JobTaskInfo e)
    {
        //如果不是PrintJob，则不予处理。
        if (e.JobType != this._currentJobTaskType)
        {
            return;
        }

        this._logger.LogTrace($"Request to cancel print jobId:{e.Id}");
        if (this._tokenSource is not null)
        {
            this._logger.LogTrace($"Try to cancel print jobId:{e.Id}");
            this._tokenSource.Cancel();
        }

    }

    private void TryRunNextJob()
    {
        //If the previous job is still running, we will try again when the previous job is finished.
        if (_task is not null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingToRun ||
                                  _task.Status == TaskStatus.WaitingForActivation || _task.Status == TaskStatus.WaitingForChildrenToComplete))
        {
            this._logger.LogTrace($"Previous job is still running with task status:{_task.Status},so return to wait.");
            return;
        }

        //确保清理上次的令牌资源
        this.DisposePreviousTaskResource();

        var newJob = _jobManagementService.FetchNextAvailableJob(this._currentJobTaskType);
        if (newJob is null || string.IsNullOrEmpty(newJob.Id))
        {
            this._logger.LogTrace("Currently no fetched job from JobManagementService.");
            return;
        }

        var jobParameter = JsonConvert.DeserializeObject<PrintJobRequest>(newJob.Parameter);
        this._logger.LogTrace($"PrintJobProcessor TryRunNextJob: Fetched job is:{newJob.Parameter}");

        //清空子项执行结果状态记录列表
        this._processedStatusList.Clear();
        this._totalCountOfItems = jobParameter.ImagePathList.Count;
        this._processedCount = 0;
        this._logger.LogTrace($"PrintJobProcessor process begins with JobID:{jobParameter.Id}");

        //使用C-Echo服务验证SCP是否可用
        var echoResult = this._echoVerificationHandler.VerifyEcho(jobParameter?.Host, jobParameter.Port, jobParameter?.CallingAE, jobParameter?.CalledAE);
        //如果验证失败,则修改任务状态，并发出错误信息
        if (echoResult.Item1 == false)
        {
            this._logger.LogWarning($"PrintJobProcessor failed to print for jobId:{jobParameter.Id} with exception:{echoResult.Item2}");
            //更新状态并通知执行结果
            this.UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Failed);
            string errorMessage = echoResult.Item2;
            this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Failed, errorMessage, this._processedCount, this._totalCountOfItems);
            return;
        }

        this._tokenSource = new CancellationTokenSource();

        //通知开始处理job
        this._jobQueueHandler.SendJobTaskMessage(newJob.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Processing, string.Empty, 0, jobParameter.ImagePathList.Count);

        //开始执行打印
        var dicomNode = new DicomNode(jobParameter.Host, jobParameter.Port, jobParameter.CallingAE, jobParameter.CalledAE);
        var printJob = new PrintJob(jobParameter.Id, jobParameter.IsColor, jobParameter.NumberOfCopies);
        var printSCUExecutor = new PrintSCUExecutor(this._logger, this._tokenSource);
        printSCUExecutor.ExecuteStatusInfoChanged += OnPrintExecuteStatusChanged;

        this._task = new Task(
        async () =>
        {
            try
            {
                await Process(printSCUExecutor, dicomNode, printJob, jobParameter);
            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"PrintJobProcessor cancel print for jobId:{jobParameter.Id}, the exception is:{canceledException.Message}");
                //更新状态并通知执行结果
                this.UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Cancelled);
                this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Cancelled, MESSAGE_CANCELLED, this._processedCount, this._totalCountOfItems);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning($"PrintJobProcessor failed to print for jobId:{jobParameter.Id} with exception:{ex.Message}");
                //更新状态并通知执行结果
                this.UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Failed);
                string errorMessage = ex.Message;
                this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Failed, errorMessage, this._processedCount, this._totalCountOfItems);
            }
            finally
            {
                //取消事件，便于对象回收释放
                if (printSCUExecutor is not null)
                {
                    printSCUExecutor.ExecuteStatusInfoChanged -= OnPrintExecuteStatusChanged;
                    printSCUExecutor = null;
                }
            }
        }, this._tokenSource.Token, TaskCreationOptions.LongRunning);
        this._task?.Start();
    }

    private async Task Process(PrintSCUExecutor printSCUExecutor, DicomNode dicomNode, PrintJob printJob, PrintJobRequest jobParameter)
    {
        //清空子项执行结果状态记录列表
        this._processedStatusList.Clear();
        this._totalCountOfItems = jobParameter.ImagePathList.Count;
        this._processedCount = 0;
        this._logger.LogTrace($"PrintJobProcessor Process begins with JobID:{jobParameter.Id}");

        //通知开始执行Job，并修改任务状态为处理中
        this.UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, JobTaskStatus.Processing);
        this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Processing, string.Empty, 0, this._totalCountOfItems);

        //开始处理打印任务
        foreach (string imagePath in jobParameter.ImagePathList)
        {
            //检查是否收到取消命令
            this.CheckIfAskedToCancel();

            printJob.FastAddImage(new Bitmap(imagePath), jobParameter.Orientation, jobParameter.PageSize);
            await printSCUExecutor.PrintAsync(dicomNode, printJob);

            //发送进度信息
            this._processedCount++;
            this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.PrintJob, JobTaskStatus.Processing, string.Empty, this._processedCount, this._totalCountOfItems);
        }

        //计算最终的JobTaskStatus: 对于打印任务，只要有任何1个序列打印成功，则该检查的打印状态视为成功，否则视为打印失败。
        //打印任务不存在部分成功的情况。
        var completedCount = this._processedStatusList.Where(s => s == JobTaskStatus.Completed).Count();
        var jobTaskStatus = completedCount > 0 ? JobTaskStatus.Completed : JobTaskStatus.Failed;

        //更新最终TaskStatus并通知执行结果
        this.UpdatePrintStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesID, jobTaskStatus);
        this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.PrintJob, jobTaskStatus, string.Empty, completedCount, this._totalCountOfItems);

        this._logger.LogTrace($"PrintJobProcessor Process finished with JobID:{jobParameter.Id}");

        //尝试运行下个任务
        this.TryRunNextJob();
    }

    private void CheckIfAskedToCancel()
    {
        if (this._tokenSource is null)
        {
            return;
        }

        if (this._tokenSource.Token.IsCancellationRequested)
        {
            this._tokenSource.Token.ThrowIfCancellationRequested();
        }
    }

    private void DisposePreviousTaskResource()
    {
        if (this._tokenSource is not null)
        {
            this._tokenSource.Dispose();
            this._tokenSource = null;
        }
        if (this._task is not null)
        {
            this._task.Dispose();
            this._task = null;
        }
    }

    private void OnPrintExecuteStatusChanged(object? sender, ExecuteStatusInfo e)
    {
        if (e.Status == ExecuteStatus.Succeeded)
        {
            this._processedStatusList.Add((JobTaskStatus.Completed));
        }
    }

    private void UpdatePrintStatus(string jobId, string studyId, string seriesId, JobTaskStatus jobTaskStatus)
    {
        //Update PrintStatus of job task
        this._jobQueueHandler.UpdateTaskStatusByJobId(jobId, jobTaskStatus);

        //Update PrintStatus of Series and Study
        var seriesList = this._seriesService.GetSeriesByStudyId(studyId);
        if (seriesList is null || seriesList.Count == 0)
        {
            return;
        }

        var currentSeries = seriesList.FirstOrDefault(s => s.Id == seriesId);
        if (currentSeries is not null)
        {
            currentSeries.PrintStatus = (int)jobTaskStatus;
            this._seriesService.UpdatePrintStatus(new List<DatabaseService.Contract.Models.SeriesModel> { currentSeries });
        }

        JobTaskStatus studyStatus = jobTaskStatus;
        int countOfProcessing = seriesList.Where(s => s.PrintStatus == (int)JobTaskStatus.Processing).Count();
        if (countOfProcessing > 0)
        {
            studyStatus = JobTaskStatus.Processing;
        }

        var studyIds = new string[] { studyId };
        var study = this._studyService.GetStudiesByIds(studyIds).FirstOrDefault();
        if (study is not null)
        {
            study.PrintStatus = (int)studyStatus;
            this._studyService.UpdatePrintStatus(new List<DatabaseService.Contract.Models.StudyModel> { study });
        }

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (this._jobManagementService is not null)
        {
            //this._jobManagementService.NewJobEnqueued -= OnNewJobEnqueued;
            this._jobManagementService.CancelRunningJob -= OnCancelRunningJob;
        }

        this.DisposePreviousTaskResource();

        return Task.CompletedTask;
    }
}