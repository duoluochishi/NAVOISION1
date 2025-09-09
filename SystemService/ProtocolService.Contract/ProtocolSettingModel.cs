//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NV.CT.CTS.Enums;

namespace NV.CT.ProtocolService.Contract;

[Serializable]
public class ProtocolSettingModel
{
    public string DefaultEmergencyProtocol { get; set; }
    public string EmergencyProtocol { get; set; }

    public List<ProtocolSettingItem> Items { get; set; }
}

[Serializable]
public class ProtocolSettingItem
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string OtherBodyPart { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public BodyPart BodyPart { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public BodySize BodySize { get; set; }
}