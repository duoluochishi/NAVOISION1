namespace NV.CT.Models.MouseKeyboard;

public class KeyboardEventData
{
	public string EventType { get; set; } = string.Empty;
	public string KeyCode { get; set; } = string.Empty;
	public string KeyChar { get; set; } = string.Empty;
	public string Shift { get; set; } = string.Empty;
	public string Alt { get; set; } = string.Empty;
	public string Control { get; set; } = string.Empty;
}