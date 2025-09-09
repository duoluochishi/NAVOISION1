using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter;
public class BodyPartSelConverter : IValueConverter
{
    private readonly string STRING_ONE = "1";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return string.Empty;
        }

        var stringValue = value.ToString();
        var isEqual = !string.IsNullOrEmpty(stringValue) && stringValue.Equals(parameter.ToString());
        return isEqual ? STRING_ONE : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}