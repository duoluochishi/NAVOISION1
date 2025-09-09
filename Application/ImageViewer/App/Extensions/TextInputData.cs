namespace NV.CT.ImageViewer.Extensions;

/// <summary>
/// 文本输入对象
/// </summary>
public class TextInputData
{
	public Point ChangePoint { get; set; }
	public string Text { get; set; } = string.Empty;
	public TextInputAction Action { get; set; }
}

public enum TextInputAction
{
	InputText,
	ChangeText
}
