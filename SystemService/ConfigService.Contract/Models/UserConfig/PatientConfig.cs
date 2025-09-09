//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/25 10:05:13     V1.0.0       张震
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.UserConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PatientConfig", IsNullable = false)]
public class PatientConfig : BaseConfig
{
    [XmlElement("DisplayItems")]
    public DisplayItems DisplayItems { get; set; }
    [XmlElement("ChildrenAge")]
    public ChildrenAge ChildrenAgeLimit { get; set; }
    [XmlElement("RefreshTime")]
    public RefreshTime RefreshTimeInterval { get; set; }
    [XmlElement("PatientIdConfig")]
    public PatientIdConfig PatientIdConfig { get; set; }

    [XmlElement("PatientQueryConfig")]
    public PatientQueryConfig PatientQueryConfig { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("DisplayItems", IsNullable = false)]
public class DisplayItems
{
    [XmlElement("Item")]
    public List<PatientItem> Items { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("Item", IsNullable = false)]
public class PatientItem
{
    [XmlAttribute]
    public string ItemName { get; set; }

    [XmlAttribute]
    public bool CheckState { get; set; }

    [XmlAttribute]
    public bool IsFixed { get; set; }

    public bool IsEnabled { 
        get {
            return !IsFixed;
        } 
    }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("ChildrenAge", IsNullable = false)]
public class ChildrenAge
{
    [XmlAttribute]
    public int UpperLimit { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("RefreshTime", IsNullable = false)]
public class RefreshTime
{
    [XmlAttribute]
    public int Interval { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PatientIdConfig", IsNullable = false)]
public class PatientIdConfig
{
    [XmlAttribute]
    public string Prefix { get; set; }

    [XmlAttribute]
    public string Infix { get; set; }

    [XmlAttribute]
    public SuffixType SuffixType { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PatientQueryConfig", IsNullable = false)]
public class PatientQueryConfig
{
    [XmlAttribute]
    public SearchTimeType PatientQueryTimeType { get; set; }
}