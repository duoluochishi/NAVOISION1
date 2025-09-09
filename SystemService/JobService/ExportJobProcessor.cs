//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/5/15 13:45:36    V1.0.0         胡安
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
using NV.CT.DicomUtility.Transfer.Export;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.Language;
using NV.CT.MessageService.Contract;
using NV.MPS.Environment;
using System.Drawing.Imaging;

namespace NV.CT.JobService
{
    public class ExportJobProcessor : IHostedService, IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ExportJobProcessor> _logger;
        private readonly IJobManagementService _jobManagementService;
        private readonly IMessageService _messageService;
        private readonly IJobTaskService _jobTaskService;
        private readonly IRawDataService _rawDataService;
        private readonly IStudyService _studyService;
        private readonly IPatientService _patientService;
        private readonly IScanTaskService _scanTaskService;
        private readonly IReconTaskService _reconTaskService;
        private readonly ISeriesService _seriesService;
        private readonly JobTaskType _currentJobTaskType = JobTaskType.ExportJob;
        private readonly MessageType _currentMessageType = MessageType.ExportJobResponse;
        private int _totalCountOfItems = 0; //用于记录本次任务中的子项总个数
        private int _processedCount = 0; //用于记录本次任务中当前已处理的子项个数

        private volatile CancellationTokenSource _tokenSource;
        private volatile Task _task;

        public ExportJobProcessor(IMapper mapper,
                                  ILogger<ExportJobProcessor> logger,
                                  IJobManagementService jobManagementService,
                                  IMessageService messageService,
                                  IJobTaskService jobTaskService,
                                  IRawDataService rawDataService,
                                     IStudyService studyService,
                                     IPatientService patientService,
                                     IScanTaskService scanTaskService,
                                     IReconTaskService reconTaskService,
                                     ISeriesService seriesService)
        {
            this._mapper = mapper;
            this._logger = logger;
            this._jobManagementService = jobManagementService;
            this._messageService = messageService;
            this._jobTaskService = jobTaskService;
            this._rawDataService = rawDataService;
            _studyService = studyService;
            _patientService = patientService;
            _scanTaskService = scanTaskService;
            _reconTaskService = reconTaskService;
            _seriesService = seriesService;
            //this._jobManagementService.NewJobEnqueued += OnNewJobEnqueued;
            this._jobManagementService.CancelRunningJob += OnCancelRunningJob;

            //this.TryRunNextJob();
        }

        public void EnqueueNewJob(JobTaskInfo jobTaskInfo)
        {
            //如果不是ExportJob，则不予处理。
            if (jobTaskInfo.JobType != this._currentJobTaskType)
            {
                return;
            }

            //通知有新任务加入
            this.SendJobTaskMessage(jobTaskInfo.Id, this._currentMessageType, JobTaskStatus.Queued, string.Empty, string.Empty, 0, 1);

            this.TryRunNextJob();
        }

        private void OnCancelRunningJob(object? sender, JobTaskInfo e)
        {
            //如果不是ExportJob，则不予处理。
            if (e.JobType != this._currentJobTaskType)
            {
                return;
            }

            this._logger.LogTrace($"Request to cancel export jobId:{e.Id}");
            if (this._tokenSource is not null)
            {
                this._logger.LogTrace($"Try to cancel export jobId:{e.Id}");
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
            if (newJob is null || string.IsNullOrEmpty(newJob.Id) )
            {
                this._logger.LogTrace("Currently no fetched export job from JobManagementService.");
                return;
            }

            var jobParameter = JsonConvert.DeserializeObject<ExportJobRequest>(newJob.Parameter);
            this._logger.LogTrace($"ExportJobProcessor TryRunNextJob: Fetched job is:{newJob.Parameter}");

            string[] sourcePaths = jobParameter.InputFolders.ToArray();
            string targetRootPath = jobParameter.OutputFolder;
            string binPath = RuntimeConfig.Console.MCSBin.Path;
            string[] patientNames = jobParameter.PatientNames.ToArray();
            bool isAnonymouse = jobParameter.IsAnonymouse;
            bool isCorrected = jobParameter.IsCorrected;
            bool isBurnToCDROM = jobParameter.IsBurnToCDROM;
            bool isAddViewer = jobParameter.IsAddViewer;
            string[] rawDataIDList=jobParameter.SeriesIdList.ToArray();
            string[] rtdDicomList=jobParameter.RTDDicomFolders.ToArray();
            
            Enum.TryParse<SupportedTransferSyntax>(jobParameter.DicomTransferSyntax, false, out SupportedTransferSyntax dicomTransferSyntax);

            this._tokenSource = new CancellationTokenSource();
            ITaskExecutor exportTaskExecutor = null;
            if (jobParameter.IsExportedToDICOM) 
            {
                exportTaskExecutor = new ExportToDicomExecutor(_logger, _tokenSource, newJob.Id, patientNames,
                                                               sourcePaths, targetRootPath, binPath, 
                                                               isAnonymouse, isCorrected, isBurnToCDROM, isAddViewer, dicomTransferSyntax);
            }
            else if(jobParameter.IsExportedToImage)
            {
                var imageFormatType = ConvertToImageFormat(jobParameter.PictureType is null ? FileExtensionType.Png : jobParameter.PictureType.Value);
                exportTaskExecutor = new ExportToImageExecutor(_logger, _tokenSource, newJob.Id, patientNames,
                                                               sourcePaths, targetRootPath, binPath, imageFormatType, isBurnToCDROM, dicomTransferSyntax);
            }else if(jobParameter.IsExportedToRawData)
            {
                exportTaskExecutor = new ExportToRawDataExecutor(_logger, _tokenSource, newJob.Id, patientNames,
                                               sourcePaths, rawDataIDList, rtdDicomList, targetRootPath, _rawDataService,_studyService,_patientService,
                                               _scanTaskService,_reconTaskService,_seriesService, jobParameter.StudyId);
            }
            exportTaskExecutor.ExecuteStatusChanged += OnExecuteStatusChanged;

            this._task = new Task(() => {
                try 
                { 
                    Process(exportTaskExecutor, jobParameter);
                }
                catch (OperationCanceledException canceledException)
                {
                    this._logger.LogWarning($"ExportJobProcessor is cancelled for jobId:{jobParameter.Id}, the exception is:{canceledException.Message}");

                    //更新状态并通知执行结果
                    this.UpdateJobTaskStatus(jobParameter.Id, JobTaskStatus.Cancelled);

                    string patientNameList = jobParameter.IsExportedToDICOM ? ((ExportToDicomExecutor)exportTaskExecutor).PatientNameListString :
                                                                              ((ExportToImageExecutor)exportTaskExecutor).PatientNameListString;
                    this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Cancelled, patientNameList, string.Empty, this._processedCount, this._totalCountOfItems);
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning($"ExportJobProcessor failed for jobId:{jobParameter.Id} with exception:{ex.Message}");

                    //更新状态并通知执行结果
                    string errorMessage = ex.Message;
                    this.UpdateJobTaskStatus(jobParameter.Id, JobTaskStatus.Failed);
                    string patientNameList = jobParameter.IsExportedToDICOM ? ((ExportToDicomExecutor)exportTaskExecutor).PatientNameListString :
                                                                              ((ExportToImageExecutor)exportTaskExecutor).PatientNameListString;
                    this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Failed, patientNameList, errorMessage, this._processedCount, this._totalCountOfItems);                                
                }
                finally
                {
                    //取消事件，便于对象回收释放
                    if (exportTaskExecutor is not null)
                    {
                        exportTaskExecutor.ExecuteStatusChanged -= OnExecuteStatusChanged;
                        exportTaskExecutor = null;
                    }
                } 
            }, this._tokenSource.Token, TaskCreationOptions.LongRunning);
            this._task?.RunSynchronously();
            this.TryRunNextJob();
        }

        private void Process(ITaskExecutor exportExecutor, ExportJobRequest jobParameter)
        {
            this.CheckIfAskedToCancel();

            this._processedCount = 0;
            this._totalCountOfItems = 0;
            this._logger.LogTrace($"ExportJobProcessor begins with JobID:{jobParameter.Id}");

            //开始处理任务
            exportExecutor.Start();
            this._logger.LogTrace($"ExportJobProcessor finished with JobID:{jobParameter.Id}");

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
            this._processedCount = e.ProcessedCount;
            this._totalCountOfItems = e.TotalCount;

            switch (e.Status)
            {
                case ExecuteStatus.Started:
                    this.UpdateJobTaskStatus(e.JobTaskID, JobTaskStatus.Processing);
                    this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Processing, e.Tips, string.Empty, e.ProcessedCount, e.TotalCount);
                    break;

                case ExecuteStatus.InProgress:
                    this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Processing, e.Tips, string.Empty, e.ProcessedCount, e.TotalCount);
                    break;
                case ExecuteStatus.Succeeded:
                    this.UpdateJobTaskStatus(e.JobTaskID, JobTaskStatus.Completed);
                    this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Completed, e.Tips, string.Empty, e.ProcessedCount, e.TotalCount);
                    break;

                case ExecuteStatus.Failed:
                    this.UpdateJobTaskStatus(e.JobTaskID, JobTaskStatus.Failed);
                    string errorMessage = e.Data is null ? string.Empty : e.Data.ToString();
                    this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Failed, e.Tips, errorMessage, e.ProcessedCount, e.TotalCount);
                    break;

                case ExecuteStatus.Cancelled:
                    this.UpdateJobTaskStatus(e.JobTaskID, JobTaskStatus.Cancelled);
                    this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Cancelled, e.Tips, string.Empty, e.ProcessedCount, e.TotalCount);
                    break;
                default:
                    break;            
            }

        }

        private void UpdateJobTaskStatus(string jobId, JobTaskStatus jobTaskStatus)
        {
            //Update ImportStatus of job task
            this._jobTaskService.UpdateTaskStatusByJobId(jobId, jobTaskStatus.ToString());
        }

        private void SendJobTaskMessage(string jobId,
                                        MessageType messageType,
                                        JobTaskStatus jobTaskStatus,
                                        string messageContent,
                                        string errorMessage,
                                        int processedCount,
                                        int totalCount)
        {
            //发送消息通知
            var jobTaskMessage = new JobTaskMessage()
            {
                JobId = jobId,
                MessageType = messageType,
                JobStatus = jobTaskStatus,
                Content = messageContent,
                ProgressedCount = processedCount,
                TotalCount = totalCount,
            };
            MessageInfo messageInfo = new MessageInfo()
            {
                Sender = MessageSource.ExportJob,
                Level = MessageLevel.Info,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson(),
            };
            string message = $"{LanguageResource.Content_Exporting}";
            if (!string.IsNullOrEmpty(messageContent))
            {
                message = $"{LanguageResource.Content_ExportingFor} [{messageContent}]";
            }
            //不用始终在消息框显示进度变更信息，只需在首次时提醒一次即可, 否则进度消息会引起刷屏
            if (jobTaskStatus == JobTaskStatus.Processing && processedCount == 0)
            {
                message = $"{LanguageResource.Content_ExportingFor}  [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Completed)
            {
                message = $"{LanguageResource.Content_ExportingDoneFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Cancelled)
            {
                message = $"{LanguageResource.Content_CanceledExportingFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Failed)
            {
                message = $"{LanguageResource.Content_FailedToExportingFor} [{errorMessage}]";
            }
            messageInfo.Content = message;
            this._messageService.SendMessage(messageInfo);
        }

        private ImageFormat ConvertToImageFormat(FileExtensionType fileExtensionType)
        {
            ImageFormat imageFormat = ImageFormat.Png;
            switch (fileExtensionType)
            {
                case FileExtensionType.Bmp:
                    imageFormat = ImageFormat.Bmp;
                    break;
                case FileExtensionType.Gif:
                    imageFormat = ImageFormat.Gif;
                    break;
                case FileExtensionType.Jpeg:
                    imageFormat = ImageFormat.Jpeg;
                    break;
                case FileExtensionType.Png:
                    imageFormat = ImageFormat.Png;
                    break;
                default:
                    return imageFormat;
            }
            return imageFormat;
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
