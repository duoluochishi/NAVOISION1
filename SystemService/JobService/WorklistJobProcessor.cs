//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/11 13:45:36    V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.Transfer;
using NV.CT.DicomUtility.Transfer.CEchoSCU;
using NV.CT.DicomUtility.Transfer.ModalityWorklist;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.MessageService.Contract;
using System.Collections.Concurrent;

namespace NV.CT.JobService
{
    public class WorklistJobProcessor : IHostedService, IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<WorklistJobProcessor> _logger;
        private readonly IJobManagementService _jobManagementService;
        private readonly IMessageService _messageService;
        private readonly IPatientService _patientService;
        private readonly IStudyService _studyService;
        private readonly IJobTaskService _jobTaskService;
        private readonly JobTaskType _currentJobTaskType = JobTaskType.WorklistJob;
        private readonly MessageType _currentMessageType = MessageType.WorklistJobResponse;
        private readonly IEchoVerificationHandler _echoVerificationHandler;
        private readonly ConcurrentQueue<JobTaskInfo> _tasksQueue = new ConcurrentQueue<JobTaskInfo>();

        private int _totalCountOfItems = 0; //用于记录本次任务中的子项总个数
        private int _processedCount = 0; //用于记录本次任务中当前已处理的子项个数

        private const string MESSAGE_CANCELLED = "Cancelled";
        private ModalityWorklistSCUExecutor _dicomWorklistSCUExecutor = null;
        private string _studyId = string.Empty;
        private string _currentSeriesId = string.Empty;

        private volatile CancellationTokenSource _tokenSource;
        private volatile Task _task;

        public WorklistJobProcessor(IMapper mapper,
                                   ILogger<WorklistJobProcessor> logger,
                                   IJobManagementService jobManagementService,
                                   IMessageService messageService,
                                   IStudyService studyService,
                                   IPatientService patientService,
                                   IJobTaskService jobTaskService)
        {
            this._mapper = mapper;
            this._logger = logger;
            this._jobManagementService = jobManagementService;
            this._messageService = messageService;
            this._studyService = studyService;
            this._patientService = patientService;
            this._jobTaskService = jobTaskService;
            this._echoVerificationHandler = new EchoVerificationHandler();

            //this._jobManagementService.NewJobEnqueued += OnNewJobEnqueued;
            this._jobManagementService.CancelRunningJob += OnCancelRunningJob;
        }

        public void EnqueueNewJob(JobTaskInfo jobTaskInfo)
        {
            //如果不是WorklistJob，则不予处理。
            if (jobTaskInfo.JobType != this._currentJobTaskType)
            {
                return;
            }

            _tasksQueue.Enqueue(jobTaskInfo);

            //通知有新任务加入
            this.SendJobTaskMessage(jobTaskInfo.Id, this._currentMessageType, JobTaskStatus.Queued, string.Empty, 0, 1);

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
            this.SendJobTaskMessage(e.Id, this._currentMessageType, JobTaskStatus.Cancelled, string.Empty, 0, 1);

            this._logger.LogTrace($"Request to cancel worklist jobId:{e.Id}");
            if (this._tokenSource is not null)
            {
                this._logger.LogTrace($"Try to cancel worklist jobId:{e.Id}");
                this._tokenSource.Cancel();
            }
        }

        private void TryRunNextJob()
        {
            //If the previous job is still running, we will try again when the previous job is finished.
            if (_task is not null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingToRun ||
                                      _task.Status == TaskStatus.WaitingForActivation || _task.Status == TaskStatus.WaitingForChildrenToComplete))
            {
                //this._logger.LogTrace($"Previous job is still running with task status:{_task.Status},so return to wait.");
                return;
            }

            //确保清理上次的令牌资源
            this.DisposePreviousTaskResource();

            var hasNewJob = _tasksQueue.TryDequeue(out var newJob); 
            if (!hasNewJob)
            {
                //this._logger.LogTrace("Currently no fetched job from JobManagementService.");
                return;
            }

            if (newJob is null || string.IsNullOrEmpty(newJob.Id))
            {
                //this._logger.LogTrace("Currently no fetched job from JobManagementService.");
                return;
            }

            var jobParameter = JsonConvert.DeserializeObject<WorklistJobRequest>(newJob.Parameter);

            //使用C-Echo服务验证SCP是否可用
            var echoResult = this._echoVerificationHandler.VerifyEcho(jobParameter?.Host, jobParameter.Port, jobParameter?.AECaller, jobParameter?.AETitle);
            //如果验证失败,则修改任务状态，并发出错误信息
            if (echoResult.Item1 == false)
            {
                //更新最终TaskStatus并通知执行结果
                this._processedCount = 1;
                this._totalCountOfItems = 1;
                this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Failed, echoResult.Item2, this._processedCount, this._totalCountOfItems);
                //this._logger.LogTrace($"WorklistJobProcessor failed with JobID:{jobParameter.Id}, task status is:{echoResult.Item1.ToString()}");
                return;
            }

            this._tokenSource = new CancellationTokenSource();

            //通知开始处理job
            this.SendJobTaskMessage(newJob.Id, this._currentMessageType, JobTaskStatus.Processing, string.Empty, 0, 1);

            this._task = new Task(
            async () => {
                try
                {
                    await ProcessAsync(jobParameter);
                }
                catch (OperationCanceledException canceledException)
                {
                    this._logger.LogDebug($"WorklistJobProcessor cancelled for jobId:{jobParameter.Id}, the exception is:{canceledException.Message}");
                    this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Cancelled, MESSAGE_CANCELLED, 1, 1);
                }
                catch (Exception ex)
                {
                    //this._logger.LogWarning($"WorklistJobProcessor failed for jobId:{jobParameter.Id} with exception:{ex.Message}");
                    string errorMessage = ex.Message;
                    this.SendJobTaskMessage(jobParameter.Id, this._currentMessageType, JobTaskStatus.Failed, errorMessage, 1, 1);
                }
                finally
                {
                    //取消事件，便于对象回收释放
                    if (_dicomWorklistSCUExecutor is not null)
                    {
                        _dicomWorklistSCUExecutor = null;
                    }
                }
            
            }, this._tokenSource.Token, TaskCreationOptions.LongRunning);

            this._task?.Start();
        }

        private async Task ProcessAsync(WorklistJobRequest jobParameter)
        {
            var dicomNode = new DicomNode(jobParameter.Host, jobParameter.Port, jobParameter.AECaller, jobParameter.AETitle);
            var worklistFilter = new WorklistFilter(jobParameter.Id, 
                                                    jobParameter.StudyDateStart,
                                                    jobParameter.StudyDateEnd);

            _dicomWorklistSCUExecutor = new ModalityWorklistSCUExecutor();
            //this._logger.LogTrace($"ModalityWorkListJobProcessor begins with JobID:{jobParameter.Id}");
            var queryResults = await _dicomWorklistSCUExecutor.QueryAsync(dicomNode, worklistFilter);
            //this._logger.LogTrace($"ModalityWorkListJobProcessor begins with JobID:{jobParameter.Id}");

            this.UpdateStudyQueryResults(jobParameter.Id, queryResults);

            //尝试运行下个任务
            this.TryRunNextJob();
        }

        private void UpdateStudyQueryResults(string jobId, WorklistResult[] worklistResult)
        {
            if (worklistResult is null || worklistResult.Length == 0)
            {
                //this._logger.LogInformation($"worklistResult is empty in UpdateStudyQueryResults");
                return;
            }
            int processingCount = 0;
            foreach (var studyQuery in worklistResult)
            {
                //保存患者
                var patient = new DatabaseService.Contract.Models.PatientModel
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = studyQuery.PatientID,
                    PatientName = studyQuery.PatientName.Trim(),
                    PatientSex = this.CovertToGender(studyQuery.PatientSex),
                    PatientBirthDate = studyQuery.PatientBirthDateTime,
                    CreateTime = DateTime.Now,
                };

                var ageInfo = AgeHelper.CalculateAgeByBirthday(studyQuery.PatientBirthDateTime);
                var study = new DatabaseService.Contract.Models.StudyModel
                {
                    Id = Guid.NewGuid().ToString(),
                    InternalPatientId = patient.Id,
                    AccessionNo = studyQuery.AccessionNumber,
                    ReferringPhysicianName = studyQuery.ReferringPhysicianName,
                    StudyStatus = WorkflowStatus.NotStarted.ToString(),
                    StudyInstanceUID = studyQuery.StudyInstanceUID,
                    PatientType = (int)PatientType.PreRegistration,

                    //MedicalAlerts = studyQuery.MedicalAlerts,
                    //Allergies = studyQuery.Allergies,
                    //PregnancyStatus = studyQuery.PregnancyStatus,
                    //SmokingStatus = studyQuery.SmokingStatus,

                    StudyId = string.IsNullOrEmpty(studyQuery.RequestedProcedureID) ? UIDHelper.CreateStudyID() : studyQuery.RequestedProcedureID,
                    RequestProcedure = studyQuery.ScheduledProcedureStepID,
                    StudyDate = studyQuery.ScheduledProcedureStepStartDateTime.Date,
                    StudyTime = studyQuery.ScheduledProcedureStepStartDateTime,
                    RegistrationDate = studyQuery.ScheduledProcedureStepStartDateTime,
                    PatientSize = studyQuery.PatientSize==0?null: studyQuery.PatientSize,
                    PatientWeight = studyQuery.PatientWeight==0?null:studyQuery.PatientWeight,
                    PatientSex = patient.PatientSex,
                    Age = ageInfo.Item1,
                    AgeType = ageInfo.Item2,
                    Technician = studyQuery.RequestingPhysician,
                    InstitutionName= studyQuery.InstitutionName,
                    InstitutionAddress = studyQuery.InstitutionAddress

                };

                this._studyService.UpdateWorklistByStudy(patient, study); 

                //同步进度信息
                processingCount++;
                this.SendJobTaskMessage(jobId, this._currentMessageType, JobTaskStatus.Processing, studyQuery.PatientName, processingCount, worklistResult.Length);
            }

            this.SendJobTaskMessage(jobId, this._currentMessageType, JobTaskStatus.Completed, string.Empty, worklistResult.Length, worklistResult.Length);
        }

        private void SendJobTaskMessage(string jobId, MessageType messageType, JobTaskStatus jobTaskStatus, string messageContent, int processedCount, int totalCount)
        {
            //发送消息通知
            var jobTaskMessage = new JobTaskMessage()
            {
                JobId = jobId,
                MessageType = messageType,
                JobStatus = jobTaskStatus,
                Content = string.Empty,
                ProgressedCount = processedCount,
                TotalCount = totalCount,
            };

            MessageInfo messageInfo = new MessageInfo()
            {
                Sender = MessageSource.WorklistJob,
                Level = MessageLevel.Info,
                Content = messageContent,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson()
            };
            this._messageService.SendMessage(messageInfo);
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

        private Gender CovertToGender(PatientSexCS sex)
        {
            if (sex == PatientSexCS.M)
            {
                return Gender.Male;
            }
            else if (sex == PatientSexCS.F)
            {
                return Gender.Female;
            }
            else
            {
                return Gender.Other;
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