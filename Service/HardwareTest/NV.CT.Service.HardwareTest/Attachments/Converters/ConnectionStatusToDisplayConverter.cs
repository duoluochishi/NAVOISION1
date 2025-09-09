using MaterialDesignThemes.Wpf;
using NV.CT.Service.HardwareTest.Share.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class ConnectionStatusToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ConnectionStatus)value == ConnectionStatus.Connected ? PackIconKind.LanConnect : PackIconKind.LanDisconnect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
