using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    [XmlRoot("Descriptor")]
    public class DescriptorModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}
