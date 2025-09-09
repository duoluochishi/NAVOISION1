using NV.CT.CTS.Enums;

namespace NV.CT.AppService.Contract;

public class ApplicationResponse
{
	public string ApplicationName { get; set; } = string.Empty;
	public string Parameters { get; set; } = string.Empty;
	public int ProcessId { get; set; } = int.MinValue;
	public IntPtr ControlHandle { get; set; } = IntPtr.Zero;
	public ProcessStatus Status { get; set; } = ProcessStatus.None;
	/// <summary>
	/// ”√¿¥√Ë ˆProcessStatusµƒ
	/// </summary>
	public string Message { get; set; }=string.Empty;
	public bool NeedConfirm { get; set; }
	public object? ExtraParameter { get; set; }
	public ProcessStartPart ProcessStartPart { get; set; }
}