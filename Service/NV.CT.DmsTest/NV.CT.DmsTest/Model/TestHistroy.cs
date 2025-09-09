using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Serialization;

namespace NV.CT.DmsTest.Model
{
    public class TestHistory
    {
        [XmlAttribute("TestUser")]
        public string ?TestUser { get; set; }
        [XmlAttribute("TestTime")]
        public string ?TestTime { get; set; }

        [XmlAttribute("TestItem")]
        public TestItemInfo ?ItemInfo { get; set; }
    }
}
