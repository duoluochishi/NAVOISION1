//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/6 11:22:50           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.JobService.Contract;

namespace NV.CT.Examination.ApplicationService.Impl;

public class OfflineReconHandler : IHostedService
{
    private readonly ILogger<OfflineReconHandler> _logger;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IOfflineTaskService _offlineService;
    private readonly IStudyHostService _studyHostService;

    public OfflineReconHandler(ILogger<OfflineReconHandler> logger, IOfflineTaskService offlineService, IProtocolHostService protocolHostService, IStudyHostService studyHostService)
    {
        _logger = logger;
        _offlineService = offlineService;
        _protocolHostService = protocolHostService;
        _studyHostService = studyHostService;
    }

    private void OnOfflineService_ErrorOccured(object? sender, CTS.EventArgs<List<string>> e)
    {
        //TODO: 暂不处理
    }

    private void OnOfflineService_TaskCreated(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        //TODO: 暂不处理
    }

    private void OnOfflineService_TaskWaiting(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        if (e.Data.StudyUID != _studyHostService.Instance.StudyInstanceUID) return;

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogInformation($"OfflineReconHandler.ReconWaiting: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }
        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Waiting);
    }

    private void OnOfflineService_TaskStarted(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        if (e.Data.StudyUID != _studyHostService.Instance.StudyInstanceUID) return;

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogInformation($"OfflineReconHandler.ReconStarted: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Performing);
    }

    private void OnOfflineService_TaskCanceled(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        if (e.Data.StudyUID != _studyHostService.Instance.StudyInstanceUID) return;

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogInformation($"OfflineReconHandler.ReconCancelled: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Unperform);
    }

    private void OnOfflineService_TaskAborted(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        if (e.Data.StudyUID != _studyHostService.Instance.StudyInstanceUID) return;

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogInformation($"OfflineReconHandler.ReconAborted: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetParameter(reconModel, ProtocolParameterNames.ERROR_CODE, e.Data.ErrorCodes, false);
        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Performed, FailureReasonType.SystemError);
        UpdateProtocol();
    }

    private void OnOfflineService_ImageProgressChanged(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        //TODO:无需处理
    }

    private void OnOfflineService_TaskFinished(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        //TODO:暂不处理
    }

    private void OnOfflineService_TaskDone(object? sender, CTS.EventArgs<CTS.Models.OfflineTaskInfo> e)
    {
        if (e.Data.StudyUID != _studyHostService.Instance.StudyInstanceUID) return;

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogInformation($"OfflineReconHandler.ReconDone: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_IMAGE_PATH, e.Data.ImagePath);
        //TODO: 待确认，图像传输完成是否认为重建完成，否则需要增加判断
        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Performed);
        UpdateProtocol();
    }

    private void UpdateProtocol()
    {
        _studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_offlineService is not null)
        {
            _offlineService.ErrorOccured += OnOfflineService_ErrorOccured;
            _offlineService.ImageProgressChanged += OnOfflineService_ImageProgressChanged;
            _offlineService.TaskCreated += OnOfflineService_TaskCreated;
            _offlineService.TaskWaiting += OnOfflineService_TaskWaiting;
            _offlineService.TaskStarted += OnOfflineService_TaskStarted;
            _offlineService.TaskCanceled += OnOfflineService_TaskCanceled;
            _offlineService.TaskAborted += OnOfflineService_TaskAborted;
            _offlineService.TaskFinished += OnOfflineService_TaskFinished;
            _offlineService.TaskDone += OnOfflineService_TaskDone;
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_offlineService is not null)
        {
            _offlineService.ErrorOccured -= OnOfflineService_ErrorOccured;
            _offlineService.ImageProgressChanged -= OnOfflineService_ImageProgressChanged;
            _offlineService.TaskCreated -= OnOfflineService_TaskCreated;
            _offlineService.TaskWaiting -= OnOfflineService_TaskWaiting;
            _offlineService.TaskStarted -= OnOfflineService_TaskStarted;
            _offlineService.TaskCanceled -= OnOfflineService_TaskCanceled;
            _offlineService.TaskAborted -= OnOfflineService_TaskAborted;
            _offlineService.TaskFinished -= OnOfflineService_TaskFinished;
            _offlineService.TaskDone -= OnOfflineService_TaskDone;
        }
        return Task.CompletedTask;
    }
}
