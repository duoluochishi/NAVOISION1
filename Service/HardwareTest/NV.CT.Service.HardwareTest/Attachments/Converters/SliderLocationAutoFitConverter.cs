using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class SliderLocationAutoFitConverter : IValueConverter
    {
        public bool HeightAutoFit { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temp = (double)value - 5;

            if (temp < 0)
            {
                return 0;
            }

            return HeightAutoFit ? (double)value - 5 : ((double)value - 5)/2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
