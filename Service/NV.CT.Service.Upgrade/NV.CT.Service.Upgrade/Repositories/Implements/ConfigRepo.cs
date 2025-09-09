using System.Collections.Generic;
using NV.CT.Service.Upgrade.DAL.Interfaces;
using NV.CT.Service.Upgrade.Models;
using NV.CT.Service.Upgrade.Repositories.Interfaces;

namespace NV.CT.Service.Upgrade.Repositories.Implements
{
    internal class ConfigRepo : IConfigRepo
    {
        private readonly IConfigFAO _configFAO;

        public ConfigRepo(IConfigFAO configFAO)
        {
            _configFAO = configFAO;
        }

        public UpgradeConfigModel GetConfig()
        {
            return _configFAO.GetConfig();
        }

        public IEnumerable<FirmwareTypeModel> GetFwTypes()
        {
            return _configFAO.GetFwTypes();
        }

        public IEnumerable<FirmwareModel> GetFws()
        {
            return _configFAO.GetFws();
        }

        public IEnumerable<UpgradeFileModel> GetFwCanUpgradeFiles()
        {
            return _configFAO.GetFwCanUpgradeFiles();
        }
    }
}