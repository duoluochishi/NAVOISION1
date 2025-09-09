
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/31 09:23:59     V1.0.0       张震
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NV.CT.CTS.Enums;
using System.ComponentModel;

namespace NV.CT.ConfigService.Models.SystemConfig;

//[Serializable]
//public class HardwareConfig : BaseConfig
//{
//    public List<HeatCapacityItem> TubeHeatCapacity { get; set; }
//    public TableItem TableParameter { get; set; }
//}

//[Serializable]
//public class HeatCapacityItem
//{
//    [JsonProperty("Name")]
//    public string Name { get; set; }

//    [JsonProperty("Min")]
//    public int Min { get; set; }

//    [JsonProperty("Max")]
//    public int Max { get; set; }

//    [JsonProperty("Color")]
//    public string Color { get; set; }

//    [JsonProperty("Enable")]
//    public bool Enable { get; set; }
//}

[Serializable]
public class TableItem
{
    [JsonProperty("TableLength")]
    public uint TableLength { get; set; }

    [JsonProperty("MinVerticalHeight")]
    public uint MinVerticalHeight { get; set; }

    [JsonProperty("MaxVerticalHeight")]
    public uint MaxVerticalHeight { get; set; }

    [JsonProperty("LimitVerticalHeight")]
    public uint LimitVerticalHeight { get; set; }

    [JsonProperty("TableFeed")]
    public uint TableFeed { get; set; }
}