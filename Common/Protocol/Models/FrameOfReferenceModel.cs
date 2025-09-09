using Newtonsoft.Json;
using NV.CT.FacadeProxy.Common.Enums;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    public class FrameOfReferenceModel : BaseModel<ProtocolModel, MeasurementModel>
    {
        [XmlArray("Measurements")]
        [XmlArrayItem("Measurement")]
        public override List<MeasurementModel> Children { get => base.Children; set => base.Children = value; }

        [XmlIgnore, JsonIgnore]
        public PatientPosition PatientPosition => GetParameterValue<PatientPosition>(ProtocolParameterNames.PATIENT_POSITION);

        [XmlIgnore, JsonIgnore]
        public bool IsEnhanced { get => Children.Any(measurement => measurement.IsEnhanced == true); }

        [XmlIgnore, JsonIgnore]
        public bool IsIntervention { get => Children.Any(measurement => measurement.IsIntervention == true); }
    }
}
