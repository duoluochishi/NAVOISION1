//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl;

public class RTDReconService : IRTDReconService
{
    private readonly ILogger<RTDReconService> _logger;
    private readonly IRealtimeReconProxyService _realtimeProxyService;
    public ReconModel? CurrentActiveRecon { get; private set; }

    public RTDReconService(ILogger<RTDReconService> logger,
        IRealtimeReconProxyService realtimeProxyService)
    {
        _logger = logger;
        _realtimeProxyService = realtimeProxyService;
        _realtimeProxyService.ImageReceived += OnProxyService_ImageReceived;

        _realtimeProxyService.RealtimeReconStatusChanged += RealtimeProxyService_RealtimeReconStatusChanged;
    }

    private void RealtimeProxyService_RealtimeReconStatusChanged(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        _logger.LogInformation($"RealtimeReconStatusChanged: {JsonConvert.SerializeObject(new { CreateTime = DateTime.Now.ToFullString(), ScanId = e.Data.ScanId, ReconId = e.Data.ReconId, Status = e.Data.Status.ToString(), LastImagePath = e.Data.LastImage })}");

        if (e.Data.Status == RealtimeReconStatus.Loaded)
        {
            ReconLoaded?.Invoke(this, e);
        }
        else if (e.Data.Status == RealtimeReconStatus.Reconning)
        {
            ReconReconning?.Invoke(this, e);
        }
        else if (e.Data.Status == RealtimeReconStatus.Finished)
        {
            ReconDone?.Invoke(this, e);
        }
        else if (e.Data.Status == RealtimeReconStatus.Error)
        {
            ReconAborted?.Invoke(this, e);
        }
        else if (e.Data.Status == RealtimeReconStatus.Cancelled)
        {
            ReconCancelled?.Invoke(this, e);
        }
    }

    private void OnProxyService_ImageReceived(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        ImageReceived?.Invoke(this, e);
    }

    public event EventHandler<CTS.EventArgs<RealtimeReconInfo>>? ReconLoaded;

    public event EventHandler<CTS.EventArgs<RealtimeReconInfo>>? ReconReconning;

    public event EventHandler<CTS.EventArgs<RealtimeReconInfo>>? ImageReceived;

    public event EventHandler<CTS.EventArgs<RealtimeReconInfo>>? ReconCancelled;

    public event EventHandler<CTS.EventArgs<RealtimeReconInfo>>? ReconDone;

    public event EventHandler<CTS.EventArgs<RealtimeReconInfo>>? ReconAborted;
}