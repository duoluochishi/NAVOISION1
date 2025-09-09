using NV.CT.FacadeProxy.Models.Upgrade;
using NV.CT.Service.Upgrade.Models;

namespace NV.CT.Service.Upgrade.Extensions
{
    internal static class FirmwareModelExtension
    {
        public static FirmwareInfo ToProto(this FirmwareModel item)
        {
            return new FirmwareInfo()
            {
                ID = item.ID,
                Num = item.Num,
                FirmwareType = item.FirmwareType,
                PackagePath = item.PackagePath,
                CanUpgradeFiles = item.CanUpgradeFiles,
            };
        }
    }
}