//-----------------------------------------------------------------------
// <copyright file="ExportedStatusStyleConverter.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------
// <summary>
//     修改日期                  版本号       创建人     
//   2024/03/22 10:05:38         V0.0.1        胡安     
// </summary>
// ---------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter;

public class ExportedStatusStyleConverter : BasicsConverter
{
    private const string IS_EXPORTED_STATUS = "RightPath";

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool valueBool)
        {
            return valueBool ? (Style)Application.Current.Resources[IS_EXPORTED_STATUS] : null;
        }
        else
        {
            return null;
        }
    }
}