//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0        胡安
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
using NV.CT.DicomUtility.Transfer.CStoreScu;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.MessageService.Contract;
using System.Collections.Concurrent;
using System.Data.Common;

namespace NV.CT.JobService
{
    public class ArchiveJobProcessor : IHostedService, IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ArchiveJobProcessor> _logger;
        private readonly IJobManagementService _jobManagementService;
        private readonly IMessageService _messageService;
        private readonly IStudyService _studyService;
        private readonly ISeriesService _seriesService;
        private readonly IJobQueueHandler _jobQueueHandler;
        private readonly JobTaskType _currentJobTaskType = JobTaskType.ArchiveJob;
        private readonly MessageType _currentMessageType = MessageType.ArchiveJobResponse;
        private readonly IEchoVerificationHandler _echoVerificationHandler;
        private ConcurrentBag<(JobTaskStatus, string,string)> _processedStatusList; //用于记录本次任务中的各个子项执行结果状态
        private int _totalCountOfItems = 0; //用于记录本次任务中的子项总个数
        private int _processedCount = 0; //用于记录本次任务中当前已处理的子项个数
        private const string MESSAGE_CANCELLED = "Cancelled";
        private C_StoreSCUExecutor _storeScuExecutor = null;
        private string _studyId = string.Empty;
        private string _currentSeriesId = string.Empty;

        private volatile CancellationTokenSource _tokenSource;
        private volatile Task _task;

        public ArchiveJobProcessor(IMapper mapper,
                                   ILogger<ArchiveJobProcessor> logger,
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
            this._jobQueueHandler = jobQueueHandler;
            this._echoVerificationHandler = new EchoVerificationHandler();

            this._processedStatusList = new ConcurrentBag<(JobTaskStatus, string, string)>();
            //this._jobManagementService.NewJobEnqueued += OnNewJobEnqueued;
            this._jobManagementService.CancelRunningJob += OnCancelRunningJob;
        }

        public void EnqueueNewJob(JobTaskInfo jobTaskInfo)
        {
            //如果不是ArchiveJob，则不予处理。
            if (jobTaskInfo.JobType != this._currentJobTaskType)
            {
                return;
            }

            //通知有新任务加入
            this._jobQueueHandler.SendJobTaskMessage(jobTaskInfo.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Queued, string.Empty, 0, 1);
            var jobParameter = JsonConvert.DeserializeObject<ArchiveJobRequest>(jobTaskInfo.Parameter);
            if (jobParameter is null) return;
            UpdateArchiveStatusOfStudy(jobParameter.StudyId, JobTaskStatus.Queued);
            foreach (var seriesId in jobParameter.SeriesIdList)
            {
                //修改序列的归档状态为处理中
                this.UpdateArchiveStatusOfSeries(seriesId, JobTaskStatus.Queued);
            }
            this.TryRunNextJob(); 
        }
        
        private void OnCancelRunningJob(object? sender, JobTaskInfo e)
        {
            //如果不是ArchiveJob，则不予处理。
            if (e.JobType != this._currentJobTaskType)
            {
                return;
            }

            //通知有任务取消
            this._jobQueueHandler.SendJobTaskMessage(e.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Cancelled, string.Empty, 0, 1);

            this._logger.LogTrace($"Request to cancel archive jobId:{e.Id}");
            if (this._tokenSource is not null)
            {
                this._logger.LogTrace($"Try to cancel archive jobId:{e.Id}");
                this._tokenSource.Cancel();
            }

        }

        private  void TryRunNextJob()
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

            var jobParameter = JsonConvert.DeserializeObject<ArchiveJobRequest>(newJob.Parameter);
            if (jobParameter is null) return;
            this._logger.LogTrace($"ArchiveJobProcessor TryRunNextJob: Fetched job is:{newJob.Parameter}");

            //清空子项执行结果状态记录列表
            this._processedStatusList.Clear();
            this._totalCountOfItems = jobParameter.SeriesIdList.Count;
            this._processedCount = 0;
            this._logger.LogTrace($"ArchiveJobProcessor process begins with JobID:{jobParameter.Id}");

            //使用C-Echo服务验证SCP是否可用
            var echoResult = this._echoVerificationHandler.VerifyEcho(jobParameter.Host, jobParameter.Port, jobParameter.AECaller, jobParameter.AETitle);
            //如果验证失败,则修改任务状态，并发出错误信息
            if (echoResult.Item1 == false)
            {
                //更新状态并通知执行结果
                this.UpdateArchiveStatus(jobParameter.Id, jobParameter.StudyId, jobParameter.SeriesIdList.Count > 0 ? jobParameter.SeriesIdList[0] : string.Empty, JobTaskStatus.Failed);
                //更新最终TaskStatus并通知执行结果
                this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Failed, echoResult.Item2, this._processedCount, this._totalCountOfItems);
                this._logger.LogTrace($"ArchiveJobProcessor failed with JobID:{jobParameter.Id}, task status is:{echoResult.Item1.ToString()}");
                return;
            }

            this._tokenSource = new CancellationTokenSource();

            //通知开始处理job
            this._jobQueueHandler.SendJobTaskMessage(newJob.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Processing, string.Empty, 0, jobParameter.SeriesIdList.Count);

            this._task = new Task(
                () =>
                {
                    try
                    {
                        ProcessAsync(jobParameter);
                    }
                    catch (OperationCanceledException canceledException)
                    {
                        this._logger.LogWarning($"ArchiveJobProcessor cancel archive for jobId:{jobParameter.Id}, the exception is:{canceledException.Message}");

                        //更新状态并通知执行结果
                        this.UpdateArchiveStatus(jobParameter.Id, jobParameter.StudyId, this._currentSeriesId, JobTaskStatus.Cancelled);
                        this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Cancelled, MESSAGE_CANCELLED, this._processedCount, this._totalCountOfItems);
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogWarning($"ArchiveJobProcessor failed to archive for jobId:{jobParameter.Id} with exception:{ex.Message}");

                        //更新状态并通知执行结果
                        this.UpdateArchiveStatus(jobParameter.Id, jobParameter.StudyId, this._currentSeriesId, JobTaskStatus.Failed);
                        string errorMessage = ex.Message;
                        this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Failed, errorMessage, this._processedCount, this._totalCountOfItems);
                    }
                    finally
                    {
                        //取消事件，便于对象回收释放
                        //if (_storeScuExecutor is not null)
                        //{
                        //    _storeScuExecutor.ExecuteStatusInfoChanged -= OnExecuteStatusChanged;
                        //    _storeScuExecutor = null;
                        //}
                    }

                }, this._tokenSource.Token, TaskCreationOptions.LongRunning);
            this._task?.RunSynchronously();
            this.TryRunNextJob();
        }

        private void ProcessAsync(ArchiveJobRequest jobParameter)
        {
            //开始执行归档
            string workflowId = jobParameter.Id;
            this._studyId = jobParameter.StudyId;
            string host = jobParameter.Host;
            var port = jobParameter.Port;
            var callingAE = jobParameter.AECaller;
            var calledAE = jobParameter.AETitle;
            var useTls = jobParameter.UseTls;
            var anonymous = jobParameter.Anonymous;
            if (!Enum.TryParse<SupportedTransferSyntax>(jobParameter.DicomTransferSyntax, true, out SupportedTransferSyntax dicomTransferSyntaxType))
            {
                this._logger.LogWarning($"Failed to parse {jobParameter.DicomTransferSyntax} to SupportedTransferSyntax in ArchiveJobProcessor.");
                dicomTransferSyntaxType = SupportedTransferSyntax.ImplicitVRLittleEndian;
            }

            //修改JobTask的任务状态为处理中
            _jobQueueHandler.UpdateTaskStatusByJobId(workflowId, JobTaskStatus.Processing);
            //修改Study的归档状态为处理中
            UpdateArchiveStatusOfStudy(this._studyId, JobTaskStatus.Processing);

            foreach (var seriesId in jobParameter.SeriesIdList)
            {
                this._currentSeriesId = seriesId;

                //检查是否收到取消命令
                this.CheckIfAskedToCancel();

                var seriesModel = _seriesService.GetSeriesById(seriesId);
                string seriesPath = seriesModel.SeriesPath;
                //修改序列的归档状态为处理中
                this.UpdateArchiveStatusOfSeries(seriesId, JobTaskStatus.Processing);
                _storeScuExecutor = new C_StoreSCUExecutor(this._logger,
                                                           this._tokenSource,
                                                           workflowId,
                                                           seriesId,
                                                           seriesPath,
                                                           host,
                                                           port,
                                                           callingAE,
                                                           calledAE,
                                                           FellowOakDicom.Network.DicomPriority.High,
                                                           dicomTransferSyntaxType,
                                                           useTls,
                                                           anonymous);
                _storeScuExecutor.ExecuteStatusInfoChanged += OnExecuteStatusChanged;
                _storeScuExecutor.StartAnsyc();

                //发送进度信息
                this._processedCount++;
                this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Processing, string.Empty, this._processedCount, this._totalCountOfItems);
            }

            //计算最终的JobTaskStatus:如果没有任何一个series归档成功，则Study归档状态为失败；
            //                        如果部分series归档成功，则Study归档状态为部分成功；
            //                        如果所有series都归档成功，则Study归档状态为成功；
            string errorMessage = string.Empty;
            var completedCount = this._processedStatusList.Where(s => s.Item1 == JobTaskStatus.Completed&&s.Item3== jobParameter.Id).Count();
            var workflowTaskCount = this._processedStatusList.Where(s =>  s.Item3 == jobParameter.Id).Count();
            var failedCount = this._processedStatusList.Where(s => s.Item1 == JobTaskStatus.Failed&& s.Item3== jobParameter.Id).Count();
            this._logger.LogTrace($"ArchiveJobProcessor with JobID:{jobParameter.Id} , _processedStatusList count:{_processedStatusList.Count}, completedcount is:{completedCount},failedcount is :{failedCount},the studyID is :{jobParameter.StudyId}");
            JobTaskStatus jobTaskStatus = JobTaskStatus.Unknown;
            if (completedCount == 0)
            {
                jobTaskStatus = JobTaskStatus.Failed;
                errorMessage = failedCount > 0 ? this._processedStatusList.First(s => s.Item1 == JobTaskStatus.Failed).Item2 : string.Empty;
            }
            else if (completedCount == workflowTaskCount)
            {
                jobTaskStatus = JobTaskStatus.Completed;
            }
            else
            {
                jobTaskStatus = JobTaskStatus.PartlyCompleted;
                errorMessage = failedCount > 0 ? this._processedStatusList.First(s => s.Item1 == JobTaskStatus.Failed).Item2 : string.Empty;
            }

            //修改JobTask任务状态
            _jobQueueHandler.UpdateTaskStatusByJobId(workflowId, jobTaskStatus);
            //修改检查的最终归档状态
            CheckAndUpdateStudyArchiveStatus(jobParameter.StudyId, _currentSeriesId);

            //更新最终TaskStatus并通知执行结果
            this._jobQueueHandler.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, MessageSource.ArchiveJob, jobTaskStatus, errorMessage, completedCount, workflowTaskCount);
            this._logger.LogTrace($"ArchiveJobProcessor finished with JobID:{jobParameter.Id}, task status is:{jobTaskStatus.ToString()},the studyID is :{jobParameter.StudyId}");

            //尝试运行下个任务
            //this.TryRunNextJob();
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

        private void OnExecuteStatusChanged(object? sender, ExecuteStatusInfo e)
        {
            if (e.SeriesID is not null)
            {
                if (e.Status == ExecuteStatus.Succeeded)
                {
                    this._processedStatusList.Add((JobTaskStatus.Completed, string.Empty,e.JobTaskID));
                    //修改序列的归档状态为成功
                    this.UpdateArchiveStatusOfSeries(e.SeriesID, JobTaskStatus.Completed);
                }
                else if (e.Status == ExecuteStatus.Failed)
                {
                    this._processedStatusList.Add((JobTaskStatus.Failed, e.Tips,e.JobTaskID));
                    //修改序列的归档状态失败
                    this.UpdateArchiveStatusOfSeries(e.SeriesID, JobTaskStatus.Failed);
                }
            }
        }

        private void UpdateArchiveStatusOfSeries(string seriesId, JobTaskStatus jobTaskStatus)
        {
            var series = this._seriesService.GetSeriesById(seriesId);
            series.ArchiveStatus = (int)jobTaskStatus;
            this._seriesService.UpdateArchiveStatus(new List<DatabaseService.Contract.Models.SeriesModel> { series });
        }

        private void UpdateArchiveStatusOfStudy(string studyId, JobTaskStatus jobTaskStatus)
        {
            var studyIds = new string[] { studyId };
            var study = this._studyService.GetStudiesByIds(studyIds).SingleOrDefault();
            if (study is not null)
            {
                study.ArchiveStatus = (int)jobTaskStatus;
                this._studyService.UpdateArchiveStatus(new List<DatabaseService.Contract.Models.StudyModel> { study });
            }
        }

        private void CheckAndUpdateStudyArchiveStatus(string studyId,string seriesId)
        {
            var studyIds = new string[] { studyId };
            var study = this._studyService.GetStudiesByIds(studyIds).Single();
            var seriesList = this._seriesService.GetSeriesByStudyId(studyId);
            var seriesRTDId=this._seriesService.GetSeriesIdByStudyId(studyId);
            var seriesModel=seriesList.FirstOrDefault(r => r.Id == seriesRTDId);
            if (seriesModel != null) { seriesList.Remove(seriesModel); };
            var completedCount = seriesList.Where(s => s.ArchiveStatus == (int)JobTaskStatus.Completed).Count();
            if (completedCount > 0 && completedCount == seriesList.Count)
            {
                study.ArchiveStatus = (int)JobTaskStatus.Completed;
            }
            else if (completedCount > 0 && completedCount < seriesList.Count)
            {
                study.ArchiveStatus = (int)JobTaskStatus.PartlyCompleted;
            }
            else
            {
                study.ArchiveStatus = (int)JobTaskStatus.Failed;
            }
            if (seriesRTDId== seriesId)
            {
                study.ArchiveStatus = (int)JobTaskStatus.PartlyCompleted;
            }
            this._studyService.UpdateArchiveStatus(new List<DatabaseService.Contract.Models.StudyModel> { study });
        }

        private void UpdateArchiveStatus(string jobId, string studyId, string seriesId, JobTaskStatus jobTaskStatus)
        {
            //Update ArchiveStatus of job task
            _jobQueueHandler.UpdateTaskStatusByJobId(jobId, jobTaskStatus);

            //Update ArchiveStatus of Study
            UpdateArchiveStatusOfStudy(studyId, jobTaskStatus);

            //Update ArchiveStatus of Series
            UpdateArchiveStatusOfSeries(seriesId, jobTaskStatus);
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
}