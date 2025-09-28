using AutoMapper;
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace NV.CT.JobService
{
    public class ImportJobProcessor : IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ImportJobProcessor> _logger;
        private readonly IMessageService _messageService;
        private readonly IStudyService _studyService;
        private readonly ISeriesService _seriesService;
        private readonly IJobTaskService _jobTaskService;
        private readonly IRawDataService _rawDataService;
        private readonly IPatientService _patientService;
        private readonly IScanTaskService _scanTaskService;
        private readonly IReconTaskService _reconTaskService;
        private readonly MessageType _currentMessageType = MessageType.ImportJobResponse;
        private int _totalCountOfItems = 0;
        private int _processedCount = 0;
        private const string PATIENT_SEX_M = "M";
        private const string PATIENT_SEX_F = "F";
        private const string PID_PREFIX = "PID";

        public ImportJobProcessor(
            IMapper mapper,
            ILogger<ImportJobProcessor> logger,
            IMessageService messageService,
            IStudyService studyService,
            ISeriesService seriesService,
            IJobTaskService jobTaskService,
            IRawDataService rawDataService,
            IPatientService patientService,
            IScanTaskService scanTaskService,
            IReconTaskService reconTaskService)
        {
            _mapper = mapper;
            _logger = logger;
            _messageService = messageService;
            _studyService = studyService;
            _seriesService = seriesService;
            _jobTaskService = jobTaskService;
            _rawDataService = rawDataService;
            _patientService = patientService;
            _scanTaskService = scanTaskService;
            _reconTaskService = reconTaskService;
        }

        public async Task ProcessJobAsync(JobTaskInfo job, CancellationToken cancellationToken)
        {
            var jobParameter = JsonConvert.DeserializeObject<ImportJobRequest>(job.Parameter);
            if (jobParameter == null)
            {
                _logger.LogWarning($"Could not deserialize job parameter for job {job.Id}");
                return;
            }

            _logger.LogTrace($"ImportJobProcessor starting for job {job.Id}");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IImportTaskExecutor importExecutor = CreateExecutor(job, jobParameter, cts);

            if (importExecutor == null)
            {
                _logger.LogError($"Could not create an executor for job type {job.JobType} on job {job.Id}");
                UpdateJobTaskStatus(job.Id, JobTaskStatus.Failed);
                return;
            }

            try
            {
                importExecutor.ExecuteStatusChanged += (sender, e) => OnExecuteStatusChanged(sender, e, cts.Token);
                await Task.Run(() => importExecutor.Start(), cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Import job {job.Id} was canceled.");
                UpdateJobTaskStatus(job.Id, JobTaskStatus.Cancelled);
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Cancelled, "Canceled", string.Empty, _processedCount, _totalCountOfItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during import job {job.Id}.");
                UpdateJobTaskStatus(job.Id, JobTaskStatus.Failed);
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Failed, "Error", ex.Message, _processedCount, _totalCountOfItems);
            }
            finally
            {
                importExecutor.ExecuteStatusChanged -= (sender, e) => OnExecuteStatusChanged(sender, e, cts.Token);
            }
        }

        private IImportTaskExecutor CreateExecutor(JobTaskInfo job, ImportJobRequest jobParameter, CancellationTokenSource cts)
        {
            string targetRootPath = Path.Combine(RuntimeConfig.Console.MCSAppData.Path);
            if (jobParameter.IsRawDataImport)
            {
                return new ImportByRawDataExecutor(job.Id, jobParameter.SourcePath, targetRootPath, _logger, cts, _rawDataService, _studyService, _patientService, _scanTaskService, _reconTaskService, _seriesService, _mapper);
            }
            else
            {
                return new ImportByDirExecutor(job.Id, jobParameter.SourcePath, targetRootPath, _logger, cts);
            }
        }

        private void OnExecuteStatusChanged(object? sender, ExecuteStatusInfo e, CancellationToken cancellationToken)
        {
            _processedCount = e.ProcessedCount;
            _totalCountOfItems = e.TotalCount;

            JobTaskStatus status = JobTaskStatus.Unknown;
            switch (e.Status)
            {
                case ExecuteStatus.Started:
                case ExecuteStatus.InProgress:
                    status = JobTaskStatus.Processing;
                    break;
                case ExecuteStatus.Succeeded:
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (e.Data is List<DicomPatientInfo> dicomInfo)
                        {
                            SaveDicomInfo(dicomInfo, cancellationToken);
                        }
                        status = JobTaskStatus.Completed;
                    }
                    catch (OperationCanceledException)
                    {
                        status = JobTaskStatus.Cancelled;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to save DICOM info for job {e.JobTaskID}");
                        status = JobTaskStatus.Failed;
                        e.Tips = ex.Message;
                    }
                    break;
                case ExecuteStatus.Failed:
                    status = JobTaskStatus.Failed;
                    break;
                case ExecuteStatus.Cancelled:
                    status = JobTaskStatus.Cancelled;
                    break;
            }

            if (status != JobTaskStatus.Unknown)
            {
                UpdateJobTaskStatus(e.JobTaskID, status);
                string errorMessage = (status == JobTaskStatus.Failed) ? e.Data?.ToString() ?? e.Tips : string.Empty;
                SendJobTaskMessage(e.JobTaskID, _currentMessageType, status, e.Tips, errorMessage, e.ProcessedCount, e.TotalCount);
            }
        }

        private void UpdateJobTaskStatus(string jobId, JobTaskStatus jobTaskStatus)
        {
            _jobTaskService.UpdateTaskStatusByJobId(jobId, jobTaskStatus.ToString());
        }

        private void SendJobTaskMessage(string jobId, MessageType messageType, JobTaskStatus jobTaskStatus, string messageContent, string errorMessage, int processedCount, int totalCount)
        {
            var jobTaskMessage = new JobTaskMessage
            {
                JobId = jobId,
                MessageType = messageType,
                JobStatus = jobTaskStatus,
                Content = messageContent,
                ProgressedCount = processedCount,
                TotalCount = totalCount,
            };
            var messageInfo = new MessageInfo
            {
                Sender = MessageSource.ImportJob,
                Level = MessageLevel.Info,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson(),
            };

            string message = jobTaskStatus switch
            {
                JobTaskStatus.Processing when processedCount == 0 => $"{LanguageResource.Content_ImportingFor} [{messageContent}]",
                JobTaskStatus.Completed => $"{LanguageResource.Content_Importing_DoneFor} [{messageContent}]",
                JobTaskStatus.Cancelled => $"{LanguageResource.Content_Canceled_ImportingFor} [{messageContent}]",
                JobTaskStatus.Failed => $"{LanguageResource.Content_FailedToImportFor} [{errorMessage}]",
                _ => messageContent
            };

            messageInfo.Content = message;
            _messageService.SendMessage(messageInfo);
        }

        private void SaveDicomInfo(List<DicomPatientInfo> dicomPatientList, CancellationToken cancellationToken)
        {
            var patients = new List<DatabaseService.Contract.Models.PatientModel>();
            var studies = new List<DatabaseService.Contract.Models.StudyModel>();
            var serieses = new List<DatabaseService.Contract.Models.SeriesModel>();

            foreach (var item in dicomPatientList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var patient = new DatabaseService.Contract.Models.PatientModel
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = string.IsNullOrEmpty(item.PatientId) ? $"{PID_PREFIX}_{IdGenerator.NextRandomID()}" : item.PatientId,
                    PatientName = item.PatientName,
                    CreateTime = DateTime.Now,
                    PatientBirthDate = item.PatientBirthDateTime ?? DateTime.MinValue,
                    PatientSex = item.PatientSex switch
                    {
                        PATIENT_SEX_M => Gender.Male,
                        PATIENT_SEX_F => Gender.Female,
                        _ => Gender.Other
                    }
                };
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
                        BodyPart = item2.SeriesList.Count > 0 ? item2.SeriesList[0].BodyPartExamined : string.Empty,
                        StudyStatus = WorkflowStatus.ExaminationClosed.ToString(),
                        StudyInstanceUID = item2.StudyInstanceUID,
                        Ward = item.CurrentPatientLocation,
                        PatientType = (int)PatientType.Local,
                        InstitutionAddress = item2.PatientAddress,
                        Comments = item2.StudyDescription,
                        StudyDate = item2.StudyDateTime ?? DateTime.MinValue,
                        StudyTime = item2.StudyDateTime ?? DateTime.MinValue,
                    };

                    if (double.TryParse(item.PatientSize, out var size)) study.PatientSize = size;
                    if (double.TryParse(item.PatientWeight, out var weight)) study.PatientWeight = weight;

                    if (item.PatientBirthDateTime == null || item.PatientBirthDateTime.Value == DateTime.MinValue)
                    {
                        study.Age = 0;
                        study.AgeType = AgeType.Year;
                    }
                    else
                    {
                        TimeSpan span = DateTime.Now.Subtract(item.PatientBirthDateTime.Value);
                        int diff = span.Days;
                        if (diff >= 365) { study.Age = (diff / 365); study.AgeType = AgeType.Year; }
                        else if (diff >= 30) { study.Age = (diff / 30); study.AgeType = AgeType.Month; }
                        else if (diff >= 7) { study.Age = (diff / 7); study.AgeType = AgeType.Week; }
                        else { study.Age = diff; study.AgeType = AgeType.Day; }
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
                            ReconEndDate = item3.SeriesDateTime ?? DateTime.MinValue,
                            SeriesDescription = item3.SeriesDescription,
                            ImageCount = item3.ImageList.Count,
                            PatientPosition = item3.PatientPosition,
                            BodyPart = item3.BodyPartExamined,
                            SeriesType = Constants.SERIES_TYPE_IMAGE,
                            ImageType = item3.ImageType,
                            SeriesNumber = item3.SeriesNumber?.ToString() ?? string.Empty,
                            WindowWidth = item3.WindowWidth,
                            WindowLevel = item3.WindowLevel,
                        };

                        if (item3.ImageType == Constants.SERIES_TYPE_SR || item3.ImageType == Constants.SERIES_TYPE_DOSE_REPORT)
                        {
                            series.SeriesType = item3.ImageType;
                            series.BodyPart = study.BodyPart;
                            series.ImageCount = item3.ImageType == Constants.SERIES_TYPE_DOSE_REPORT ? 1 : 0;
                        }

                        var imageModel = item3.ImageList.FirstOrDefault(s => s.Path != null);
                        if (imageModel != null)
                        {
                            series.SeriesPath = Path.GetDirectoryName(imageModel.Path);
                        }
                        serieses.Add(series);
                    }
                    studies.Add(study);
                }
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _studyService.InsertPatientListStudyListAndSeriesList(patients, studies, serieses);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[ImportJobProcessor][SaveDicomInfo]:Failed to execute InsertPatientListStudyListAndSeriesList for patients [{string.Join(", ", patients.Select(p => p.PatientName))}]");
                throw;
            }
        }
    }
}