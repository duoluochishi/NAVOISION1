using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.Service.Common.Converters
{
    /// <summary>
    /// 此Converter是为了解决绑定float、double、decimal等小数类型，但Binding.UpdateSourceTrigger=PropertyChanged时，无法输入小数点的问题。
    /// <para>1) 如果Binding.UpdateSourceTrigger=LostFocus，如上所述，使用此Converter没有什么意义。</para>
    /// <para>2) 建议使用此Converter的同时，设置TextBoxAttachedProperty.InputType附加属性。</para>
    /// <para>3) 如果单独使用此Converter，即未同时设置TextBoxAttachedProperty.InputType附加属性，会有坏效果，当结尾为"."时，失去焦点并不会自动处理掉结尾的“.”，请务必配合着自行处理LostFocus事件。</para>
    /// </summary>
    public class DecimalPointConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var str = value?.ToString();

            if (string.IsNullOrWhiteSpace(str))
            {
                return Activator.CreateInstance(targetType);
            }

            return str.EndsWith('.') ? Binding.DoNothing : value;
            // return str.EndsWith('.') ? Binding.DoNothing : System.Convert.ChangeType(value, targetType, culture);
        }
    }
}