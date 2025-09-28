using AutoMapper;
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
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.JobService
{
    public class ExportJobProcessor : IJobProcessor
    {
        private readonly ILogger<ExportJobProcessor> _logger;
        private readonly IMessageService _messageService;
        private readonly IJobTaskService _jobTaskService;
        private readonly IRawDataService _rawDataService;
        private readonly IStudyService _studyService;
        private readonly IPatientService _patientService;
        private readonly IScanTaskService _scanTaskService;
        private readonly IReconTaskService _reconTaskService;
        private readonly ISeriesService _seriesService;
        private readonly MessageType _currentMessageType = MessageType.ExportJobResponse;
        private int _totalCountOfItems = 0;
        private int _processedCount = 0;

        public ExportJobProcessor(
            ILogger<ExportJobProcessor> logger,
            IMessageService messageService,
            IJobTaskService jobTaskService,
            IRawDataService rawDataService,
            IStudyService studyService,
            IPatientService patientService,
            IScanTaskService scanTaskService,
            IReconTaskService reconTaskService,
            ISeriesService seriesService)
        {
            _logger = logger;
            _messageService = messageService;
            _jobTaskService = jobTaskService;
            _rawDataService = rawDataService;
            _studyService = studyService;
            _patientService = patientService;
            _scanTaskService = scanTaskService;
            _reconTaskService = reconTaskService;
            _seriesService = seriesService;
        }

        public async Task ProcessJobAsync(JobTaskInfo job, CancellationToken cancellationToken)
        {
            var jobParameter = JsonConvert.DeserializeObject<ExportJobRequest>(job.Parameter);
            if (jobParameter == null)
            {
                _logger.LogWarning($"Could not deserialize job parameter for job {job.Id}");
                return;
            }

            _logger.LogTrace($"ExportJobProcessor starting for job {job.Id}");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ITaskExecutor exportTaskExecutor = CreateExecutor(job, jobParameter, cts);

            if (exportTaskExecutor == null)
            {
                _logger.LogError($"Could not create an executor for job type {job.JobType} on job {job.Id}");
                UpdateJobTaskStatus(job.Id, JobTaskStatus.Failed);
                return;
            }

            try
            {
                exportTaskExecutor.ExecuteStatusChanged += OnExecuteStatusChanged;
                await Task.Run(() => exportTaskExecutor.Start(), cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Export job {job.Id} was canceled.");
                UpdateJobTaskStatus(job.Id, JobTaskStatus.Cancelled);
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Cancelled, "Canceled", string.Empty, _processedCount, _totalCountOfItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during export job {job.Id}.");
                UpdateJobTaskStatus(job.Id, JobTaskStatus.Failed);
                SendJobTaskMessage(job.Id, _currentMessageType, JobTaskStatus.Failed, "Error", ex.Message, _processedCount, _totalCountOfItems);
            }
            finally
            {
                exportTaskExecutor.ExecuteStatusChanged -= OnExecuteStatusChanged;
            }
        }

        private ITaskExecutor CreateExecutor(JobTaskInfo job, ExportJobRequest jobParameter, CancellationTokenSource cts)
        {
            string[] sourcePaths = jobParameter.InputFolders.ToArray();
            string targetRootPath = jobParameter.OutputFolder;
            string binPath = RuntimeConfig.Console.MCSBin.Path;
            string[] patientNames = jobParameter.PatientNames.ToArray();
            bool isAnonymouse = jobParameter.IsAnonymouse;
            bool isCorrected = jobParameter.IsCorrected;
            bool isBurnToCDROM = jobParameter.IsBurnToCDROM;
            bool isAddViewer = jobParameter.IsAddViewer;
            string[] rawDataIDList = jobParameter.SeriesIdList.ToArray();
            string[] rtdDicomList = jobParameter.RTDDicomFolders.ToArray();

            Enum.TryParse<SupportedTransferSyntax>(jobParameter.DicomTransferSyntax, false, out var dicomTransferSyntax);

            if (jobParameter.IsExportedToDICOM)
            {
                return new ExportToDicomExecutor(_logger, cts, job.Id, patientNames, sourcePaths, targetRootPath, binPath, isAnonymouse, isCorrected, isBurnToCDROM, isAddViewer, dicomTransferSyntax);
            }
            if (jobParameter.IsExportedToImage)
            {
                var imageFormatType = ConvertToImageFormat(jobParameter.PictureType ?? FileExtensionType.Png);
                return new ExportToImageExecutor(_logger, cts, job.Id, patientNames, sourcePaths, targetRootPath, binPath, imageFormatType, isBurnToCDROM, dicomTransferSyntax);
            }
            if (jobParameter.IsExportedToRawData)
            {
                return new ExportToRawDataExecutor(_logger, cts, job.Id, patientNames, sourcePaths, rawDataIDList, rtdDicomList, targetRootPath, _rawDataService, _studyService, _patientService, _scanTaskService, _reconTaskService, _seriesService, jobParameter.StudyId);
            }

            return null;
        }

        private void OnExecuteStatusChanged(object? sender, ExecuteStatusInfo e)
        {
            _processedCount = e.ProcessedCount;
            _totalCountOfItems = e.TotalCount;

            JobTaskStatus status = e.Status switch
            {
                ExecuteStatus.Started => JobTaskStatus.Processing,
                ExecuteStatus.InProgress => JobTaskStatus.Processing,
                ExecuteStatus.Succeeded => JobTaskStatus.Completed,
                ExecuteStatus.Failed => JobTaskStatus.Failed,
                ExecuteStatus.Cancelled => JobTaskStatus.Cancelled,
                _ => JobTaskStatus.Unknown
            };

            if (status != JobTaskStatus.Unknown)
            {
                UpdateJobTaskStatus(e.JobTaskID, status);
                string errorMessage = e.Data?.ToString() ?? string.Empty;
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
                Sender = MessageSource.ExportJob,
                Level = MessageLevel.Info,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson(),
            };

            string message = jobTaskStatus switch
            {
                JobTaskStatus.Processing when processedCount == 0 => $"{LanguageResource.Content_ExportingFor}  [{messageContent}]",
                JobTaskStatus.Processing => $"{LanguageResource.Content_ExportingFor} [{messageContent}]",
                JobTaskStatus.Completed => $"{LanguageResource.Content_ExportingDoneFor} [{messageContent}]",
                JobTaskStatus.Cancelled => $"{LanguageResource.Content_CanceledExportingFor} [{messageContent}]",
                JobTaskStatus.Failed => $"{LanguageResource.Content_FailedToExportingFor} [{errorMessage}]",
                _ => $"{LanguageResource.Content_Exporting}"
            };

            messageInfo.Content = message;
            _messageService.SendMessage(messageInfo);
        }

        private ImageFormat ConvertToImageFormat(FileExtensionType fileExtensionType)
        {
            return fileExtensionType switch
            {
                FileExtensionType.Bmp => ImageFormat.Bmp,
                FileExtensionType.Gif => ImageFormat.Gif,
                FileExtensionType.Jpeg => ImageFormat.Jpeg,
                FileExtensionType.Png => ImageFormat.Png,
                _ => ImageFormat.Png,
            };
        }
    }
}
