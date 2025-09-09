//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.AuxConsole;

public class Global
{
	private static readonly Lazy<Global> _instance = new(() => new Global());

	//public IServiceProvider ServiceProvider { get; set; }

	//public List<SubProcessSetting> SubProcessesList { get; set; } = new List<SubProcessSetting>();

	public string StudyId { get; set; } = string.Empty;

	public static Global Instance => _instance.Value;

	private Global()
	{
	}
	private MCSServiceClientProxy? _serviceClientProxy;
	private ClientInfo? _clientInfo;

	public void Subscribe()
	{
		var tag = $"[NanoAuxConsole]-{DateTime.Now:yyyyMMddHHmmss}";
		_clientInfo = new() { Id = tag };
        _serviceClientProxy = CTS.Global.ServiceProvider.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);
	}
	public void Unsubscribe()
	{
		if (null != _clientInfo)
		{
            _serviceClientProxy?.Unsubscribe(_clientInfo);
		}
	}

	public void SetWindowHwnd(IntPtr windowHwnd)
	{
		var consoleApplicationService = CTS.Global.ServiceProvider.GetRequiredService<IConsoleApplicationService>();
		consoleApplicationService.SetWindowHwnd(ProcessStartPart.Auxilary, windowHwnd);
	}

	public void Initialize()
	{
	}
}