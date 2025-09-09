using Newtonsoft.Json;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    [XmlRoot(ElementName = "ProtocolTemplate", Namespace = null)]
    public class ProtocolTemplateModel : BaseModel
    {
        public ProtocolModel Protocol { get; set; }

        [XmlIgnore, JsonIgnore]
        public bool IsFactory { get => Protocol.IsFactory; }

        [XmlIgnore, JsonIgnore]
        public bool IsAdult { get => Protocol.IsAdult;}

        [XmlIgnore, JsonIgnore]
        public CTS.Enums.BodyPart BodyPart { get => Protocol.BodyPart; }

        [XmlIgnore, JsonIgnore]
        public bool IsEnhanced { get => Protocol.IsEnhanced; }

        [XmlIgnore, JsonIgnore]
        public bool IsEmergency { get => Protocol.IsEmergency; }

        [XmlIgnore, JsonIgnore]
        public bool IsIntervention { get => Protocol.IsIntervention; }

        [XmlIgnore, JsonIgnore]
        public bool IsWarning { get => Protocol.IsValid; }       

        [XmlIgnore]
        public string FullPath { get; set; }

        [XmlIgnore]
        public string FileName { get; set; }
    }
}
