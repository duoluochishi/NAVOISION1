using MaterialDesignThemes.Wpf;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class TableMoveStatusToPackIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (TableMoveStatus)value;

            return status switch
            {
                TableMoveStatus.NotArrived => PackIconKind.TimerSand,
                TableMoveStatus.Arrived => PackIconKind.Check,
                _ => throw new ArgumentException(nameof(value))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
