//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/10 13:33:09     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Environment;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class RealtimeProxyService : IRealtimeProxyService
{
    private bool _isInitialized;
    private readonly ILogger<RealtimeProxyService> _logger;

    public AuxBoard AuxBoard => AcqReconProxy.Instance.CurrentDeviceSystem.AuxBoard;

    public bool IsDeviceConnected { get; private set; }

    public bool IsReconConnected { get; private set; }

    public RealtimeStatus RealtimeStatus { get; private set; }

    public SystemStatus SystemStatus { get; private set; }

    public event EventHandler<AcqReconStatusArgs> AcqReconStatusChanged;
    public event EventHandler<SystemStatusArgs> SystemStatusChanged;
    public event EventHandler<ImageSavedEventArgs> ReconImageSaved;
    public event EventHandler<CycleStatusArgs> CycleStatusChanged;
    public event EventHandler<RealtimeEventArgs> RealTimeStatusChanged;
    public event EventHandler<ConnectionStatusArgs> DeviceConnectionChanged;
    public event EventHandler<ConnectionStatusArgs> ReconConnectionChanged;
    public event EventHandler<List<string>> DeviceErrorOccurred;
    public event EventHandler<List<string>> ScanReconErrorOccurred;
    public event EventHandler<RawImageSavedEventArgs> RawImageSaved;

    public RealtimeProxyService(ILogger<RealtimeProxyService> logger)
    {
        _isInitialized = false;
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        var deviceAddress = new FacadeProxy.Common.Models.IPPort
        {
            IP = RuntimeConfig.MRSServices.DeviceServer.IP,
            Port = RuntimeConfig.MRSServices.DeviceServer.Port
        };
        var reconCommandAddress = new FacadeProxy.Common.Models.IPPort
        {
            IP = RuntimeConfig.MRSServices.ReconCommandServer.IP,
            Port = RuntimeConfig.MRSServices.ReconCommandServer.Port
        };
        var reconStateAddress = new FacadeProxy.Common.Models.IPPort
        {
            IP = RuntimeConfig.MRSServices.ReconStatusServer.IP,
            Port = RuntimeConfig.MRSServices.ReconStatusServer.Port
        };
        var reconDataAddress = new FacadeProxy.Common.Models.IPPort
        {
            IP = RuntimeConfig.MRSServices.ReconDataServer.IP,
            Port = RuntimeConfig.MRSServices.ReconDataServer.Port
        };

        var server = new FacadeProxy.Common.Models.ServerInfo(deviceAddress, reconCommandAddress, reconStateAddress, reconDataAddress);

        AcqReconProxy.Instance.AcqReconStatusChanged += (s, e) => { AcqReconStatusChanged?.Invoke(s, e); };
        AcqReconProxy.Instance.SystemStatusChanged += (s, e) => { SystemStatusChanged?.Invoke(s, e); };
        AcqReconProxy.Instance.ReconImageSaved += (s, e) => { ReconImageSaved?.Invoke(s, e); };
        AcqReconProxy.Instance.CycleStatusChanged += (s, e) => { CycleStatusChanged?.Invoke(s, e); };
        AcqReconProxy.Instance.RealTimeStatusChanged += (s, e) => {
            RealtimeStatus = e.Status;
            RealTimeStatusChanged?.Invoke(s, e);
        };        
        AcqReconProxy.Instance.DeviceConnectionChanged += (s, e) => {
            IsDeviceConnected = e.Connected;
            DeviceConnectionChanged?.Invoke(s, e);
        };

        AcqReconProxy.Instance.ScanReconConnectionChanged += (s, e) => {
            IsReconConnected = e.Connected;
            ReconConnectionChanged?.Invoke(s, e);
        };

        AcqReconProxy.Instance.DeviceErrorOccurred += (s, e) => {
            DeviceErrorOccurred?.Invoke(s, e.ErrorCodes.Codes);
        };

        AcqReconProxy.Instance.ScanReconErrorOccurred += (s, e) => {
            ScanReconErrorOccurred?.Invoke(s, e.ErrorCodes.Codes);
        };

        AcqReconProxy.Instance.RawImageSaved += (s, e) => {
            RawImageSaved?.Invoke(s, e);
        };

        try
        {
            AcqReconProxy.Instance.Init(server);
            _logger.LogDebug($"RealtimeConnection current status (Initialization): {AcqReconProxy.Instance.CurrentDeviceSystem.RealtimeStatus}, {AcqReconProxy.Instance.CurrentDeviceSystem.SystemStatus}");
            _isInitialized = true;

            RealtimeStatus = AcqReconProxy.Instance.CurrentDeviceSystem.RealtimeStatus;
            SystemStatus = AcqReconProxy.Instance.CurrentDeviceSystem.SystemStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Initialize excepion: {ex.Message}");
        }
    }
}
