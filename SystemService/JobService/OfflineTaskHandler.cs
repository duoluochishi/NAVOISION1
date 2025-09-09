using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.ErrorCodes;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.JobService.Contract;
using NV.CT.MessageService.Contract;
using NV.CT.Protocol;
using System.IO;

namespace NV.CT.JobService;

public class OfflineTaskHandler : IHostedService
{
    private readonly ILogger<OfflineTaskHandler> _logger;
    private readonly IStudyService _studyService;
    private readonly IScanTaskService _scanTaskService;
    private readonly IReconTaskService _reconTaskService;
    private readonly ISeriesService _seriesService;
    private readonly IOfflineTaskService _offlineService;
    private readonly IMessageService _messageService;

    public OfflineTaskHandler(ILogger<OfflineTaskHandler> logger, IOfflineTaskService offlineService, IStudyService studyService, IScanTaskService scanTaskService, IReconTaskService reconTaskService, ISeriesService seriesService, IMessageService messageService)
    {
        _logger = logger;
        _offlineService = offlineService;
        _studyService = studyService;
        _scanTaskService = scanTaskService;
        _reconTaskService = reconTaskService;
        _seriesService = seriesService;
        _messageService = messageService;
    }

    private void OnOfflineTaskService_ErrorOccured(object? sender, CTS.EventArgs<List<string>> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ErrorOccured, event arguments: {JsonConvert.SerializeObject(e.Data)}");
        SendErrorMessage(e.Data.FirstOrDefault());
    }

    private MessageLevel GetLevel(ErrorLevel level) => level switch {
        ErrorLevel.Warning => MessageLevel.Warning,
        ErrorLevel.Error => MessageLevel.Error,
        ErrorLevel.Fatal => MessageLevel.Fatal,
        _ => MessageLevel.Info
    };

    private void OnOfflineTaskService_TaskCreated(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconCreated, event parameters: {JsonConvert.SerializeObject(e.Data)}");
        if (e.Data.IsOfflineRecon)
        {
            _reconTaskService.UpdateReconTaskStatus((e.Data.ScanId, e.Data.ReconId), (e.Data.Status, default(DateTime), default(DateTime)));
        }
        else
        {
            var examInfo = _studyService.GetWithUID(e.Data.StudyUID);
            _reconTaskService.UpdateTaskStatus(examInfo.Study.Id, e.Data.ReconId, e.Data.Status, default(DateTime), default(DateTime));
        }
    }

    private void OnOfflineTaskService_TaskWaiting(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconWaiting, event parameters: {JsonConvert.SerializeObject(e.Data)}");
        if (e.Data.IsOfflineRecon)
        {
            _reconTaskService.UpdateReconTaskStatus((e.Data.ScanId, e.Data.ReconId), (e.Data.Status, default(DateTime), default(DateTime)));
        }
        else
        {
            var examInfo = _studyService.GetWithUID(e.Data.StudyUID);
            _reconTaskService.UpdateTaskStatus(examInfo.Study.Id, e.Data.ReconId, e.Data.Status, default(DateTime), default(DateTime));
        }
    }

    private void OnOfflineTaskService_TaskStarted(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconStarted, event parameters: {JsonConvert.SerializeObject(e.Data)}");
        if (e.Data.IsOfflineRecon)
        {
            _reconTaskService.UpdateReconTaskStatus((e.Data.ScanId, e.Data.ReconId), (e.Data.Status, DateTime.Now, default(DateTime)));
        }
        else
        {
            var examInfo = _studyService.GetWithUID(e.Data.StudyUID);
            _reconTaskService.UpdateTaskStatus(examInfo.Study.Id, e.Data.ReconId, e.Data.Status, default(DateTime), default(DateTime));
        }
    }

    private void OnOfflineTaskService_TaskCanceled(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconCancelled, event parameters: {JsonConvert.SerializeObject(e.Data)}");
        if (e.Data.IsOfflineRecon)
        {
            _reconTaskService.UpdateReconTaskStatus((e.Data.ScanId, e.Data.ReconId), (e.Data.Status, default(DateTime), DateTime.Now));
        }
        else
        {
            var examInfo = _studyService.GetWithUID(e.Data.StudyUID);
            _reconTaskService.UpdateTaskStatus(examInfo.Study.Id, e.Data.ReconId, e.Data.Status, default(DateTime), default(DateTime));
        }
    }

    private void OnOfflineTaskService_TaskAborted(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconAborted, event parameters: {JsonConvert.SerializeObject(e.Data)}");

        if (e.Data.ErrorCodes is not null && e.Data.ErrorCodes.Count != 0)
        {
            SendErrorMessage(e.Data.ErrorCodes.FirstOrDefault());
        }

        if (e.Data.IsOfflineRecon)
        {
            _reconTaskService.UpdateReconTaskStatus((e.Data.ScanId, e.Data.ReconId), (e.Data.Status, default(DateTime), DateTime.Now));
        }
        else
        {
            var examInfo = _studyService.GetWithUID(e.Data.StudyUID);
            _reconTaskService.UpdateTaskStatus(examInfo.Study.Id, e.Data.ReconId, e.Data.Status, default(DateTime), default(DateTime));
        }
    }

    private void SendErrorMessage(string errorCode)
    {
        if (string.IsNullOrEmpty(errorCode)) return;

        var errorInfo = ErrorCodeHelper.GetErrorCode(errorCode);

        if (errorInfo is null) return;

        var level = GetLevel(errorInfo.Level);

        if (level == MessageLevel.Info) return;

        var msgInfo = new MessageInfo {
            Level = level,
            Content = $"{errorInfo.Code}, {errorInfo.Description}, {errorInfo.Reason}",
            Sender = MessageSource.System,
            SendTime = DateTime.Now,
            Remark = errorInfo.Solution,
        };
        _messageService.SendMessage(msgInfo);
    }

    private void OnOfflineTaskService_TaskFinished(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconFinished, event parameters: {JsonConvert.SerializeObject(e.Data)}");
        if (e.Data.IsOfflineRecon)
        {
            _reconTaskService.UpdateReconTaskStatus((e.Data.ScanId, e.Data.ReconId), (e.Data.Status, default(DateTime), DateTime.Now));
        }
        else
        {
            var examInfo = _studyService.GetWithUID(e.Data.StudyUID);
            _reconTaskService.UpdateTaskStatus(examInfo.Study.Id, e.Data.ReconId, e.Data.Status, default(DateTime), default(DateTime));
        }
    }

    private void OnOfflineTaskService_TaskDone(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        _logger.LogInformation($"OfflineTaskHandler.ReconDone(ImageSavedFinished), event parameters: {JsonConvert.SerializeObject(e.Data)}");
        SaveReconSeriesImages(e.Data);
    }

    private void SaveReconSeriesImages(CTS.Models.OfflineTaskInfo offlineTaskInfo)
    {
        //todo: 保护处理
        if (string.IsNullOrEmpty(offlineTaskInfo.ImagePath))
        {
            _logger.LogDebug($"OfflineTaskHandler.ReconDone, the image path of offline recon completed can not empty, (StudyInstanceId : {offlineTaskInfo.StudyUID}, scanId : {offlineTaskInfo.ScanId}, reconId : {offlineTaskInfo.ReconId})");
            return;
        }

        try
        {
            var series = new SeriesModel();
            series.Id = Guid.NewGuid().ToString();
            var imagePath = offlineTaskInfo.ImagePath;
            series.ImageCount = Directory.GetFiles(imagePath, "*.dcm").Length;
            series.SeriesPath = offlineTaskInfo.ImagePath;
            series.ReconId = offlineTaskInfo.ReconId;
            //series.ImageCount = offlineTaskInfo.FinishCount;
            series.ReconEndDate = DateTime.Now;
            series.SeriesType = "image";

            if (offlineTaskInfo.IsOfflineRecon)
            {
                //TODO: MRS和MCS不在一台机器上，图像文件需要拷贝
                var examInfo = _studyService.GetWithUID(offlineTaskInfo.StudyUID);
                var scanTask = _scanTaskService.Get3(examInfo.Study.Id, offlineTaskInfo.ScanId);
                var reconTask = _reconTaskService.Get2(examInfo.Study.Id, offlineTaskInfo.ScanId, offlineTaskInfo.ReconId);

                var studyProtocol = ProtocolHelper.Deserialize(examInfo.Study.Protocol);
                var protocolItems = ProtocolHelper.Expand(studyProtocol);
                var selectedItem = protocolItems.FirstOrDefault(m => m.Scan.Descriptor.Id == offlineTaskInfo.ScanId);
                var reconModel = selectedItem.Scan.Children.FirstOrDefault(recon => recon.Descriptor.Id == offlineTaskInfo.ReconId);

                series.InternalStudyId = examInfo.Study.Id;
                series.BodyPart = $"{selectedItem.Scan.BodyPart}";
                series.ProtocolName = studyProtocol.Descriptor.Name;

                series.SeriesNumber = reconModel.SeriesNumber.ToString();
                series.SeriesInstanceUID = offlineTaskInfo.SeriesUID;
                series.ScanId = offlineTaskInfo.ScanId;

                series.FrameOfReferenceUID = selectedItem.Frame.Descriptor.Id;
                series.PatientPosition = selectedItem.Frame.PatientPosition.ToString();

                series.WindowType = $"{reconModel.WindowType}";
                series.WindowWidth = reconTask.WindowWidth;
                series.WindowLevel = reconTask.WindowLevel;

                series.SeriesDescription = string.IsNullOrEmpty(reconModel.SeriesDescription) ? reconModel.DefaultSeriesDescription : $"{reconModel.SeriesDescription}";
            }
            else
            {
                //todo:待处理
                var examInfo = _studyService.GetWithUID(offlineTaskInfo.StudyUID);
                var reconTask = _reconTaskService.Get2(examInfo.Study.Id, offlineTaskInfo.ScanId, offlineTaskInfo.ReconId);
                series.InternalStudyId = examInfo.Study.Id;
                series.BodyPart = string.Empty;
                series.ProtocolName = string.Empty;
                series.SeriesNumber = offlineTaskInfo.SeriesNumber.ToString();
                series.SeriesInstanceUID = offlineTaskInfo.SeriesUID;
                series.ScanId = string.Empty;
                series.FrameOfReferenceUID = string.Empty;
                series.PatientPosition = PatientPosition.HFS.ToString();
                series.WindowType = "Custom";
                series.WindowWidth = "0";
                series.WindowLevel = "0";
                series.SeriesDescription = offlineTaskInfo.SeriesDescription;
                series.IsDeleted = false;
                series.IsProtected = false;
            }

            _seriesService.Add(series);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"OfflineTaskHandler.ImageSaved(OfflineTaskDone), exception: {ex.Message}");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_offlineService is not null)
        {
            _offlineService.ErrorOccured += OnOfflineTaskService_ErrorOccured;
            _offlineService.TaskCreated += OnOfflineTaskService_TaskCreated;
            _offlineService.TaskWaiting += OnOfflineTaskService_TaskWaiting;
            _offlineService.TaskStarted += OnOfflineTaskService_TaskStarted;
            _offlineService.TaskCanceled += OnOfflineTaskService_TaskCanceled;
            _offlineService.TaskAborted += OnOfflineTaskService_TaskAborted;
            _offlineService.TaskFinished += OnOfflineTaskService_TaskFinished;
            _offlineService.TaskDone += OnOfflineTaskService_TaskDone;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_offlineService is not null)
        {
            _offlineService.ErrorOccured -= OnOfflineTaskService_ErrorOccured;
            _offlineService.TaskCreated -= OnOfflineTaskService_TaskCreated;
            _offlineService.TaskWaiting -= OnOfflineTaskService_TaskWaiting;
            _offlineService.TaskStarted -= OnOfflineTaskService_TaskStarted;
            _offlineService.TaskCanceled -= OnOfflineTaskService_TaskCanceled;
            _offlineService.TaskAborted -= OnOfflineTaskService_TaskAborted;
            _offlineService.TaskFinished -= OnOfflineTaskService_TaskFinished;
            _offlineService.TaskDone -= OnOfflineTaskService_TaskDone;
        }

        return Task.CompletedTask;
    }
}
