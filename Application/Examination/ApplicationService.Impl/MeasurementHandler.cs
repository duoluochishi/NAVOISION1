//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/16 8:40:13     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace NV.CT.Examination.ApplicationService.Impl;

public class MeasurementHandler : IHostedService
{
    private readonly ILogger<MeasurementHandler> _logger;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IMeasurementStatusService _measurementStatusService;

    public MeasurementHandler(ILogger<MeasurementHandler> logger, IProtocolHostService protocolHostService, IMeasurementStatusService measurementStatusService)
    {
        _logger = logger;
        _measurementStatusService = measurementStatusService;
        _protocolHostService = protocolHostService;
    }

    private void MeasurementStatusService_MeasurementLoadingFailed(object? sender, CTS.EventArgs<string> e)
    {
        _logger.LogInformation($"ScanControlHandler.MeasurementLoaded: {e.Data}");

        var item = _protocolHostService.Models.FirstOrDefault(m => m.Measurement.Descriptor.Id == e.Data);
        if (item.Measurement is null)
        {
            _logger.LogInformation($"ScanControlHandler.MeasurementLoaded: Measurement ({0}) is not exist.", e.Data);
            return;
        }
        foreach (var scanItem in item.Measurement.Children)
        {
            _protocolHostService.SetPerformStatus(scanItem, PerformStatus.Unperform);
        }
        _protocolHostService.SetPerformStatus(item.Measurement, PerformStatus.Unperform);
        _protocolHostService.SetPerformStatus(item.Frame, PerformStatus.Unperform);
    }

    private void MeasurementStatusService_MeasurementLoaded(object? sender, CTS.EventArgs<string> e)
    {
        _logger.LogInformation($"ScanControlHandler.MeasurementLoaded: {e.Data}");

        var item = _protocolHostService.Models.FirstOrDefault(m => m.Measurement.Descriptor.Id == e.Data);
        if (item.Measurement is null)
        {
            _logger.LogInformation($"ScanControlHandler.MeasurementLoaded: Measurement ({0}) is not exist.", e.Data);
            return;
        }
        foreach (var scanItem in item.Measurement.Children)
        {
            _protocolHostService.SetPerformStatus(scanItem, PerformStatus.Waiting);
        }
        _protocolHostService.SetPerformStatus(item.Measurement, PerformStatus.Performing);
        _protocolHostService.SetPerformStatus(item.Frame, PerformStatus.Performing);
    }

    private void MeasurementStatusService_MeasurementAborted(object? sender, CTS.EventArgs<(string MeasurementId, bool ModelType, string ScanId, string ReconId, FailureReasonType ReasonType)> e)
    {
        _logger.LogInformation($"ScanControlService.MeasurementAborted: {e.Data}");

        var item = _protocolHostService.Models.FirstOrDefault(m => m.Measurement.Descriptor.Id == e.Data.MeasurementId);
        if (item.Measurement is null)
        {
            _logger.LogInformation($"ScanControlService.MeasurementAborted: Measurement ({e.Data.MeasurementId}) is not exist.");
            return;
        }
        //TODO: 待处理不存在 或 拆分等
        _protocolHostService.SetPerformStatus(item.Measurement, PerformStatus.Performed, e.Data.ReasonType);
    }

    private void MeasurementStatusService_MeasurementCanceled(object? sender, CTS.EventArgs<string> e)
    {
        _logger.LogInformation($"ScanControlService.MeasurementCanceled: {e.Data}");

        var currentMeasurement = _protocolHostService.Models.Where(s => s.Measurement.Descriptor.Id == e.Data).Select(m => m.Measurement).FirstOrDefault();
        if (currentMeasurement is null)
        {
            _logger.LogInformation($"ScanControlService.MeasurementCanceled: Measurement ({e.Data}) is not exist.");
            return;
        }
        _protocolHostService.SetPerformStatus(currentMeasurement, PerformStatus.Unperform);
    }

    private void MeasurementStatusService_MeasurementDone(object? sender, CTS.EventArgs<string> e)
    {
        _logger.LogInformation($"ScanControlService.MeasurementDone: {e.Data}");

        var item = _protocolHostService.Models.FirstOrDefault(m => m.Measurement.Descriptor.Id == e.Data);

        if (item.Measurement is null)
        {
            _logger.LogInformation($"ScanControlService.MeasurementDone: Measurement ({e.Data}) is not exist.");
            return;
        }

        //TODO: 仅所有扫描都完成，才可以
        if (item.Measurement.Children.All(s => s.Status == PerformStatus.Performed))
        {
            var isCancelled = item.Measurement.Children.Any(s => s.FailureReason == FailureReasonType.UserCancellation);

            _protocolHostService.SetPerformStatus(item.Measurement, PerformStatus.Performed, isCancelled ? FailureReasonType.UserCancellation : FailureReasonType.None);

            if (item.Frame.Children.All(m => m.Status == PerformStatus.Performed))
            {
                _protocolHostService.SetPerformStatus(item.Frame, PerformStatus.Performed);
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_measurementStatusService is not null)
        {
            _measurementStatusService.MeasurementAborted += MeasurementStatusService_MeasurementAborted;
            _measurementStatusService.MeasurementCanceled += MeasurementStatusService_MeasurementCanceled;
            _measurementStatusService.MeasurementDone += MeasurementStatusService_MeasurementDone;
            _measurementStatusService.MeasurementLoadingFailed += MeasurementStatusService_MeasurementLoadingFailed;
            _measurementStatusService.MeasurementLoaded += MeasurementStatusService_MeasurementLoaded;
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_measurementStatusService is not null)
        {
            _measurementStatusService.MeasurementAborted -= MeasurementStatusService_MeasurementAborted;
            _measurementStatusService.MeasurementCanceled -= MeasurementStatusService_MeasurementCanceled;
            _measurementStatusService.MeasurementDone -= MeasurementStatusService_MeasurementDone;
            _measurementStatusService.MeasurementLoadingFailed -= MeasurementStatusService_MeasurementLoadingFailed;
            _measurementStatusService.MeasurementLoaded -= MeasurementStatusService_MeasurementLoaded;
        }
        return Task.CompletedTask;
    }
}