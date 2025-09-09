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
using NV.CT.CTS.Enums;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.UserConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "ViewTypes", IsNullable = false)]
public class ImageAnnotationConfig : BaseConfig
{
    [XmlElement(ElementName = "ScanTopo")]
    public ImageAnnotationSetting ScanTopoSettings { get; set; }

    [XmlElement(ElementName = "ScanTomo")]
    public ImageAnnotationSetting ScanTomoSettings { get; set; }

    [XmlElement(ElementName = "View")]
    public ImageAnnotationSetting ViewSettings { get; set; }

    [XmlElement(ElementName = "Print")]
    public ImageAnnotationSetting PrintSettings { get; set; }

    [XmlElement(ElementName = "MPR")]
    public ImageAnnotationSetting MPRSettings { get; set; }

    [XmlElement(ElementName = "VR")]
    public ImageAnnotationSetting VRSettings { get; set; }
}

[Serializable]
public class ImageAnnotationSetting
{
    [XmlElement(ElementName = "ViewSetting")]
    public List<AnnotationItem> AnnotationItemSettings { get; set; }

    [XmlAttribute]
    public short FontSize { get; set; }

    [XmlAttribute]
    public short FontColorR { get; set; }

    [XmlAttribute]
    public short FontColorG { get; set; }

    [XmlAttribute]
    public short FontColorB { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "ViewSetting", IsNullable = false)]
public class AnnotationItem
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public FourCornerTextSource TextSource { get; set; }

    [XmlAttribute]
    public string DicomTagGroup { get; set; }

    [XmlAttribute]
    public string DicomTagId { get; set; }
    [XmlAttribute]
    public string TextPrefix { get; set; }
    [XmlAttribute]
    public string TextSuffix { get; set; }
    [XmlAttribute]
    public bool Visibility { get; set; }
    [XmlAttribute]
    public int Row { get; set; }
    [XmlAttribute]
    public int Column { get; set; }
    [XmlAttribute]
    public FourCornersLocation Location { get; set; }
    [XmlAttribute]
    public string GroupName { get; set; }
}