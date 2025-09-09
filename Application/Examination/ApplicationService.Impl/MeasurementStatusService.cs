//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/26 9:07:57     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS;

namespace NV.CT.Examination.ApplicationService.Impl;

public class MeasurementStatusService : IMeasurementStatusService
{
    private readonly ILogger<MeasurementStatusService> _logger;
    private readonly IProtocolHostService _protocolHostService;

    public MeasurementStatusService(ILogger<MeasurementStatusService> logger, IProtocolHostService protocolHostService)
    {
        _logger = logger;
        _protocolHostService = protocolHostService;
    }

    public void RaiseMeasurementLoadingFailed(string measurementId)
    {
        MeasurementLoadingFailed?.Invoke(this, new EventArgs<string>(measurementId));
    }

    public void RaiseMeasurementLoaded(string measurementId)
    {
        MeasurementLoaded?.Invoke(this, new EventArgs<string>(measurementId));
    }

    public void RaiseMeasurementDone(string measurementId)
    {
        var items = _protocolHostService.Models.Where(s => s.Measurement.Descriptor.Id == measurementId).ToList();
        var isDone = true;
        foreach(var item in items)
        {
            if (item.Scan.Status != PerformStatus.Performed)
            {
                isDone = false;
                break;
            }

            if (item.Scan.Children.Any(r => r.IsRTD && r.Status != PerformStatus.Performed))
            {
                isDone = false;
                break;
            }
        }
        if (isDone)
        {
            MeasurementDone?.Invoke(this, new EventArgs<string>(measurementId));
        }
    }

    public void RasiseMeasurementAborted(string measurementId, bool modelType, string scanId, string reconId, FailureReasonType reasonType)
    {
        MeasurementAborted?.Invoke(this, new EventArgs<(string, bool, string, string, FailureReasonType)>((measurementId, modelType, scanId, reconId, reasonType)));
    }

    public void RaiseMeasurementCancelled(string measurementId)
    {
        MeasurementCanceled?.Invoke(this, new EventArgs<string>(measurementId));
    }

    public event EventHandler<EventArgs<string>>? MeasurementLoadingFailed;
    public event EventHandler<EventArgs<string>>? MeasurementLoaded;
    public event EventHandler<EventArgs<(string, bool, string, string, FailureReasonType)>>? MeasurementAborted;
    public event EventHandler<EventArgs<string>>? MeasurementCanceled;
    public event EventHandler<EventArgs<string>>? MeasurementDone;
}
