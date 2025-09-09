using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class ComponentStatusScrollViewerHeightAutoFitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temp = (double)value - 80;

            return temp > 0 ? temp : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
