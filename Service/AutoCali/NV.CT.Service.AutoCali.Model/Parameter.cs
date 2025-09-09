using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.Model
{
    public class Parameter
    {
        [XmlAttribute("Name")]
        public string? Name { get; set; }

        [XmlAttribute("Value")]
        public string? Value { get; set; }

        [XmlAttribute("Description")]
        public string? Description { get; set; }
        public override string ToString()
        {
            return $"{{{Name}:{Value}}}";
        }
    }
}
