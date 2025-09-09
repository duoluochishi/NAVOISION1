//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole;

public class Global
{
	private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

	public Application Application { get; set; }

	public List<SubProcessSetting>? SubProcessesList { get; set; } = new();

	public string StudyId { get; set; } = string.Empty;

	//public Action? ShowScanList;
	//public Action? ShowReconList;

	public static Global Instance => _instance.Value;

	private Global()
	{
		try
		{
			IWorkflow? _workflowClientProxy = CTS.Global.ServiceProvider.GetRequiredService<IWorkflow>();
			StudyId = _workflowClientProxy?.GetCurrentStudy();
		}
		catch (Exception ex)
		{
			CTS.Global.Logger.LogError($"Examination Global Error {0}", ex);
		}
	}

	private static ClientInfo? _clientInfo;
	private static MCSServiceClientProxy? _serviceClientProxy;
	private static JobClientProxy? _jobClientProxy;

	public void Subscribe()
	{
		var tag = $"[NanoConsole]-{DateTime.Now:yyyyMMddHHmmss}";
		_clientInfo = new() { Id = tag };

        _serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);

		_jobClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<JobClientProxy>();
		_jobClientProxy?.Subscribe(_clientInfo);
	}
	public void Unsubscribe()
	{
		if (null != _clientInfo)
		{
            _serviceClientProxy?.Unsubscribe(_clientInfo);
			_jobClientProxy?.Unsubscribe(_clientInfo);
		}
	}

	public void SetWindowHwnd(IntPtr windowHwnd)
	{
		var consoleApplicationService = CTS.Global.ServiceProvider?.GetRequiredService<IConsoleApplicationService>();
		consoleApplicationService?.SetWindowHwnd(ProcessStartPart.Master, windowHwnd);
	}

	/// <summary>
	/// 预加载
	/// </summary>
	public void Initialize()
	{
		CTS.Global.ServiceProvider?.GetRequiredService<MainWindow>();
		CTS.Global.ServiceProvider?.GetRequiredService<EmergencyWindow>();
		//预加载SelfCheckControl页面，因为打开慢
		CTS.Global.ServiceProvider?.GetRequiredService<SelfCheckControl>();
		CTS.Global.ServiceProvider?.GetRequiredService<LockControl>();
		CTS.Global.ServiceProvider?.GetRequiredService<MessagesViewModel>();
	}
}