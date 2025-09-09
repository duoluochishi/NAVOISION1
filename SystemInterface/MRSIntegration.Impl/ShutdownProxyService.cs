//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Environment;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class ShutdownProxyService : IShutdownProxyService
{
	private bool _isInitialized;
	private readonly IMapper _mapper;
	private readonly ILogger<ShutdownProxyService> _logger;
    
    public event EventHandler<(bool IsConnected, ShutdownScope ShutdownScope)>? ConnectionStatusChanged;
    public event EventHandler<ShutdownStatusArgs>? ShutdownStatusChanged;

	public ShutdownProxyService(ILogger<ShutdownProxyService> logger, IMapper mapper)
    {
	    _isInitialized = false;
		_logger = logger;
        _mapper = mapper;
        ShutdownProxy.Instance.ConnectionChanged += ShutdownProxy_ConnectionChanged;
        ShutdownProxy.Instance.ShutdownStatusChanged += ShutdownProxy_ShutdownStatusChanged;
        
        Initialize();
	}

    private void Initialize()
    {
	    if (_isInitialized)
		    return;

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

	    var serverInfo = new FacadeProxy.Common.Models.ServerInfo(deviceAddress, reconCommandAddress, reconStateAddress, reconDataAddress);

        var offlineServer = new FacadeProxy.Models.OfflineServerInfo
        {
            IP = RuntimeConfig.MRSServices.OfflineCommandServer.IP,
            CmdPort = RuntimeConfig.MRSServices.OfflineCommandServer.Port,
            StatePort = RuntimeConfig.MRSServices.OfflineStatusServer.Port
        };

	    try
	    {
		    ShutdownProxy.Instance.Init(serverInfo, offlineServer);
		    _logger.LogDebug($"ShutdownProxyService current status (Initialization)");
		    _isInitialized = true;
	    }
	    catch (Exception ex)
	    {
		    _logger.LogError(ex, $"Initialize exception: {ex.Message}");
	    }
    }
	private void ShutdownProxy_ShutdownStatusChanged(object? sender, ShutdownStatusArgs e)
    {
        try
        {
            ShutdownStatusChanged?.Invoke(sender, e);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"ShutdownStatusChanged handling failed: {ex.Message}");
        }
    }

    private void ShutdownProxy_ConnectionChanged(object? sender, ShutdownConnectionStatusArgs e)
    {
        try
        {
            ConnectionStatusChanged?.Invoke(this, (e.Connected, e.Scope));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"ConnectionStatusChanged handling failed: {ex.Message}");
        }
    }

    public BaseCommandResult Shutdown(ShutdownScope scope)
    {
        var result = ShutdownProxy.Instance.Shutdown(scope);
        _logger.LogInformation($"ShutdownProxyService call shutdown method with param {scope.ToString()} and result {result.Status.ToString()}");
		if (result is null || result.Status != CommandStatus.Success)
        {
            _logger.LogDebug($"Shutdown failed: {JsonConvert.SerializeObject(new { Status = result?.Status, Codes = result?.ErrorCodes.Codes })}");
        }
        return new BaseCommandResult
        {
            Status = _mapper.Map<CommandExecutionStatus>(result.Status),
            Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
        };
    }

    public BaseCommandResult Restart(ShutdownScope scope)
    {
        //todo:待完善
        var result = ShutdownProxy.Instance.Restart(scope);
        if (result is null || result.Status != CommandStatus.Success)
        {
            _logger.LogDebug($"Restart failed: {result?.Status.ToString()}, {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
        }
        return new BaseCommandResult
        {
            Status = _mapper.Map<CommandExecutionStatus>(result.Status),
            Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
        };
    }

    public BaseCommandResult CanShutdown(ShutdownScope scope)
    {
        var result = ShutdownProxy.Instance.CanShutdown(scope);
        if (result is null || result.Status != CommandStatus.Success)
        {
            _logger.LogDebug($"Can not shutdow {scope.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
        }
        return new BaseCommandResult
        {
            Status = _mapper.Map<CommandExecutionStatus>(result.Status),
            Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
        };
    }

    public BaseCommandResult CanRestart(ShutdownScope scope)
    {
        var result = ShutdownProxy.Instance.CanRestart(scope);
        if (result is null || result.Status != CommandStatus.Success)
        {
            _logger.LogDebug($"Can not restart {scope.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
        }
        return new BaseCommandResult
        {
            Status = _mapper.Map<CommandExecutionStatus>(result.Status),
            Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
        };
    }


}
