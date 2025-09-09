using NV.MPS.Environment;
using System;
using System.IO;

namespace NV.CT.Service.Common
{
    public class ConfigPathService
    {
        private static readonly Lazy<ConfigPathService> _instance =
            new Lazy<ConfigPathService>(() => new ConfigPathService());
        public static ConfigPathService Instance => _instance.Value;
        private ConfigPathService()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleName">模块名字,与文件夹对应</param>
        /// <param name="configName">配置文件名称</param>
        /// <returns></returns>
        public string GetConfigPath(string moduleName, string configName)
        {
            return Path.Combine(RuntimeConfig.Console.MCSConfig.Path, moduleName, configName);
        }
    }
}
