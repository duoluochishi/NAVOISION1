using NV.CT.FacadeProxy.Common.Helpers;
using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.Model
{
    /// <summary>
    /// 计算任务执行在那台机器上
    /// </summary>
    public enum ComputingMachineType
    {
        OfflineMachine,
        MasterMachine
    }

    public interface IName
    {
        string? Name { get; }
    }
    /// <summary>
    /// 校准项目
    /// </summary>
    [XmlRoot("CalibrationItem")]
    public class CalibrationItem : IName
    {        
        /// <summary>
        /// 校准项类型
        /// 区别于校准项名称，校准项类型是固定的
        /// </summary>
        [XmlAttribute("CalibrationType")]
        public string? CalibrationType { get; set; }

        /// <summary>
        /// 校准项目名称
        /// </summary>
        [XmlAttribute("Name")]
        public string? Name { get; set; }

        [XmlAttribute("Description")]
        public string? Description { get; set; }

        [XmlAttribute("ID")]
        public string? ID { get; set; }

        /// <summary>
        /// 校准项目的注意事项
        /// </summary>
        [XmlAttribute("Attention")]
        public string? Attention { get; set; }

        /// <summary>
        /// 校准项目的计算资源类型：离线即（默认） / 主控机
        /// </summary>
        [XmlElement]
        public ComputingMachineType? ComputingMachineType { get; set; }

        /// 校准协议，不同扫描参数组合确定一种校准协议，并以此命名校准表
        /// </summary>
        public List<CalibrationProtocol>? CalibrationProtocolGroup { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializeHelper.ToJson(this);
        }
    }
}
