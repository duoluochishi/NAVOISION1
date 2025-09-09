using System.Collections.Generic;
using NV.CT.Service.Upgrade.Models;

namespace NV.CT.Service.Upgrade.DAL.Interfaces
{
    public interface IConfigFAO
    {
        UpgradeConfigModel GetConfig();
        IEnumerable<FirmwareTypeModel> GetFwTypes();
        IEnumerable<FirmwareModel> GetFws();
        IEnumerable<UpgradeFileModel> GetFwCanUpgradeFiles();
    }
}