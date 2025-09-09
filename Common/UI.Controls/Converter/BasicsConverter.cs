//-----------------------------------------------------------------------
// <copyright file="BasicsConverter.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------
// <summary>
//     修改日期                  版本号       创建人     
// 2020/5/22 13:41:09               V0.0.1                   liujian     
// </summary>
// ---------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;

namespace NV.CT.UI.Controls.Converter;
public class BasicsConverter : IValueConverter
{
    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }

    public  virtual  object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
