using NV.CT.FacadeProxy.Common.Helpers;
using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.Model
{
    /// <summary>
	/// 校准协议，一组参数作为一个协议的方式出现
	/// 其中，可以包含多个处理流程（扫描采集，生成校准表）
	/// </summary>
	public class CalibrationProtocol : IName
    {
        [XmlAttribute("ID")]
        public string? ID { get; set; }

        [XmlAttribute("Name")]
        public string? Name { get; set; }

        [XmlAttribute("Study")]
        public string? Study { get; set; } = "Default";

        /// <summary>
        /// 处理流程集合
        /// </summary>
        public List<Handler>? HandlerGroup { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializeHelper.ToJson(this);
        }
    }
}
