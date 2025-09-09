using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter
{
    public class BooleanToInverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isBool = value is bool;
            return isBool ? !(bool)value : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isBool = value is bool;
            return isBool ? !(bool)value : false;

        }
    }
}
