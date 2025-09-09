using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    public class MeasurementModel : BaseModel<FrameOfReferenceModel,ScanModel>
    {
        [XmlArray("Scans")]
        [XmlArrayItem("Scan")]
        public override List<ScanModel> Children { get => base.Children; set => base.Children = value; }

        [XmlIgnore, JsonIgnore]
        public bool IsEnhanced { get => Children.Any(scan => scan.IsEnhanced == true); }

        [XmlIgnore, JsonIgnore]
        public bool IsIntervention { get => Children.Any(scan => scan.IsIntervention == true); }
    }
}
