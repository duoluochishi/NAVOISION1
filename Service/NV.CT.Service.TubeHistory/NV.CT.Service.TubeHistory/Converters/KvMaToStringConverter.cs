using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.TubeHistory.Converters
{
    internal class KvMaToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is uint[] kV)
            {
                return string.Join(", ", kV);
            }

            if (value is double[] mA)
            {
                return string.Join(", ", mA);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}