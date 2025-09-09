using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.Util;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NV.CT.Service.AutoCali.DAL
{
    public interface ICaliItemDAO
    {
        IList<CalibrationItem> Get();
        void Save(IList<CalibrationItem> caliItems);
    }

    public class CaliItemDAOImpl : ICaliItemDAO
    {
        public IList<CalibrationItem> Get()
        {
            throw new NotImplementedException();
        }

        public void Save(IList<CalibrationItem> caliItems)
        {
            throw new NotImplementedException();
        }
    }

    public class CaliItemDAO_FileImpl : ICaliItemDAO
    {
        private static readonly string ConfigFileName = "CalibrationItemConfig.xml";

        private static string ConfigFilePath;

        static CaliItemDAO_FileImpl()
        {
            string root = System.AppDomain.CurrentDomain.BaseDirectory;
            ConfigFilePath = Path.Combine(root, ConfigFileName);
            Console.WriteLine($"Set the path:{ConfigFilePath}");
        }

        public IList<CalibrationItem> Get()
        {
            return XmlUtil.DeserializeFromFile<List<CalibrationItem>>(ConfigFilePath);
        }

        /// <summary>
        /// CaliProject, CaliProtocol, CaliProcess
        /// </summary>
        private void TestNewDesigin()
        {

            var projectList = new List<CaliProjectDto>();

            var project = new CaliProjectDto();
            project.Name = "校准项目";
            project.PropertyItemGroup = new PropertyItemGroupDto();

            var propertyItem = new PropertyItemDto();
            propertyItem.Name = "Attention";
            propertyItem.Value = "项目提示";
            project.PropertyItemGroup.SubPropertyItemGroup.Add(propertyItem);

            var aCaliProtocol = new CaliProtocolDto();
            aCaliProtocol.Name = "校准协议#2";
            aCaliProtocol.PropertyItemGroup = new PropertyItemGroupDto();
            propertyItem = new PropertyItemDto();
            propertyItem.Name = "KV";
            propertyItem.Value = "100";
            aCaliProtocol.PropertyItemGroup.SubPropertyItemGroup.Add(propertyItem);

            var aCaliProcess = new CaliProcessDto();
            aCaliProcess.Name = "扫描#1";
            aCaliProcess.ProcessType = "Scan";

            aCaliProcess.PropertyItemGroup = new PropertyItemGroupDto();
            propertyItem = new PropertyItemDto();
            propertyItem.Name = "Attention";
            propertyItem.Value = "扫描#1--提示";
            aCaliProcess.PropertyItemGroup.SubPropertyItemGroup.Add(propertyItem);

            var propertyItemGroup = new PropertyItemGroupDto();
            aCaliProcess.PropertyItemGroup.SubPropertyItemGroupList.Add(propertyItemGroup);

            propertyItemGroup.Name = "Scan";
            propertyItem = new PropertyItemDto();
            propertyItem.Name = "KV";
            propertyItem.Value = "120";
            propertyItemGroup.SubPropertyItemGroup.Add(propertyItem);

            propertyItem = new PropertyItemDto();
            propertyItem.Name = "MA";
            propertyItem.Value = "100";
            propertyItemGroup.SubPropertyItemGroup.Add(propertyItem);

            propertyItem = new PropertyItemDto();
            propertyItem.Name = "Frames";
            propertyItem.Value = "485";
            propertyItemGroup.SubPropertyItemGroup.Add(propertyItem);


            //< PropertyItemGroup >
            ////              <PropertyItem Name = "Attention" Value="扫描#1--提示"/>

            ////              <PropertyItemGroup Name = "Scan" >
            ////                < PropertyItem Name="KV" Value="100"/>
            ////                <PropertyItem Name = "MA" Value="100"/>
            ////                <PropertyItem Name = "Frames" Value="485"/>
            ////              </PropertyItemGroup>

            aCaliProtocol.CaliProcessGroup.Add(aCaliProcess);
            project.CaliProtocolGroup.Add(aCaliProtocol);
            projectList.Add(project);

            string projectData = XmlUtil.SerializeXml(projectList);


            var listCaliProject = XmlUtil.DeserializeFromFile<List<CaliProjectDto>>(
                "D:\\Code3\\MCS\\Service\\AutoCali\\NV.CT.Service.AutoCali.DAL\\CalibrationProjectConfig.xml");

            projectData = XmlUtil.SerializeXml(listCaliProject);
        }

        public void Save(IList<CalibrationItem> caliItems)
        {
            XmlUtil.SaveToFile(ConfigFilePath, caliItems);
        }
    }

    //<CaliProjectGroup>

    //  <CaliProject Name = "硬化校准" >
    //    < PropertyItemGroup >
    //      < PropertyItem Name="Attention" Value="项目提示"/>
    //    </PropertyItemGroup>

    //    <CaliProtocolGroup>
    //      <CaliProtocol Name = "校准协议#2" >
    //        < PropertyItemGroup >
    //          < PropertyItem Name="KV" Value="100"/>
    //        </PropertyItemGroup>

    //        <CaliProcessGroup>
    //          <CaliProcess Name = "扫描#1" ProcessType="Scan">

    //            <PropertyItemGroup>
    //              <PropertyItem Name = "Attention" Value="扫描#1--提示"/>

    //              <PropertyItemGroup Name = "Scan" >
    //                < PropertyItem Name="KV" Value="100"/>
    //                <PropertyItem Name = "MA" Value="100"/>
    //                <PropertyItem Name = "Frames" Value="485"/>
    //              </PropertyItemGroup>

    //              <PropertyItemGroup Name = "Recon" >
    //                < PropertyItem Name="FOV" Value="10"/>
    //              </PropertyItemGroup>
    //            </PropertyItemGroup>
    //          </CaliProcess>

    [XmlRoot("CaliProject")]
    public class CaliProjectDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("CaliProtocolGroup")]
        public List<CaliProtocolDto> CaliProtocolGroup { get; set; }= new List<CaliProtocolDto>();

        [XmlElement("PropertyItemGroup")]
        public PropertyItemGroupDto PropertyItemGroup { get; set; }
    }

    [XmlRoot("CaliProtocol")]
    public class CaliProtocolDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("CaliProcessGroup")]
        public List<CaliProcessDto> CaliProcessGroup { get; set; }= new List<CaliProcessDto>();

        [XmlElement("PropertyItemGroup")]
        public PropertyItemGroupDto PropertyItemGroup { get; set; }
    }

    [XmlRoot("CaliProcess")]
    public class CaliProcessDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("ProcessType")]
        public string ProcessType { get; set; }

        [XmlElement("PropertyItemGroup")]
        public PropertyItemGroupDto PropertyItemGroup { get; set; }
    }

    [XmlRoot("PropertyItemGroup")]
    public class PropertyItemGroupDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        //[XmlElement("PropertyItemGroup")]
        public List<PropertyItemDto> SubPropertyItemGroup { get; set; } = new List<PropertyItemDto>();

        //[XmlElement("PropertyItemGroup")]
        public List<PropertyItemGroupDto> SubPropertyItemGroupList { get; set; } = new List<PropertyItemGroupDto>();
    }

    [XmlRoot("PropertyItem")]
    public class PropertyItemDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }
    }
}
