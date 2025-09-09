namespace MaterialDesignThemes.Wpf.Controls;

public class ArithmeticHelper
{
    /// <summary>
    ///     判断是否是有效的双精度浮点数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValidDoubleValue(object value)
    {
        var d = (double)value;
        if (!double.IsNaN(d))
            return !double.IsInfinity(d);
        return false;
    }
}