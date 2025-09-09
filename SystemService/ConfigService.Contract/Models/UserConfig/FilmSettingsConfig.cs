//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/07/05 11:35:43     V1.0.0       an.hu
// </summary>

using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.UserConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("FilmSettingsList", IsNullable = false)]
public class FilmSettingsConfig : BaseConfig
{
    [XmlElement("FilmSettings")]
    public ObservableCollection<FilmSettings> FilmSettingsList { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class FilmSettings
{
    [XmlAttribute]
    public string Id { get; set; }
    [XmlAttribute]
    public bool IsPortrait { get; set; }
    [XmlAttribute]
    public float NormalizedHeaderHeight { get; set; }
    [XmlAttribute]
    public float NormalizedFooterHeight { get; set; }

    [XmlElement("HeaderLogo")]
    public Logo HeaderLogo { get; set; }

    [XmlElement("FooterLogo")]
    public Logo FooterLogo { get; set; }

    [XmlElement("Headers")]
    public Cells HeaderCellsList { get; set; }

    [XmlElement("Footers")]
    public Cells FooterCellsList { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class Logo
{
    [XmlAttribute]
    public bool IsVisible { get; set; }
    [XmlAttribute]
    public float NormalizedLocationX { get; set; }
    [XmlAttribute]
    public float NormalizedLocationY { get; set; }
    [XmlAttribute]
    public float NormalizedWidth { get; set; }
    [XmlAttribute]
    public float NormalizedHeight { get; set; }
    [XmlAttribute]
    public string PicturePath { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class Cells
{
    [XmlElement("Cell")]
    public ObservableCollection<Cell> CellList { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public class Cell
{
    [XmlAttribute]
    public int RowIndex { get; set; }

    [XmlAttribute]
    public int ColumnIndex { get; set; }

    [XmlAttribute]
    public string Text { get; set; }

    [XmlAttribute]
    public int FontSize { get; set; }

    [XmlAttribute]
    public float NormalizedLocationX { get; set; }
    [XmlAttribute]
    public float NormalizedLocationY { get; set; }
    [XmlAttribute]
    public float NormalizedWidth { get; set; }
    [XmlAttribute]
    public float NormalizedHeight { get; set; }

}
