using MaterialDesignThemes.Wpf;
using NV.CT.Service.HardwareTest.Share.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class XOnlineStatusToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (XOnlineStatus)value switch
            {
                XOnlineStatus.Online => PackIconKind.CheckBold,
                XOnlineStatus.Offline => PackIconKind.CloseThick,
                _ => throw new ArgumentException(nameof(value))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
