using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class BooleanToVisibilityConverter : BasicsConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
    }
}
