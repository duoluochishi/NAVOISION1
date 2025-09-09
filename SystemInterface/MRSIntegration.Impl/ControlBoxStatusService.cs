//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/12 14:58:52           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class ControlBoxStatusService : IControlBoxStatusService
{
    private PartStatus _status = PartStatus.Disconnection;
    private readonly IRealtimeStatusProxyService _realtimeStatusService;
    private readonly ILogger<ControlBoxStatusService> _logger;
    public ControlBoxStatusService(IRealtimeStatusProxyService realtimeStatusService,ILogger<ControlBoxStatusService> logger)
    {
        _logger=logger;
        _realtimeStatusService = realtimeStatusService;
        _realtimeStatusService.CycleStatusChanged += RealtimeStatusService_CycleStatusChanged;
    }

    private void RealtimeStatusService_CycleStatusChanged(object? sender, CTS.EventArgs<Contract.Models.DeviceSystem> e)
    {
        var boxStatus = e.Data.ControlBox.Status;
        if (boxStatus != _status)
        {
            _logger.LogInformation($"ControlBoxStatusService monitor status changed to {boxStatus} , _status={_status}");
            _status = boxStatus;
            StatusChanged?.Invoke(this, _status);
        }
    }

    public PartStatus Status => _status;

    public event EventHandler<PartStatus>? StatusChanged;
}
