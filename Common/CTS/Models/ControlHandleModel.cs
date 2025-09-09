using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Models;

public class ControlHandleModel
{
	public string ControlKey => ItemName + Parameters;

	public string ItemName { get; set; } = string.Empty;
	public string Parameters { get; set; } = string.Empty;
	public int ProcessId { get; set; } = int.MinValue;
	public ControlActiveStatus ActiveStatus { get; set; } = ControlActiveStatus.None;
	public string ActiveStatusString=>ActiveStatus.ToString();
	public IntPtr ControlHandle { get; set; } = IntPtr.Zero;

	public ControlModelType ControlModelType { get; set; }

	public string ControlModelTypeString => ControlModelType.ToString();

	/// <summary>
	/// 进程启动所属容器
	/// </summary>
	public ProcessStartPart ProcessStartContainer { get; set; }
	public string ProcessStartContainerString=>ProcessStartContainer.ToString();


	//public string CloseAndActiveName { get; set; } = string.Empty;
	//public bool IsConsoleControl { get; set; } = false;
	//public ViewHost ControlViewHost { get; set; } = null;
	//public bool IsMainConsole { get; set; } = false;
}