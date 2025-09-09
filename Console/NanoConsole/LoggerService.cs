using System.Collections.Concurrent;

namespace NV.CT.NanoConsole;

public class HeatCapacityLoggerService : IHostedService,IDisposable
{
	private Timer _timer;

	private readonly IHeatCapacityService _heatCapacityService;

	public HeatCapacityLoggerService(IHeatCapacityService heatCapacityService)
	{
		_heatCapacityService = heatCapacityService;
	}

    public Task StartAsync(CancellationToken cancellationToken)
	{
		// 每隔 1 分钟调用一次 WriteLog 方法
		_timer = new Timer(WriteLog, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
		return Task.CompletedTask;
	}
	private void WriteLog(object state)
	{
		if (_heatCapacityService.Current == null || _heatCapacityService.Current.Count == 0) return;

		var logFilePath = GetLogFilePath();

		var content = _heatCapacityService.Current.ToList().ToJson();

		try
		{
			// 写入日志内容到文件
			File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {content}{Environment.NewLine}");
		}
		catch (Exception ex)
		{
			File.AppendAllText(logFilePath, $"{ex.Message}:{ex.StackTrace} {Environment.NewLine}");
		}
	}

	private string GetLogFilePath()
	{
		var logDirectory = Path.Combine(RuntimeConfig.Console.MCSLog.Path);
		if (!Directory.Exists(logDirectory))
		{
			Directory.CreateDirectory(logDirectory);
		}
		return Path.Combine(logDirectory, $"HeatCapacity_{DateTime.Now:yyyyMMdd}.log");
	}
	
	public Task StopAsync(CancellationToken cancellationToken)
	{
		_timer?.Change(Timeout.Infinite, 0); // 停止定时器
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_timer.Dispose();
	}
}