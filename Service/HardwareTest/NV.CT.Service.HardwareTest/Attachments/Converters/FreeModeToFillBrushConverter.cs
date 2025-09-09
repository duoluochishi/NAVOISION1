using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class FreeModeToFillBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = (DetAcqMode)value;

            return (mode == DetAcqMode.FreeMode) ? Brushes.LimeGreen : Brushes.OrangeRed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
