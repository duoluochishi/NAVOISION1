//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/28 15:35:43     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.SystemConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("Errors", IsNullable = false)]
public class ErrorConfig : BaseConfig
{
    [XmlElement("ErrorInfo")]
    public List<ErrorInfo> Errors { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("ErrorInfo", IsNullable = false)]
public class ErrorInfo
{
    [XmlAttribute]
    public string Code { get; set; }

    [XmlAttribute]
    public string Level { get; set; }

    [XmlAttribute]
    public string Module { get; set; }

    [XmlAttribute]
    public string Description { get; set; }

    [XmlAttribute]
    public string Reason { get; set; }

    [XmlAttribute]
    public string Solution { get; set; }

    [XmlAttribute]
    public string Exception { get; set; }
}
