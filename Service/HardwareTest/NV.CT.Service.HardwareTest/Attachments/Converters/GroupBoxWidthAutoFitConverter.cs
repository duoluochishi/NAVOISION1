using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.Attachments.Converters
{
    public class GroupBoxWidthAutoFitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && System.Convert.ToInt32(value) > 0) 
            {
                double divide6 = (double)value / 5 - 2;

                return System.Convert.ToInt32(divide6);
            }

            return 176;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
