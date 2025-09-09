using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NV.CT.DmsTest.Model
{
    [XmlRoot("DmsTestScanParam")]
    public class DmsTestScanParam
    {
        [XmlElement("Kv")]
        public uint Kv { get; set; }
        [XmlElement("Ma")]
        public uint Ma { get; set; }
        [XmlElement("FrameTime")]
        public uint FrameTime { get; set; }
        [XmlElement("ExpTime")]
        public uint ExpTime { get; set; }
        [XmlElement("FramesPerCycle")]
        public uint FramesPerCycle { get; set; }
        [XmlElement("Cycles")]
        public uint Cycles { get; set; }
        [XmlElement("ExposureMode")]
        public ExposureMode ExposureMode { get; set; }
        [XmlElement("Gain")]
        public Gain Gain { get; set; }

        [XmlElement("ScanLen")]
        public uint ScanLen { get; set; }
    }
    [XmlRoot("ScanGroup")]
    public class ScanGroup
    {
        [XmlElement("Id")]
        public int Id { get; set; }
        [XmlElement("ScanParam")]
        public DmsTestScanParam ?Param { get; set; }

        [XmlElement("Times")]
        public int Times  { get; set; }
    }
    [XmlRoot("TestItem")]
    public class TestItem
    {
        [XmlElement("ItemName")]
        public string ?ItemName  { get; set; }
        [XmlElement("ScanGroup")]
        public List<ScanGroup> ?GroupList { get; set; }
        [XmlElement("TestDataRootPath")]
        public string ?TestDataRootPath { get; set; }
    }
}
