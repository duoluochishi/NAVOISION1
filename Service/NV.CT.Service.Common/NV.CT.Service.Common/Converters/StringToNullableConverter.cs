using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.Common.Converters
{
    public class StringToNullableConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str
             && string.IsNullOrWhiteSpace(str)
             && targetType is { IsGenericType: true, IsGenericTypeDefinition: false }
             && ReferenceEquals(targetType.GetGenericTypeDefinition(), typeof(Nullable<>)))
            {
                return null;
            }

            return value;
        }
    }
}