using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class IsValidToForegroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Brushes.LimeGreen : Brushes.OrangeRed;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}