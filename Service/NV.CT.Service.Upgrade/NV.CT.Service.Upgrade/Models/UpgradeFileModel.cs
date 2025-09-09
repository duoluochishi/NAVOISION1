using System.Collections.Generic;
using NV.CT.FacadeProxy.Models.Upgrade;

namespace NV.CT.Service.Upgrade.Models
{
    public class UpgradeFileModel
    {
        public FirmwareType FirmwareType { get; set; }
        public List<string> Files { get; set; } = new List<string>();
    }
}