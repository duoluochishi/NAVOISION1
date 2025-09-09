namespace MaterialDesignThemes.Wpf.Controls.MarkableTextBox;

public enum TextTypes
{
    /// <summary>
    /// 允许所有文本输入（除了特殊字符）。
    /// </summary>
    Text,

    /// <summary>
    /// 只允许输入数字、小数点、以及其他操控键。
    /// </summary>
    Decimal,

    /// <summary>
    /// 只允许输入字母+数字
    /// </summary>
    LetterAndNumber,

    /// <summary>
    /// IP地址
    /// </summary>
    IP,

    /// <summary>
    /// 允许任意输入
    /// </summary>
    ALL,
}
