//-----------------------------------------------------------------------
// <copyright file="ArchiveTaskStatusConverter.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------
// <summary>
//     修改日期                  版本号       创建人     
// 2021/10/13 10:05:38           V0.0.1       liujian     
// </summary>
// ---------------------------------------------------------------------
using NV.CT.CTS.Enums;
using System;
using System.Globalization;

namespace NV.CT.UI.Controls.Converter;

public class ArchiveTaskStatusConverter : BasicsConverter
{
    private const string PROCESSING = "Processing";
    private const string SUCCEED = "Succeed";
    private const string CANCELLED = "Cancelled";
    private const string FAILED = "Failed";

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            var status = (ArchiveConverterStatus)intValue;
            var resultStatus = string.Empty;
            switch (status)
            {
                case ArchiveConverterStatus.Processing:
                    resultStatus = PROCESSING;
                    break;
                case ArchiveConverterStatus.Succeed:
                    resultStatus = SUCCEED;
                    break;
                case ArchiveConverterStatus.Cancelled:
                    resultStatus = CANCELLED;
                    break;
                default:
                    resultStatus = FAILED;
                    break;
            }
            return resultStatus;
        }
        else
        {
            return null;
        }
    }
}