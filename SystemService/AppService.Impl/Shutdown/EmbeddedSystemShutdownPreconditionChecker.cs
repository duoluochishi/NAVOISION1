//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.AppService.Impl.Shutdown;

public class EmbeddedSystemShutdownPreconditionChecker : IShutdownPreconditionChecker
{
    private readonly ILogger<EmbeddedSystemShutdownPreconditionChecker> _logger;
    private readonly IShutdownProxyService _shutdownService;

    public EmbeddedSystemShutdownPreconditionChecker()
    {
        _logger = Global.ServiceProvider.GetRequiredService<ILogger<EmbeddedSystemShutdownPreconditionChecker>>();
        _shutdownService = Global.ServiceProvider.GetRequiredService<IShutdownProxyService>();
    }

    public bool IsRestartPossible()
    {
        _logger.LogDebug($"Embedded System's IsRestartPossible test.");
        return _shutdownService.CanRestart(ShutdownScope.EmbeddedDevice).Status == CTS.Enums.CommandExecutionStatus.Success;
    }

    public bool IsShutdownPossible()
    {
        var isOk= _shutdownService.CanShutdown(ShutdownScope.EmbeddedDevice).Status == CTS.Enums.CommandExecutionStatus.Success;
        _logger.LogDebug($"Embedded System's IsShutdownPossible test {isOk}");
		return isOk;
    }
}
