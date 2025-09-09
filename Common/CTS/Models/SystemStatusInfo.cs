//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/20 15:33:51           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.CTS.Models;

public class SystemStatusInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public SystemStatus Status { get; set; }

    public string ScanId { get; set; } = string.Empty;

    public RealDoseInfo DoseInfo { get; set; } = new();
}
