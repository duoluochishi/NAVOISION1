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
    public class ArchiveJobProcessor : IJobProcessor
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ArchiveJobProcessor> _logger;
        private readonly IMessageService _messageService;
        private readonly IStudyService _studyService;
        private readonly ISeriesService _seriesService;
        private readonly IJobQueueHandler _jobQueueHandler;
        private readonly MessageType _currentMessageType = MessageType.ArchiveJobResponse;
        private readonly IEchoVerificationHandler _echoVerificationHandler;
        private ConcurrentBag<(JobTaskStatus, string,string)> _processedStatusList; //用于记录本次任务中的各个子项执行结果状态
        private int _totalCountOfItems = 0; //用于记录本次任务中的子项总个数
        private int _processedCount = 0; //用于记录本次任务中当前已处理的子项个数
        private const string MESSAGE_CANCELLED = "Cancelled";
        private C_StoreSCUExecutor _storeScuExecutor = null;
        private string _studyId = string.Empty;
        private string _currentSeriesId = string.Empty;

        public ArchiveJobProcessor(IMapper mapper,
                                   ILogger<ArchiveJobProcessor> logger,
                                   IMessageService messageService,
                                   IStudyService studyService,
                                   ISeriesService seriesService,
                                   IJobQueueHandler jobQueueHandler)
        {
            this._mapper = mapper;
            this._logger = logger;
            this._messageService = messageService;
            this._studyService = studyService;
            this._seriesService = seriesService;
            this._jobQueueHandler = jobQueueHandler;
            this._echoVerificationHandler = new EchoVerificationHandler();

            this._processedStatusList = new ConcurrentBag<(JobTaskStatus, string, string)>();
        }

        public async Task ProcessJobAsync(JobTaskInfo job, CancellationToken cancellationToken)
        {
            var jobParameter = JsonConvert.DeserializeObject<ArchiveJobRequest>(job.Parameter);
            if (jobParameter is null) return;

            _logger.LogTrace($"ArchiveJobProcessor process begins with JobID:{job.Id}");

            // Clear state from previous runs
            _processedStatusList.Clear();
            _totalCountOfItems = jobParameter.SeriesIdList.Count;
            _processedCount = 0;

            // Notify clients that processing has started
            _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Processing, string.Empty, 0, _totalCountOfItems);

            // Use C-Echo to verify SCP is available
            var (isSuccess, errorMessage) = _echoVerificationHandler.VerifyEcho(jobParameter.Host, jobParameter.Port, jobParameter.AECaller, jobParameter.AETitle);
            if (!isSuccess)
            {
                _logger.LogError($"C-Echo verification failed for job {job.Id}: {errorMessage}");
                UpdateArchiveStatus(job.Id, jobParameter.StudyId, jobParameter.SeriesIdList.FirstOrDefault() ?? string.Empty, JobTaskStatus.Failed);
                _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Failed, errorMessage, _processedCount, _totalCountOfItems);
                return;
            }

            try
            {
                await ProcessAllSeriesAsync(jobParameter, cancellationToken);

                // Determine final job status
                var finalStatus = CalculateFinalJobStatus(job.Id);
                _jobQueueHandler.UpdateTaskStatusByJobId(job.Id, finalStatus.status);
                CheckAndUpdateStudyArchiveStatus(jobParameter.StudyId, _currentSeriesId);
                _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.ArchiveJob, finalStatus.status, finalStatus.errorMessage, _processedStatusList.Count(s => s.Item1 == JobTaskStatus.Completed), _totalCountOfItems);

                _logger.LogTrace($"ArchiveJobProcessor finished JobID:{job.Id} with status: {finalStatus.status}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Archive job {job.Id} was canceled.");
                UpdateArchiveStatus(job.Id, jobParameter.StudyId, _currentSeriesId, JobTaskStatus.Cancelled);
                _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Cancelled, MESSAGE_CANCELLED, _processedCount, _totalCountOfItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while processing archive job {job.Id}.");
                UpdateArchiveStatus(job.Id, jobParameter.StudyId, _currentSeriesId, JobTaskStatus.Failed);
                _jobQueueHandler.SendJobTaskMessage(job.Id, _currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Failed, ex.Message, _processedCount, _totalCountOfItems);
            }
        }

        private async Task ProcessAllSeriesAsync(ArchiveJobRequest jobParameter, CancellationToken cancellationToken)
        {
            _studyId = jobParameter.StudyId;
            UpdateArchiveStatusOfStudy(_studyId, JobTaskStatus.Processing);

            foreach (var seriesId in jobParameter.SeriesIdList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentSeriesId = seriesId;

                var seriesModel = _seriesService.GetSeriesById(seriesId);
                UpdateArchiveStatusOfSeries(seriesId, JobTaskStatus.Processing);

                if (!Enum.TryParse<SupportedTransferSyntax>(jobParameter.DicomTransferSyntax, true, out var dicomTransferSyntaxType))
                {
                    _logger.LogWarning($"Failed to parse {jobParameter.DicomTransferSyntax}, defaulting to ImplicitVRLittleEndian.");
                    dicomTransferSyntaxType = SupportedTransferSyntax.ImplicitVRLittleEndian;
                }

                using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    _storeScuExecutor = new C_StoreSCUExecutor(_logger, tokenSource, jobParameter.Id, seriesId, seriesModel.SeriesPath,
                        jobParameter.Host, jobParameter.Port, jobParameter.AECaller, jobParameter.AETitle,
                        FellowOakDicom.Network.DicomPriority.High, dicomTransferSyntaxType, jobParameter.UseTls, jobParameter.Anonymous);

                    _storeScuExecutor.ExecuteStatusInfoChanged += OnExecuteStatusChanged;
                    await _storeScuExecutor.StartAnsyc();
                    _storeScuExecutor.ExecuteStatusInfoChanged -= OnExecuteStatusChanged;
                }

                _processedCount++;
                _jobQueueHandler.SendJobTaskMessage(jobParameter.Id, _currentMessageType, MessageSource.ArchiveJob, JobTaskStatus.Processing, string.Empty, _processedCount, _totalCountOfItems);
            }
        }

        private (JobTaskStatus status, string errorMessage) CalculateFinalJobStatus(string jobId)
        {
            var jobResults = _processedStatusList.Where(r => r.Item3 == jobId).ToList();
            var completedCount = jobResults.Count(s => s.Item1 == JobTaskStatus.Completed);
            var failedCount = jobResults.Count(s => s.Item1 == JobTaskStatus.Failed);
            string errorMessage = string.Empty;

            if (completedCount == _totalCountOfItems)
            {
                return (JobTaskStatus.Completed, string.Empty);
            }
            if (completedCount > 0)
            {
                errorMessage = failedCount > 0 ? jobResults.First(s => s.Item1 == JobTaskStatus.Failed).Item2 : string.Empty;
                return (JobTaskStatus.PartlyCompleted, errorMessage);
            }

            errorMessage = failedCount > 0 ? jobResults.First(s => s.Item1 == JobTaskStatus.Failed).Item2 : "All series failed";
            return (JobTaskStatus.Failed, errorMessage);
        }

        private void OnExecuteStatusChanged(object? sender, ExecuteStatusInfo e)
        {
            if (string.IsNullOrEmpty(e.SeriesID)) return;

            if (e.Status == ExecuteStatus.Succeeded)
            {
                _processedStatusList.Add((JobTaskStatus.Completed, string.Empty, e.JobTaskID));
                UpdateArchiveStatusOfSeries(e.SeriesID, JobTaskStatus.Completed);
            }
            else if (e.Status == ExecuteStatus.Failed)
            {
                _processedStatusList.Add((JobTaskStatus.Failed, e.Tips, e.JobTaskID));
                UpdateArchiveStatusOfSeries(e.SeriesID, JobTaskStatus.Failed);
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
            if (!string.IsNullOrEmpty(seriesId))
            {
                UpdateArchiveStatusOfSeries(seriesId, jobTaskStatus);
            }
        }
    }
}