using NV.CT.FacadeProxy.Common.Enums.SelfCheck;

namespace NV.CT.NanoConsole.Model;

public class FirmwareInfo
{
	public int Index { get; set; }
	public string TypeName { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string ReleaseVersion { get; set; } = string.Empty;
	public string CurrentVersion { get; set; } = string.Empty;
	public SelfCheckStatus Status { get; set; }

	public string StatusString => Status.ToString();

	//public bool NeedCorrect { get; set; } = false;
}