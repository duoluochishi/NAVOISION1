//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.AppService.Impl;

/// <summary>
/// GRPC shutdown service impl
/// </summary>
public class ShutdownService : IShutdownService
{
	public event EventHandler? ShutdownStatusChanged;

	private readonly ILogger<ShutdownService>? _logger = Global.ServiceProvider.GetService<ILogger<ShutdownService>>();

	private readonly List<IShutdownPreconditionChecker> _computerCheckers = new();
	private readonly List<IShutdownPreconditionChecker> _systemCheckers = new();

	private readonly IShutdownProxyService _shutdownProxyService = Global.ServiceProvider.GetRequiredService<IShutdownProxyService>();

	private readonly SystemInterface.MCSRuntime.Contract.IShutdownService _mcsShutdownService =
		Global.ServiceProvider.GetRequiredService<SystemInterface.MCSRuntime.Contract.IShutdownService>();

	public ShutdownService()
	{
		var mcs = new MCSShutdownPreconditionChecker();
		var mrs = new MRSShutdownPreconditionChecker();
		_computerCheckers.Add(mcs);
		_computerCheckers.Add(mrs);

		var embeddedSystem = new EmbeddedSystemShutdownPreconditionChecker();
		_systemCheckers.Add(mcs);
		_systemCheckers.Add(mrs);
		_systemCheckers.Add(embeddedSystem);

		if (_shutdownProxyService != null)
			_shutdownProxyService.ShutdownStatusChanged += _shutdownProxyService_ShutdownStatusChanged;
	}

	private void _shutdownProxyService_ShutdownStatusChanged(object? sender, FacadeProxy.Common.Arguments.ShutdownStatusArgs e)
	{
		_logger?.LogInformation($"grpc shutdownservice received {e.Status} , {e.OperationScope} , {e.ErrorCode}");

		if (e.OperationScope == ShutdownScope.OfflineComputer && e.Status == ShutdownStatus.Finished)
		{
			_logger?.LogInformation($"received ShutdownProxy service send offlinecomputer is shutdown finished , will shutdown mcs.");
			_mcsShutdownService?.Shutdown();
		}
	}


	public void GetCurrentShutdownStatus()
	{
		//todo:待实现
	}

	public void Restart()
	{
		if (!PreCheckRestart(_computerCheckers))
			return;

		//离线机先重启
		_shutdownProxyService?.Restart(ShutdownScope.OfflineComputer);
		//接着是 MCS重启
		//TODO: call mcs restart

		_mcsShutdownService?.Restart();
	}

	public BaseCommandResult Shutdown()
	{
		_logger?.LogInformation($"call shutdown offline computer");
		//这时，只关闭离线机，等离线机关闭完毕，收到离线机关机事件之后，关闭 MCS
		return _shutdownProxyService.Shutdown(ShutdownScope.OfflineComputer);
	}

	public BaseCommandResult ShutdownSystem()
	{
		return _shutdownProxyService.Shutdown(ShutdownScope.System);
	}

	private bool PreCheckShutdown(IList<IShutdownPreconditionChecker> checkers)
	{
		var isOk = true;
		foreach (var checker in checkers)
		{
			if (!checker.IsShutdownPossible())
			{
				isOk = false;
				break;
			}
		}

		return isOk;
	}

	private bool PreCheckRestart(IList<IShutdownPreconditionChecker> checkers)
	{
		var isOk = true;
		foreach (var checker in checkers)
		{
			if (!checker.IsRestartPossible())
			{
				isOk = false;
				break;
			}
		}

		return isOk;
	}

	public List<BaseCommandResult> CanShutdown()
	{
		var mcsCanShutdownResult = _mcsShutdownService.CanShutdown();
		var mrsResult = _shutdownProxyService.CanShutdown(ShutdownScope.OfflineComputer);
		var list = new List<BaseCommandResult>();
		list.Add(mcsCanShutdownResult);
		list.Add(mrsResult);
		return list;
	}

	public List<BaseCommandResult> CanShutdownSystem()
	{
		var mcsCanShutdownResult = _mcsShutdownService.CanShutdown();
		var mrsResult = _shutdownProxyService.CanShutdown(ShutdownScope.System);
		var list = new List<BaseCommandResult>();
		list.Add(mcsCanShutdownResult);
		list.Add(mrsResult);
		return list;
	}
}
