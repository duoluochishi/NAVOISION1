using System.Collections.Generic;
using NV.CT.Service.Upgrade.Models;
using NV.CT.Service.Upgrade.Repositories.Interfaces;
using NV.CT.Service.Upgrade.Services.Interfaces;

namespace NV.CT.Service.Upgrade.Services.Implements
{
    internal class ConfigService : IConfigService
    {
        private readonly IConfigRepo _configRepo;

        public ConfigService(IConfigRepo configRepo)
        {
            _configRepo = configRepo;
        }

        public UpgradeConfigModel GetConfig()
        {
            return _configRepo.GetConfig();
        }

        public IEnumerable<FirmwareTypeModel> GetFwTypes()
        {
            return _configRepo.GetFwTypes();
        }

        public IEnumerable<FirmwareModel> GetFws()
        {
            return _configRepo.GetFws();
        }

        public IEnumerable<UpgradeFileModel> GetFwCanUpgradeFiles()
        {
            return _configRepo.GetFwCanUpgradeFiles();
        }
    }
}