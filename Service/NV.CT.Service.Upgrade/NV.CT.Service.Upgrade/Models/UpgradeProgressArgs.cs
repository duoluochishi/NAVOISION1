using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.Upgrade.Models
{
    public class UpgradeProgressArgs : EventArgs
    {
        public UpgradeProgressArgs(int partId, int progress)
        {
            this.PartId = partId;
            this.Progress = progress;
        }
        public int PartId { get; private set; }
        public int Progress { get; private set; }
    }
}
