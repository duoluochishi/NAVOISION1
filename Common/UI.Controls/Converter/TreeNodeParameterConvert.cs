using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter;
public class TreeNodeParameterConvert : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values is null ? null : values.Clone();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}