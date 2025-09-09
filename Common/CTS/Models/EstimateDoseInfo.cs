
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.CTS.Models
{
    /// <summary>
    /// 估算剂量信息
    /// </summary>
    public class EstimateDoseInfo
    {
        public float CTDIvol { get; set; }

        public float DLP { get; set; }

        public PhantomType PhantomType { get; set; }

        public bool IsEstimateSuccess { get; set; }
    }
}
