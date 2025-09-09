namespace NV.CT.Models.MouseKeyboard;

public class MouseEventData
{
	public string EventType { get; set; } = string.Empty;
	public string Button { get; set; } = string.Empty;
	public string X { get; set; } = string.Empty;
	public string Y { get; set; } = string.Empty;
	public string Delta { get; set; } = string.Empty;
}