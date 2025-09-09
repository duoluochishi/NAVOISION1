using System;
using System.Collections.Generic;
using System.IO;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Upgrade.DAL.Interfaces;
using NV.CT.Service.Upgrade.Models;
using NV.CT.Service.Upgrade.Utilities;
using NV.MPS.Environment;

namespace NV.CT.Service.Upgrade.DAL.Implements
{
    internal class ConfigFAO : IConfigFAO
    {
        #region Path

        private readonly string _configPath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "FWUpgrade", "Config.json").Replace('/', '\\');
        private readonly string _typesPath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "FWUpgrade", "Types.json").Replace('/', '\\');
        private readonly string _itemsPath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "FWUpgrade", "Items.json").Replace('/', '\\');
        private readonly string _filesPath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "FWUpgrade", "Files.json").Replace('/', '\\');
        private readonly ILogService _logService;

        #endregion

        public ConfigFAO(ILogService logService)
        {
            _logService = logService;
        }

        public UpgradeConfigModel GetConfig()
        {
            if (!File.Exists(_configPath))
            {
                _logService.Error(ServiceCategory.UpgradeFirmware, $"Configuration File Not Exist: {_configPath}");
                return new UpgradeConfigModel();
            }

            var content = File.ReadAllText(_configPath);
            var res = SerializeUtility.JsonDeserialize<UpgradeConfigModel>(content) ?? new UpgradeConfigModel();
            return res;
        }

        public IEnumerable<FirmwareTypeModel> GetFwTypes()
        {
            if (!File.Exists(_typesPath))
            {
                _logService.Error(ServiceCategory.UpgradeFirmware, $"Configuration File Not Exist: {_typesPath}");
                return Array.Empty<FirmwareTypeModel>();
            }

            var content = File.ReadAllText(_typesPath);
            var res = SerializeUtility.JsonDeserialize<FirmwareTypeModel[]>(content) ?? Array.Empty<FirmwareTypeModel>();
            return res;
        }

        public IEnumerable<FirmwareModel> GetFws()
        {
            if (!File.Exists(_itemsPath))
            {
                _logService.Error(ServiceCategory.UpgradeFirmware, $"Configuration File Not Exist: {_itemsPath}");
                return Array.Empty<FirmwareModel>();
            }

            var content = File.ReadAllText(_itemsPath);
            var res = SerializeUtility.JsonDeserialize<FirmwareModel[]>(content) ?? Array.Empty<FirmwareModel>();
            return res;
        }

        public IEnumerable<UpgradeFileModel> GetFwCanUpgradeFiles()
        {
            if (!File.Exists(_filesPath))
            {
                _logService.Error(ServiceCategory.UpgradeFirmware, $"Configuration File Not Exist: {_filesPath}");
                return Array.Empty<UpgradeFileModel>();
            }

            var content = File.ReadAllText(_filesPath);
            var res = SerializeUtility.JsonDeserialize<UpgradeFileModel[]>(content) ?? Array.Empty<UpgradeFileModel>();
            return res;
        }
    }
}