namespace NV.CT.AppService.Impl;

public class ProcessInfo
{
	public string ApplicationName { get; set; } = string.Empty;
	public string Parameter { get; set; } = string.Empty;
	public IntPtr Hwnd { get; set; }
	public Process? Process { get; set; }
	public int ProcessId => Process?.Id ?? 0;
	public ProcessStatus ProcessStatus { get; set; } = ProcessStatus.None;
}
