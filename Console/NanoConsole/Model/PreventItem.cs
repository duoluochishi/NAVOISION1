using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.NanoConsole.Model;

/// <summary>
/// Shutdown 页面阻止关机项
/// </summary>
public class PreventItem
{
	public string Name { get; set; } = string.Empty;
	public ShutdownStatus Status { get; set; }
}
