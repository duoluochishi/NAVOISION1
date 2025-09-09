using MaterialDesignThemes.Wpf;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class PartStatusToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (PartStatus)value == PartStatus.Normal ? PackIconKind.CheckBold : PackIconKind.CloseThick;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
