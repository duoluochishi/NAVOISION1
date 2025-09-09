//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NV.CT.AppService.Contract;

public class ApplicationInfo
{
	public string ProcessName { get; set; } = string.Empty;
	public IntPtr ControlHwnd { get; set; }
	public string Parameters { get; set; } = string.Empty;
	public int ProcessId => Process?.Id ?? 0;
	[JsonIgnore]
	public Process? Process { get; set; }
	public string Path { get; set; } = string.Empty;
	public int MaxInstance { get; set; } = 1;
	public ProcessStatus SubProcessStatus { get; set; } = ProcessStatus.None;

	public bool IsActive { get; set; }
}
