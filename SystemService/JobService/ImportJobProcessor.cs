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
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.Transfer;
using NV.CT.DicomUtility.Transfer.Import;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.Language;
using NV.CT.MessageService.Contract;
using NV.MPS.Environment;
using System.IO;

namespace NV.CT.JobService
{
    public class ImportJobProcessor : IHostedService, IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ImportJobProcessor> _logger;
        private readonly IJobManagementService _jobManagementService;
        private readonly IMessageService _messageService;
        private readonly IStudyService _studyService;
        private readonly ISeriesService _seriesService;
        private readonly IJobTaskService _jobTaskService;
        private readonly IRawDataService _rawDataService;
        private readonly IPatientService _patientService;
        private readonly IScanTaskService _scanTaskService;
        private readonly IReconTaskService _reconTaskService;
        private readonly JobTaskType _currentJobTaskType = JobTaskType.ImportJob;
        private readonly MessageType _currentMessageType = MessageType.ImportJobResponse;
        private int _totalCountOfItems = 0; //用于记录本次任务中的子项总个数
        private int _processedCount = 0; //用于记录本次任务中当前已处理的子项个数
        private const string PATIENT_SEX_M = "M";
        private const string PATIENT_SEX_F = "F";
        private const string PID_PREFIX = "PID";

        private volatile CancellationTokenSource _tokenSource;
        private volatile Task _task;

        public ImportJobProcessor(IMapper mapper,
                                  ILogger<ImportJobProcessor> logger,
                                  IJobManagementService jobManagementService,
                                  IMessageService messageService,
                                  IStudyService studyService,
                                  ISeriesService seriesService,
                                  IJobTaskService jobTaskService,
                                   IRawDataService rawDataService,        
                                     IPatientService patientService,
                                     IScanTaskService scanTaskService,
                                     IReconTaskService reconTaskService)
        {
            this._mapper = mapper;
            this._logger = logger;
            this._jobManagementService = jobManagementService;
            this._messageService = messageService;
            this._studyService = studyService;
            this._seriesService = seriesService;
            this._jobTaskService = jobTaskService;
            _rawDataService = rawDataService;
            _patientService = patientService;
            _scanTaskService = scanTaskService;
            _reconTaskService = reconTaskService;

            //this._jobManagementService.NewJobEnqueued += OnNewJobEnqueued;
            this._jobManagementService.CancelRunningJob += OnCancelRunningJob;

            //this.TryRunNextJob();
        }

        public void EnqueueNewJob(JobTaskInfo jobTaskInfo)
        {
            //如果不是ImportJob，则不予处理。
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
            //如果不是ImportJob，则不予处理。
            if (e.JobType != this._currentJobTaskType)
            {
                return;
            }

            this._logger.LogTrace($"Request to cancel import jobId:{e.Id}");
            if (this._tokenSource is not null)
            {
                this._logger.LogTrace($"Try to cancel import jobId:{e.Id}");
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
                this._logger.LogTrace("Currently no fetched import job from JobManagementService.");
                return;
            }

            this._tokenSource = new CancellationTokenSource();
            var jobParameter = JsonConvert.DeserializeObject<ImportJobRequest>(newJob.Parameter);
            this._logger.LogTrace($"ImportJobProcessor TryRunNextJob: Fetched job is:{newJob.Parameter}");
            if (jobParameter is null)
            {
                return;
            }
            IImportTaskExecutor importByDirExecutor=null;
            string targetRootPath = Path.Combine(RuntimeConfig.Console.MCSAppData.Path);
            //开始执行导入
            if (jobParameter.IsRawDataImport)
            {
                 importByDirExecutor = new ImportByRawDataExecutor(jobParameter.Id, jobParameter.SourcePath, targetRootPath, this._logger, this._tokenSource,
                     _rawDataService,_studyService,_patientService,_scanTaskService,_reconTaskService,_seriesService, _mapper);
            }
            else
            {
                 importByDirExecutor = new ImportByDirExecutor(jobParameter.Id, jobParameter.SourcePath, targetRootPath, this._logger, this._tokenSource);
            }
            importByDirExecutor.ExecuteStatusChanged += OnExecuteStatusChanged;

            this._task = new Task(
             () =>
            {
                try
                {
                     Process(importByDirExecutor, jobParameter);
                }
                catch (OperationCanceledException canceledException)
                {
                    this._logger.LogWarning($"ImportJobProcessor is cancelled for jobId:{jobParameter.Id}, the excetion is:{canceledException.Message}");

                    //更新状态并通知执行结果
                    this.UpdateJobTaskStatus(jobParameter.Id, JobTaskStatus.Cancelled);
                    this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Cancelled, importByDirExecutor.PatientNameListString, string.Empty, this._processedCount, this._totalCountOfItems);
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning($"ImportJobProcessor failed for jobId:{jobParameter.Id} with exception:{ex.Message}");

                    //更新状态并通知执行结果
                    string errorMessage = ex.Message;
                    this.UpdateJobTaskStatus(jobParameter.Id, JobTaskStatus.Failed);
                    this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Failed, importByDirExecutor.PatientNameListString, errorMessage, this._processedCount, this._totalCountOfItems);
                }
                finally
                {
                    //取消事件，便于对象回收释放
                    if (importByDirExecutor is not null)
                    {
                        importByDirExecutor.ExecuteStatusChanged -= OnExecuteStatusChanged;
                        importByDirExecutor = null;
                    }
                }
            }, this._tokenSource.Token, TaskCreationOptions.LongRunning);
            this._task?.RunSynchronously();
            this.TryRunNextJob();
        }

        private void Process(IImportTaskExecutor importSCUExecutor, ImportJobRequest jobParameter)
        {
            this._processedCount = 0;
            this._totalCountOfItems = 0;
            this._logger.LogTrace($"ImportJobProcessor begins with JobID:{jobParameter.Id}");

            //开始处理任务
             importSCUExecutor.Start();
            this._logger.LogTrace($"ImportJobProcessor finished with JobID:{jobParameter.Id}");

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
                   
                    try
                    {
                        this.CheckIfAskedToCancel();
                        if (e.Data is not null)
                        {
                            var dicomDirectoryInfo = e.Data as List<DicomPatientInfo>;
                            if (dicomDirectoryInfo != null) { SaveDicomInfo(dicomDirectoryInfo); }                            
                        }

                        this.UpdateJobTaskStatus(e.JobTaskID, JobTaskStatus.Completed);
                        this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Completed, e.Tips, string.Empty, e.ProcessedCount, e.TotalCount);
                    }
                    catch (Exception ex)
                    {
                        this.UpdateJobTaskStatus(e.JobTaskID, JobTaskStatus.Failed);
                        this.SendJobTaskMessage(e.JobTaskID, this._currentMessageType, JobTaskStatus.Failed, e.Tips, ex.Message, e.ProcessedCount, e.TotalCount);
                    }
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
                Sender = MessageSource.ImportJob,
                Level = MessageLevel.Info,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson(),
            };
            string message = messageContent;
            if (jobTaskStatus == JobTaskStatus.Processing && processedCount == 0)
            {
                message = $"{LanguageResource.Content_ImportingFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Completed)
            {
                message = $"{LanguageResource.Content_Importing_DoneFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Cancelled)
            {
                message = $"{LanguageResource.Content_Canceled_ImportingFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Failed)
            {
                message = $"{LanguageResource.Content_FailedToImportFor} [{errorMessage}]";
            }
            messageInfo.Content = message;
            this._messageService.SendMessage(messageInfo);
        }

        private void SaveDicomInfo(List<DicomPatientInfo> dicomPatientList)
        {
            var patients = new List<DatabaseService.Contract.Models.PatientModel>();
            var studies = new List<DatabaseService.Contract.Models.StudyModel>();
            var serieses = new List<DatabaseService.Contract.Models.SeriesModel>();

            foreach (var item in dicomPatientList)
            {
                this.CheckIfAskedToCancel();
                var patient = new DatabaseService.Contract.Models.PatientModel
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = string.IsNullOrEmpty(item.PatientId) ? $"{PID_PREFIX}_{IdGenerator.NextRandomID()}" : item.PatientId,
                    PatientName = item.PatientName,
                    CreateTime = DateTime.Now,
                    PatientBirthDate = item.PatientBirthDateTime == null ? DateTime.MinValue : item.PatientBirthDateTime.Value,
                };

                if (item.PatientSex == PATIENT_SEX_M)
                {
                    patient.PatientSex = Gender.Male;
                }
                else if (item.PatientSex == PATIENT_SEX_F)
                {
                    patient.PatientSex = Gender.Female;
                }
                else
                {
                    patient.PatientSex = Gender.Other;
                }
                patients.Add(patient);

                foreach (var item2 in item.StudyList)
                {
                    var study = new DatabaseService.Contract.Models.StudyModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        StudyId = item2.StudyID,
                        InternalPatientId = patient.Id,
                        AccessionNo = item2.AccessionNumber,
                        InstitutionName = item2.InstitutionName,
                        AdmittingDiagnosisDescription = item2.AdmittingDiagnosesDescription,
                        BodyPart = item2.SeriesList.Count > 0 ? item2.SeriesList[0].BodyPartExamined : string.Empty, //fetched from series
                        StudyStatus = WorkflowStatus.ExaminationClosed.ToString(),
                        StudyInstanceUID = item2.StudyInstanceUID,
                        Ward = item.CurrentPatientLocation,
                        PatientType = (int)PatientType.Local,
                        InstitutionAddress = item2.PatientAddress,
                        Comments = item2.StudyDescription,
                        StudyDate = item2.StudyDateTime == null ? DateTime.MinValue : item2.StudyDateTime.Value,
                        StudyTime = item2.StudyDateTime == null ? DateTime.MinValue : item2.StudyDateTime.Value,
                    };

                    if (double.TryParse(item.PatientSize, out var size)) //fetch from dicomPatientInfo
                    {
                        study.PatientSize = size;
                    }

                    if (double.TryParse(item.PatientWeight, out var height)) //fetch from dicomPatientInfo
                    {
                        study.PatientWeight = height;
                    }

                    //按Dicom标准，PatientBirthDateTime类型是2，即属性值必须存在，但值可以是空，所以要考虑值是空的场景，有问题可以联系胡安。
                    if (item.PatientBirthDateTime == null || item.PatientBirthDateTime.Value == DateTime.MinValue)
                    {
                        study.Age = 0;
                        study.AgeType = AgeType.Year;
                    }
                    else
                    {
                        TimeSpan span = DateTime.Now.Subtract((DateTime)item.PatientBirthDateTime); //fetch from dicomPatientInfo
                        int diff = span.Days;
                        if (diff >= 365)
                        {
                            study.Age = (diff / 365);
                            study.AgeType = AgeType.Year;
                        }
                        else if (diff >= 30)
                        {
                            study.Age = (diff / 30);
                            study.AgeType = AgeType.Month;
                        }
                        else if (diff >= 7)
                        {
                            study.Age = (diff / 7);
                            study.AgeType = AgeType.Week;
                        }
                        else
                        {
                            study.Age = diff;
                            study.AgeType = AgeType.Day;
                        }
                    }

                    foreach (var item3 in item2.SeriesList)
                    {
                        var series = new DatabaseService.Contract.Models.SeriesModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            InternalStudyId = study.Id,
                            Modality = item3.Modality,
                            ReconId = string.Empty,
                            SeriesInstanceUID = item3.SeriesInstanceUID,
                            StoreState = 1,
                            SeriesTime = item3.SeriesDateTime,
                            ReconEndDate = item3.SeriesDateTime.HasValue ? item3.SeriesDateTime.Value : DateTime.MinValue,
                            SeriesDescription = item3.SeriesDescription,
                            ImageCount = item3.ImageList.Count,
                            PatientPosition = item3.PatientPosition,
                            BodyPart = item3.BodyPartExamined,
                            SeriesType = Constants.SERIES_TYPE_IMAGE,
                            ImageType = item3.ImageType,
                            SeriesNumber = item3.SeriesNumber.HasValue ? item3.SeriesNumber.Value.ToString() : string.Empty,
                            WindowWidth = item3.WindowWidth,
                            WindowLevel = item3.WindowLevel,
                        };

                        if (item3.ImageType == Constants.SERIES_TYPE_SR)
                        {
                            series.SeriesType = Constants.SERIES_TYPE_SR;
                            series.BodyPart = study.BodyPart;
                            series.ImageCount = 0;
                        }
                        else if (item3.ImageType == Constants.SERIES_TYPE_DOSE_REPORT)
                        {
                            series.SeriesType = Constants.SERIES_TYPE_DOSE_REPORT;
                            series.BodyPart = study.BodyPart;
                            series.ImageCount = 1;
                        }

                        if (item3.ImageList.Count > 0)
                        {
                            var imageModel = item3.ImageList.FirstOrDefault(s => s.Path != null);
                            if (imageModel != null)
                            {
                                string seriesPath = imageModel.Path.Substring(0, imageModel.Path.LastIndexOf("\\"));//根据文件路径获取到该文件夹
                                series.SeriesPath = seriesPath;
                            }
                        }
                        serieses.Add(series);
                    }
                    studies.Add(study);
                }
            }

            try
            {
                this.CheckIfAskedToCancel();
                _studyService.InsertPatientListStudyListAndSeriesList(patients, studies, serieses);
            }
            catch (Exception ex)
            {
                this._logger?.LogError($"[ImportJobProcessor][SaveDicomInfo]:Failed to execute InsertPatientListStudyListAndSeriesList [{patients.Select(p => p.PatientName).ToArray()}] with error reason:{ex.Message}");
                throw;
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
}