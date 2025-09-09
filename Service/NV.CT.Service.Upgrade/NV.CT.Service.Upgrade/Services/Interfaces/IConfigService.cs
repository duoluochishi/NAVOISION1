using System.Collections;
using System.Collections.Generic;
using NV.CT.Service.Upgrade.Models;

namespace NV.CT.Service.Upgrade.Services.Interfaces
{
    public interface IConfigService
    {
        UpgradeConfigModel GetConfig();
        IEnumerable<FirmwareTypeModel> GetFwTypes();
        IEnumerable<FirmwareModel> GetFws();
        IEnumerable<UpgradeFileModel> GetFwCanUpgradeFiles();
    }
}
