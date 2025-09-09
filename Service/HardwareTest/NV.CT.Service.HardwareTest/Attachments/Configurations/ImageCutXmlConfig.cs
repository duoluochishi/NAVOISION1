using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Options;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.MPS.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace NV.CT.Service.HardwareTest.Share.Fields
{
    public class ImageCutXmlConfig
    {
        private readonly ILogService logger;
        private readonly HardwareTestConfigOptions hardwareTestOptions;
        
        public static ImageCutXmlConfig Instance { get; } = new ImageCutXmlConfig();

        private ImageCutXmlConfig()
        {
            /** 获取备用配置文件路径 **/
            this.hardwareTestOptions = Ioc.Default.GetRequiredService<IOptions<HardwareTestConfigOptions>>().Value;
            /** 获取logService **/
            this.logger = Ioc.Default.GetRequiredService<ILogService>();
            /** 读配置文件 **/
            this.ReadConfigXmlFile();
        }

        /** 配置文件路径 **/
        public string XmlConfigFilePath => 
            Path.Combine(RuntimeConfig.Console.MCSConfig.Path, ComponentDefaults.HardwareTest, "nvImgCut.xml");
        /** 中心位置信息 **/
        public List<SourceNum> CenterInfos { get; set; } = null!;

        private void ReadConfigXmlFile() 
        {
            /** 获取路径 **/
            string configFilePath = File.Exists(XmlConfigFilePath) ? XmlConfigFilePath : hardwareTestOptions.ImageCutXmlConfigPath;
            /** 初始化 **/
            var serializer = new XmlSerializer(typeof(SourceCenterInfos));
            /** 读取配置 **/
            try
            {
                /** StreamReader **/
                using StreamReader reader = new StreamReader(configFilePath);
                /** 反序列化 **/
                var centerInfos = serializer.Deserialize(reader) as SourceCenterInfos;
                /** 校验 **/
                if (centerInfos is null) 
                {
                    logger.Error(ServiceCategory.HardwareTest,
                        $"[ImageCutXmlConfig] The image cut xml config file is null, path: {configFilePath}");
                    return;
                }
                /** 更新 **/
                this.CenterInfos = centerInfos.Sources;
            }
            catch (Exception ex)
            {
                logger.Error(ServiceCategory.HardwareTest, 
                    $"[ImageCutXmlConfig] Fails to read image cut xml config file, path: {configFilePath}, [Stack]: {ex}");
            }
        }
    }

    [XmlRootAttribute("nvConfig")]
    public class SourceCenterInfos
    {
        public SourceCenterInfos()
        {
            this.Sources = new();
        }

        [XmlArrayAttribute("nvImgCut")]
        public List<SourceNum> Sources { get; set; }
    }

    public class SourceNum
    {
        public int No { get; set; }
        public float CenterX { get; set; }
        public float CenterZ { get; set; }
        public int Width { get; set; }
    }

}
