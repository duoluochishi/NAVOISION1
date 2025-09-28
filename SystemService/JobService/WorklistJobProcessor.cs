using AutoMapper;
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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.JobService
{
    public class WorklistJobProcessor : IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<WorklistJobProcessor> _logger;
        private readonly IMessageService _messageService;
        private readonly IPatientService _patientService;
        private readonly IStudyService _studyService;
        private readonly IEchoVerificationHandler _echoVerificationHandler;
        private readonly MessageType _currentMessageType = MessageType.WorklistJobResponse;
        private const string MESSAGE_CANCELLED = "Cancelled";

        public WorklistJobProcessor(
            IMapper mapper,
            ILogger<WorklistJobProcessor> logger,
            IMessageService messageService,
            IStudyService studyService,
            IPatientService patientService)
        {
            _mapper = mapper;
            _logger = logger;
            _messageService = messageService;
            _studyService = studyService;
            _patientService = patientService;
            _echoVerificationHandler = new EchoVerificationHandler();
        }

        public async Task ProcessJobAsync(JobTaskInfo job, CancellationToken cancellationToken)
        {
            var jobParameter = JsonConvert.DeserializeObject<WorklistJobRequest>(job.Parameter);
            if (jobParameter == null)
            {
                _logger.LogWarning($"Could not deserialize job parameter for job {job.Id}");
                return;
            }

            _logger.LogTrace($"WorklistJobProcessor starting for job {job.Id}");

            var (isSuccess, errorMessage) = _echoVerificationHandler.VerifyEcho(jobParameter.Host, jobParameter.Port, jobParameter.AECaller, jobParameter.AETitle);
            if (!isSuccess)
            {
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Failed, errorMessage, 1, 1);
                _logger.LogWarning($"Worklist C-Echo failed for job {job.Id}: {errorMessage}");
                return;
            }

            SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Processing, string.Empty, 0, 1);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dicomNode = new DicomNode(jobParameter.Host, jobParameter.Port, jobParameter.AECaller, jobParameter.AETitle);
                var worklistFilter = new WorklistFilter(job.Id, jobParameter.StudyDateStart, jobParameter.StudyDateEnd);
                var executor = new ModalityWorklistSCUExecutor();

                var queryResults = await executor.QueryAsync(dicomNode, worklistFilter);

                cancellationToken.ThrowIfCancellationRequested();
                UpdateStudyQueryResults(job.Id, queryResults, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Worklist job {job.Id} was canceled.");
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Cancelled, MESSAGE_CANCELLED, 1, 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during worklist job {job.Id}.");
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Failed, ex.Message, 1, 1);
            }
        }

        private void UpdateStudyQueryResults(string jobId, WorklistResult[] worklistResult, CancellationToken cancellationToken)
        {
            if (worklistResult == null || worklistResult.Length == 0)
            {
                SendJobTaskMessage(jobId, _currentMessageType, JobTaskStatus.Completed, "No results found", 0, 0);
                return;
            }

            int processingCount = 0;
            foreach (var studyQuery in worklistResult)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var patient = new DatabaseService.Contract.Models.PatientModel
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = studyQuery.PatientID,
                    PatientName = studyQuery.PatientName.Trim(),
                    PatientSex = CovertToGender(studyQuery.PatientSex),
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
                    StudyId = string.IsNullOrEmpty(studyQuery.RequestedProcedureID) ? UIDHelper.CreateStudyID() : studyQuery.RequestedProcedureID,
                    RequestProcedure = studyQuery.ScheduledProcedureStepID,
                    StudyDate = studyQuery.ScheduledProcedureStepStartDateTime.Date,
                    StudyTime = studyQuery.ScheduledProcedureStepStartDateTime,
                    RegistrationDate = studyQuery.ScheduledProcedureStepStartDateTime,
                    PatientSize = studyQuery.PatientSize == 0 ? null : studyQuery.PatientSize,
                    PatientWeight = studyQuery.PatientWeight == 0 ? null : studyQuery.PatientWeight,
                    PatientSex = patient.PatientSex,
                    Age = ageInfo.Item1,
                    AgeType = ageInfo.Item2,
                    Technician = studyQuery.RequestingPhysician,
                    InstitutionName = studyQuery.InstitutionName,
                    InstitutionAddress = studyQuery.InstitutionAddress
                };

                _studyService.UpdateWorklistByStudy(patient, study);

                processingCount++;
                SendJobTaskMessage(jobId, _currentMessageType, JobTaskStatus.Processing, studyQuery.PatientName, processingCount, worklistResult.Length);
            }

            SendJobTaskMessage(jobId, _currentMessageType, JobTaskStatus.Completed, string.Empty, worklistResult.Length, worklistResult.Length);
        }

        private void SendJobTaskMessage(string jobId, MessageType messageType, JobTaskStatus jobTaskStatus, string messageContent, int processedCount, int totalCount)
        {
            var jobTaskMessage = new JobTaskMessage
            {
                JobId = jobId,
                MessageType = messageType,
                JobStatus = jobTaskStatus,
                Content = string.Empty,
                ProgressedCount = processedCount,
                TotalCount = totalCount,
            };

            var messageInfo = new MessageInfo
            {
                Sender = MessageSource.WorklistJob,
                Level = MessageLevel.Info,
                Content = messageContent,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson()
            };
            _messageService.SendMessage(messageInfo);
        }

        private Gender CovertToGender(PatientSexCS sex)
        {
            return sex switch
            {
                PatientSexCS.M => Gender.Male,
                PatientSexCS.F => Gender.Female,
                _ => Gender.Other,
            };
        }
    }
}