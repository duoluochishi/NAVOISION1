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

public class MRSShutdownPreconditionChecker : IShutdownPreconditionChecker
{
    private readonly ILogger<MRSShutdownPreconditionChecker> _logger;
    private readonly IShutdownProxyService _shutdownService;

    public MRSShutdownPreconditionChecker()
    {
        _logger = Global.ServiceProvider.GetRequiredService<ILogger<MRSShutdownPreconditionChecker>>();
        _shutdownService = Global.ServiceProvider.GetRequiredService<IShutdownProxyService>();
    }

    public bool IsRestartPossible()
    {
        _logger.LogDebug($"MRS's IsRestartPossible test.");
        return _shutdownService.CanRestart(ShutdownScope.OfflineComputer).Status == CTS.Enums.CommandExecutionStatus.Success;
    }

    public bool IsShutdownPossible()
    {
        _logger.LogDebug($"MRS's IsShutdownPossible test.");
        return _shutdownService.CanShutdown(ShutdownScope.OfflineComputer).Status == CTS.Enums.CommandExecutionStatus.Success;
    }
}