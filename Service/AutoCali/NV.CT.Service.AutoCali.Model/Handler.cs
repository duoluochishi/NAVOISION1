using NV.CT.FacadeProxy.Common.Helpers;
using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.Model
{
    public class Handler : IName
    {
        [XmlAttribute("ID")]
        public string? ID { get; set; }

        [XmlAttribute("Name")]
        public string? Name { get; set; }

        [XmlAttribute("Type")]
        public string? Type { get; set; }

        [XmlAttribute("Description")]
        public string? Description { get; set; }

        [XmlAttribute("Study")]
        public string? Study { get; set; } = "Default";

        /// <summary>
        /// 参数集合
        /// </summary>
        public List<Parameter>? Parameters { get; set; } = new();

        //不要设置XmlElement，否则节点将以 PreHandlers，PreHandlers，平铺
        //期望结构：PreHandlers - Handler
        public List<Handler>? PreHandlers { get; set; }

        public List<Handler>? PostHandlers { get; set; }

        public override string ToString()
        {
            return JsonSerializeHelper.ToJson(this);
        }
    }
}
