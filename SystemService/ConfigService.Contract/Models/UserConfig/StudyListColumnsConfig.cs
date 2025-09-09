//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/30 10:05:13     V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.UserConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("StudyListColumnsConfig", IsNullable = false)]
public class StudyListColumnsConfig : BaseConfig
{
    [XmlElement("ColumnItems")]
    public ColumnItemList ColumnItems { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("ColumnItems", IsNullable = false)]
public class ColumnItemList
{
    [XmlElement("Item")]
    public List<ColumnItem> Items { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("Item", IsNullable = false)]
public class ColumnItem
{
    [XmlAttribute]
    public StudyListColumn ItemName { get; set; }

    [XmlAttribute]
    public bool IsFixed { get; set; }

    [XmlAttribute]
    public bool IsChecked { get; set; }

    [XmlAttribute]
    public int SortNumber { get; set; }
}
