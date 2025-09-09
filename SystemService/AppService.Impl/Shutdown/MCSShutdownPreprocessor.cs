//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using IShutdownService = NV.CT.SystemInterface.MCSRuntime.Contract.IShutdownService;

namespace NV.CT.AppService.Impl.Shutdown;

public class MCSShutdownPreprocessor : IShutdownPreprocessor
{
	private readonly ILogger<MCSShutdownPreprocessor> _logger;
	private readonly IShutdownService _shutdownService;

	public MCSShutdownPreprocessor()
	{
		_logger = Global.ServiceProvider.GetRequiredService<ILogger<MCSShutdownPreprocessor>>();
		_shutdownService = Global.ServiceProvider.GetRequiredService<IShutdownService>();
	}

	public void Shutdown()
	{
		_logger.LogDebug($"MCS shutdown, executing.");
		_shutdownService.Shutdown();
	}

	public void Restart()
	{
		_logger.LogDebug($"MCS restart, executing.");
		_shutdownService.Restart();
	}
}
