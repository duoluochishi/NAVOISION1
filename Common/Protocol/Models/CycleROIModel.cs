using System.Xml.Serialization;

namespace NV.CT.Protocol.Models;

[Serializable]
public class CycleROIModel
{
    /// <summary>
    /// 圆心X坐标（单位：微米）
    /// </summary>
    [XmlAttribute]
    public int CenterX { get; set; }

    /// <summary>
    /// 圆心Y坐标（单位：微米）
    /// </summary>
    [XmlAttribute]
    public int CenterY { get; set; }

    /// <summary>
    /// 圆心Z坐标（单位：微米）
    /// </summary>
    [XmlAttribute]
    public int CenterZ { get; set; }

    /// <summary>
    /// 圆半径（单位：微米）
    /// </summary>
    [XmlAttribute]
    public int Radius { get; set; }
}
