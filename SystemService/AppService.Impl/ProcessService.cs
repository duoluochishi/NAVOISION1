//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.AppService.Impl;

public class ProcessService
{
	private readonly ILogger<ProcessService> _logger;
	public event EventHandler<ProcessInfo>? ProcessStatusChanged;

	public ProcessService(ILogger<ProcessService> logger)
	{
		_logger = logger;
	}

	public (bool,string) StartProcess(string applicationName, string parameter, string applicationPath, IntPtr windowHwnd)
	{
		try
		{
			var task = Task.Run(() => RealStartProcess(applicationName, parameter, applicationPath, windowHwnd));

			return (task.Result,string.Empty);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, ex.Message);
			return (false,ex.Message );
		}
	}

	private Task<bool> RealStartProcess(string applicationName, string parameter, string applicationPath, IntPtr windowHwnd)
	{
		var info = PipeHelper.ServerStream(process =>
		{
			process.Exited += Process_Exited;
		}, applicationPath, windowHwnd, parameter);

		var processInfo = new ProcessInfo()
		{
			ApplicationName = applicationName,
			Parameter = parameter,
			Hwnd = info.Item1,
			Process = info.Item2,
		};

		//通知 Starting 事件
		processInfo.ProcessStatus = ProcessStatus.Starting;
		ProcessStatusChanged?.Invoke(this, processInfo);

		if (info.Item1 == IntPtr.Zero)
		{
			//通知 错误 事件
			processInfo.ProcessStatus = ProcessStatus.Failure;
			ProcessStatusChanged?.Invoke(this, processInfo);
			return Task.FromResult(false);
		}

		//通知 Started 事件
		processInfo.ProcessStatus = ProcessStatus.Started;
		ProcessStatusChanged?.Invoke(this, processInfo);
		return Task.FromResult(true);
	}

	private void Process_Exited(object? sender, EventArgs e)
	{
		if (sender is Process { HasExited: true } process)
		{
			var processInfo = new ProcessInfo()
			{
				ApplicationName = string.Empty,
				Parameter = string.Empty,
				Hwnd = IntPtr.Zero,
				Process = process,
				ProcessStatus = ProcessStatus.Closed
			};

			ProcessStatusChanged?.Invoke(this, processInfo);
		}
	}
}