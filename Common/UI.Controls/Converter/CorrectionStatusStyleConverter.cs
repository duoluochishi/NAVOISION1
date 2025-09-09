//-----------------------------------------------------------------------
// <copyright file="CorrectionStatusStyleConverter.cs" company="纳米维景">
// 版权所有 (C)2024,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------
// <summary>
//     修改日期                  版本号       创建人     
//   2024/05/11 10:05:38         V0.0.1        胡安     
// </summary>
// ---------------------------------------------------------------------
using NV.CT.CTS.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace NV.CT.UI.Controls.Converter;

public class CorrectionStatusStyleConverter : BasicsConverter
{
    private const string STYLE_RIGHT = "RightPath";

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            var status = (CorrectStatus)intValue;
            return status == CorrectStatus.Corrected ? (Style)Application.Current.Resources[STYLE_RIGHT] : null;
        }
        else
        {
            return null;
        }
    }
}