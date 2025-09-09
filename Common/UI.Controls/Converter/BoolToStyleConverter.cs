using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class BoolToStyleConverter : BasicsConverter
    {
        /// <summary>
        /// 转换bool类型为Style
        /// </summary>
        /// <param name="value">bool 值，true 或 false</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">接收格式必须是'{style name when true}:{style name when false }'</param>
        /// <param name="culture"></param>
        /// <returns>Style?</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null || parameter.ToString().IndexOf(":") < 0)
            {
                return null;
            }

            var styles = parameter.ToString().Split(":");
            if (styles?.Length != 2)
            {
                return null;
            }

            if (value is bool boolValue)
            {
                var styleNameWhenTrue = styles[0];
                var styleNameWhenFalse = styles[1];
                return boolValue ? (Style)Application.Current.Resources[styleNameWhenTrue] : (Style)Application.Current.Resources[styleNameWhenFalse];
            }
            else
            {
                return null;
            }
        }
    }
}
