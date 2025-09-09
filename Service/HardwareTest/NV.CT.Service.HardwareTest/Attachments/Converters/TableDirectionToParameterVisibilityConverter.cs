using NV.CT.FacadeProxy.Models.MotionControl.Table;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class TableDirectionToParameterVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;

            //移动方向
            var direction = (TableMoveDirection)value;
            //入参方向
            var input = (String)parameter;
            //判定显隐
            return (direction, input) switch
            {
                (TableMoveDirection.Horizontal, "Horizontal") => Visibility.Visible,
                (TableMoveDirection.Vertical, "Vertical") => Visibility.Visible,
                (TableMoveDirection.Union, "Horizontal" or "Vertical") => Visibility.Visible,
                (TableMoveDirection.AxisX, "AxisX") => Visibility.Visible,
                _ => Visibility.Collapsed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
