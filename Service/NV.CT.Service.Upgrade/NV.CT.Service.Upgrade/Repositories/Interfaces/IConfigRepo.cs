using System.Collections.Generic;
using NV.CT.Service.Upgrade.Models;

namespace NV.CT.Service.Upgrade.Repositories.Interfaces
{
    public interface IConfigRepo
    {
        UpgradeConfigModel GetConfig();
        IEnumerable<FirmwareTypeModel> GetFwTypes();
        IEnumerable<FirmwareModel> GetFws();
        IEnumerable<UpgradeFileModel> GetFwCanUpgradeFiles();
    }
}
