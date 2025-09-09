//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/12/18 11:35:43     V1.0.0       an.hu
// </summary>

using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.UserConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PrintProtocols", IsNullable = false)]
public class PrintProtocolConfig : BaseConfig
{
    [XmlElement("PrintProtocol")]
    public ObservableCollection<PrintProtocol> PrintProtocols { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PrintProtocol", IsNullable = false)]
public class PrintProtocol
{
    [XmlAttribute]
    public string Id { get; set; }
    [XmlAttribute]
    public string Name { get; set; }
    [XmlAttribute]
    public string BodyPart { get; set; }
    [XmlAttribute]
    public bool IsSystem { get; set; }
    [XmlAttribute]
    public bool IsDefault { get; set; }
    [XmlAttribute]
    public int Row { get; set; }
    [XmlAttribute]
    public int Column { get; set; }

}
