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

public class MRSShutdownPreprocessor : IShutdownPreprocessor
{
    private readonly ILogger<MRSShutdownPreprocessor> _logger;
    private readonly IShutdownProxyService _shutdownService;

    public MRSShutdownPreprocessor()
    {
        _logger = Global.ServiceProvider.GetRequiredService<ILogger<MRSShutdownPreprocessor>>();
        _shutdownService = Global.ServiceProvider.GetRequiredService<IShutdownProxyService>();
    }

    public void Restart()
    {
        _logger.LogDebug($"MRS restart, executing.");
        _shutdownService.Restart(ShutdownScope.OfflineComputer);
    }

    public void Shutdown()
    {
        _logger.LogDebug($"MRS shutdown, executing.");
        _shutdownService.Shutdown(ShutdownScope.OfflineComputer);
    }
}
