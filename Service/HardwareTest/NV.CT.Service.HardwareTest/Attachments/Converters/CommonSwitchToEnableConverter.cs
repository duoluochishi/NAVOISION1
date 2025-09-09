using NV.CT.Service.HardwareTest.Share.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class CommonSwitchToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var commonSwitch = (CommonSwitch)value;

            return commonSwitch == CommonSwitch.Enable ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
