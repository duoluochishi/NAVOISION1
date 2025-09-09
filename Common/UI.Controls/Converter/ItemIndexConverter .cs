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

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter;
public class ItemIndexConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || !(value is ListBoxItem))
        { 
            return 0; 
        }

        var listBox = ItemsControl.ItemsControlFromItemContainer((ListBoxItem)value) as ListBox;
        var isNotNull = (listBox is not null) && (listBox.ItemContainerGenerator is not null);
        return isNotNull ? (listBox.ItemContainerGenerator.IndexFromContainer((ListBoxItem)value) + 1) : 0;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}