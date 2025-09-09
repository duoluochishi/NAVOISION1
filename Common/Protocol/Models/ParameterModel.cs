using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    public class ParameterModel
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Min { get; set; }

        [XmlAttribute]
        public string Max { get; set; }

        [XmlAttribute]
        public string Acceptance { get; set; }

        [XmlAttribute]
        public List<string> Items { get; set; }

        public bool Validate()
        {
            return true;
        }

        public bool ShouldSerializeType()
        {
            return !string.IsNullOrEmpty(Type);
        }

        public bool ShouldSerializeMin()
        {
            return !string.IsNullOrEmpty(Min);
        }

        public bool ShouldSerializeMax()
        {
            return !string.IsNullOrEmpty(Max);
        }

        public bool ShouldSerializeAcceptance()
        {
            return !string.IsNullOrEmpty(Acceptance);
        }

        public bool ShouldSerializeItems()
        {
            return Items is not null && Items.Count > 0;
        }
    }
}
