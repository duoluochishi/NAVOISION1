using System.Linq;
using System.Xml.Serialization;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    [XmlRoot("DetectorBoardsUpdateHistory")]
    public class DetectBoardsUpdateHistory 
    {
        public DetectBoardsUpdateHistory()
        {
            DetectBoardSources = 
                Enumerable.Range(0, 64).Select(i => new DetectBoardSource()).ToArray();
        }

        [XmlArray("DetectBoards")]
        [XmlArrayItem("DetectBoard")]
        public DetectBoardSource[] DetectBoardSources { get; set; }
    }

}
