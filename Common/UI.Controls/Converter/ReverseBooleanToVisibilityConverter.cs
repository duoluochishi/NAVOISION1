//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/9/29 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter
{
    public class ReverseBooleanToVisibilityConverter : BasicsConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
    }
}
