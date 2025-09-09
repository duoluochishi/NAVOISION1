//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using IShutdownService = NV.CT.SystemInterface.MCSRuntime.Contract.IShutdownService;

namespace NV.CT.AppService.Impl.Shutdown;

public class MCSShutdownPreconditionChecker : IShutdownPreconditionChecker
{
	private readonly ILogger<MCSShutdownPreconditionChecker> _logger;
	private readonly IShutdownService _shutdownService;

	public MCSShutdownPreconditionChecker()
	{
		_logger = Global.ServiceProvider.GetRequiredService<ILogger<MCSShutdownPreconditionChecker>>();
		_shutdownService = Global.ServiceProvider.GetRequiredService<IShutdownService>();
	}

	public bool IsShutdownPossible()
	{
		_logger.LogDebug($"MCS's IsShutdownPossible test.");
		return _shutdownService.CanShutdown().Status == CTS.Enums.CommandExecutionStatus.Success;
	}

	public bool IsRestartPossible()
	{
		_logger.LogDebug($"MCS's IsRestartPossible test.");
		return _shutdownService.CanRestart().Status == CTS.Enums.CommandExecutionStatus.Success;
	}
}
