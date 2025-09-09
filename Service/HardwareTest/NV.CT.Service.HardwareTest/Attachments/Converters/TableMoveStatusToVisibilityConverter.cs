using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class TableMoveStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            var status = (TableMoveStatus)value;

            return status switch
            {
                TableMoveStatus.NotArrived => Visibility.Collapsed,
                TableMoveStatus.Arrived => Visibility.Visible,
                _ => Visibility.Collapsed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
