using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter;
public class PercentToAngleConverter : IValueConverter
{
    private const double SCALE_NUMBER = 360.0D;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not null && double.TryParse(value.ToString(), out double doubleValue))
        {
            return (doubleValue >= 1) ? SCALE_NUMBER : (doubleValue * SCALE_NUMBER);
        }
        else
        {
            return SCALE_NUMBER;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}