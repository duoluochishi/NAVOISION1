using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.Model
{
    /// <summary>
    /// 校准场景，
    /// 基于使用场景（比如，更换球管）来组织多个校准项目，实现一键运行多个校准项目
    /// </summary>
    [XmlRoot("CalibrationScenario")]
    public class CalibrationScenario : IName
    {
        [XmlAttribute("Name")]
        public string? Name { get; set; }

        [XmlAttribute("Description")]
        public string? Description { get; set; }

        [XmlAttribute("ID")]
        public string? ID { get; set; }

        public List<string>? CalibrationItemReferenceGroup { get; set; } = new List<string>();
    }

    /// <summary>
    /// 校准场景，
    /// 基于使用场景（比如，更换球管）来组织多个校准项目，实现一键运行多个校准项目
    /// </summary>
    [XmlRoot("Config")]
    public class CaliScenarioConfig
    {
        [XmlAttribute("AutoConfirmResult")]
        public bool AutoConfirmResult { get; set; }


        [XmlAttribute("DebugMode")]
        public bool DebugMode { get; set; }

        /// <summary>
        /// 散射探测器的增益模式，默认16pc（不区分大小写）
        /// </summary>
        [XmlAttribute("ScatteringDetectorGain")]
        public string ScatteringDetectorGain { get; set; }

        public List<CalibrationScenario>? CalibrationScenarioGroup { get; set; } = new();
    }
}
