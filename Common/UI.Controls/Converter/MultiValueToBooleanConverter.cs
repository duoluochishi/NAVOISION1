using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter
{
    /// <summary>
    /// 多源值转换为Boolean，必须所有的源值都为true，才返回true
    /// </summary>
    public class MultiValueToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length == 0)
            {
                return false;
            }

            var hasFalseValue = values.Any(v => (v is not bool) || (((bool)v) is false));
            return !hasFalseValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
