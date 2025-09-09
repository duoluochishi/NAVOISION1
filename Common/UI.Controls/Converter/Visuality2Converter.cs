//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------
// <summary>
//     修改日期                  版本号       创建人     
// 2020/5/21 15:42:21               V0.0.1                   longping.li     
// </summary>
// ---------------------------------------------------------------------

using NV.CT.CTS.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter;

public class Visuality2Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isCollapsed = (value is not null && int.TryParse(value.ToString(), out var visibility) && visibility == (int)VisibilityStatus.Collapsed);
        return isCollapsed ? VisibilityStatus.Collapsed.ToString() : VisibilityStatus.Visible.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return 0;
    }
}