using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter;
public class DateTimeConverter : BasicsConverter
{
    private const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime valueDateTime)
        {
            return valueDateTime == DateTime.MinValue ? string.Empty : valueDateTime.ToString(DATETIME_FORMAT);
        }
        else
        {
            return string.Empty;
        }
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is not null && DateTime.TryParse(value.ToString(), out DateTime valueDateTime))
        {
            return valueDateTime;
        }
        else
        {
            return DateTime.MinValue;
        }

    }
}