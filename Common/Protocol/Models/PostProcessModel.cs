using NV.CT.FacadeProxy.Common.Enums.PostProcessEnums;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models;

[Serializable]
public class PostProcessModel
{
    [XmlAttribute("Index")]
    public int Index { get; set; }

    [XmlAttribute("Type")]
    public PostProcessType Type { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

}
