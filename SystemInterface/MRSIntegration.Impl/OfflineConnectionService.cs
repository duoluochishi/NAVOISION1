//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 13:07:40    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Models;
using NV.CT.CTS;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class OfflineConnectionService : IOfflineConnectionService
{
    private readonly IMapper _mapper;
    private readonly ILogger<OfflineConnectionService> _logger;
    private readonly IOfflineProxyService _proxyService;

    //todo:暂时确认仅有一台离线机，多台需要处理未dictionary

    private ServiceStatusInfo _currentStatus;

    public bool IsConnected => _proxyService.IsConnected;

    public event EventHandler<EventArgs<ServiceStatusInfo>> ConnectionStatusChanged;

    public event EventHandler<(string Timestamp, string ErrorCode)> ErrorOccured;

    public OfflineConnectionService(IMapper mapper, ILogger<OfflineConnectionService> logger, IOfflineProxyService proxyService)
    {
        _mapper = mapper;
        _logger = logger;
        _proxyService = proxyService;
        _proxyService.ConnectionStatusChanged += Instance_ConnectionStatusChanged;
        _proxyService.ErrorOccured += Instance_ErrorOccured;
    }

    private void Instance_ErrorOccured(object? sender, (string Timestamp, string ErrorCode) e)
    {
        _logger.LogInformation($"OfflineConnectionService.ErrorOccured: {e.Timestamp}, {e.ErrorCode}");
        ErrorOccured?.Invoke(this, e);
    }

    private void Instance_ConnectionStatusChanged(object sender, bool args)
    {
        _logger.LogInformation($"OfflineConnectionService.ConnectionStatusChanged: {args}");
        _currentStatus = new ServiceStatusInfo { Connected = args };
        ConnectionStatusChanged?.Invoke(this, new EventArgs<ServiceStatusInfo>(_currentStatus));
    }
}
