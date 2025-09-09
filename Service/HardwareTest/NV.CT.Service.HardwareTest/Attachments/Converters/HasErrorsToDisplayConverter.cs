using MaterialDesignThemes.Wpf;
using System.Globalization;
using System;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class HasErrorsToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? PackIconKind.AlertBox : PackIconKind.Numeric0Box;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
