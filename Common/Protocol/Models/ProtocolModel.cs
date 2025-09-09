using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    [XmlRoot("Protocol")]
    public class ProtocolModel : BaseModel<ProtocolTemplateModel, FrameOfReferenceModel>
    {
        [XmlArray("FrameOfReferences")]
        [XmlArrayItem("FrameOfReference")]
        public override List<FrameOfReferenceModel> Children { get => base.Children; set => base.Children = value; }

        [XmlIgnore, JsonIgnore]
        public bool IsFactory => GetParameterValue<bool>(ProtocolParameterNames.PROTOCOL_IS_FACTORY, true);

        [XmlIgnore, JsonIgnore]
        public bool IsAdult
        {
            get
            {
                return BodySize == BodySize.Adult;
            }
        }

        [XmlIgnore, JsonIgnore]
        public BodySize BodySize => GetParameterValue<BodySize>(ProtocolParameterNames.BODY_SIZE);

        [XmlIgnore, JsonIgnore]
        public CTS.Enums.BodyPart BodyPart => GetParameterValue<CTS.Enums.BodyPart>(ProtocolParameterNames.BODY_PART);

        [XmlIgnore, JsonIgnore]
        public bool IsValid => GetParameterValue<bool>(ProtocolParameterNames.PROTOCOL_IS_VALID, true);

        [XmlIgnore, JsonIgnore]
        public string Description => GetParameterValue(ProtocolParameterNames.DESCRIPITON);

        [XmlIgnore, JsonIgnore]
        public string ProtocolFamily => GetParameterValue(ProtocolParameterNames.PROTOCOL_FAMILY);

        [XmlIgnore, JsonIgnore]
        public string ProtocolVersion => GetParameterValue(ProtocolParameterNames.PROTOCOL_VERSION);

        [XmlIgnore, JsonIgnore]
        public bool IsTabletSuitable => GetParameterValue<bool>(ProtocolParameterNames.PROTOCOL_TABLET_SUITABLE, true);

        [XmlIgnore]
        public bool IsEmergency { get; set; } = false;

        [XmlIgnore, JsonIgnore]
        public bool IsEnhanced { get => Children.Any(frame => frame.IsEnhanced == true); }

        [XmlIgnore, JsonIgnore]
        public bool IsIntervention { get => Children.Any(frame => frame.IsIntervention == true); }
    }
}
