//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.AppService.Impl;

public class SelfCheckService : ISelfCheckService
{
	private readonly ILogger<SelfCheckService> _logger = Global.ServiceProvider.GetRequiredService<ILogger<SelfCheckService>>();
	private readonly List<ISelfCheckingExecutor> _executors = new();
	public event EventHandler<SelfCheckResult>? SelfCheckStatusChanged;

	public void AddSelfCheckingExecutor(ISelfCheckingExecutor executor)
	{
		_executors.Add(executor);
		executor.SelfCheckStatusChanged += Executor_SelfCheckStatusChanged;
	}

	private void Executor_SelfCheckStatusChanged(object? sender, SelfCheckResult e)
	{
		//_logger.LogInformation($"self check status changed {e.ToJson()}");
		SelfCheckStatusChanged?.Invoke(this, e);
	}

	public void StartSelfChecking()
	{
		//_logger?.LogInformation($"self check service start");
		foreach (var executor in _executors)
		{
			Task.Run(() =>
			{
				executor.StartSelfChecking();
			});
		}
	}

	public List<SelfCheckResult> GetSelfCheckResults()
	{
		var list = new List<SelfCheckResult>();
		foreach (var selfCheckingExecutor in _executors)
		{
			List<SelfCheckResult> results = new ();
			try
			{
				results = selfCheckingExecutor.GetSelfCheckResults();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, $"{selfCheckingExecutor.GetType().Name}.GetSelfCheckResults exception: {ex.Message}");
			}
			if (results.Count > 0)
			{
				list.AddRange(results);
			}
		}
		//_logger.LogInformation($"Total get self check results : {list.ToJson()}");
		return list;
	}
}
