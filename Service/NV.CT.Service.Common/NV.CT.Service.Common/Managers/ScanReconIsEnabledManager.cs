using System;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common.Models.ScanReconModels;

namespace NV.CT.Service.Common.Managers
{
    public static class ScanReconIsEnabledManager
    {
        /// <summary>
        /// 针对<see cref="ScanParamModel"/>，螺旋扫时才可用
        /// </summary>
        public static Func<ScanParamModel, bool> WhenHelical => i => i.ScanOption == ScanOption.Helical;

        /// <summary>
        /// 针对<see cref="ScanParamModel"/>，定位扫或双定位扫时才可用
        /// </summary>
        public static Func<ScanParamModel, bool> WhenSurview => i => i.ScanOption is ScanOption.Surview or ScanOption.DualScout;

        /// <summary>
        /// 针对<see cref="ScanParamModel"/>，非定位扫且非双定位扫时才可用
        /// </summary>
        public static Func<ScanParamModel, bool> WhenNotSurview => i => i.ScanOption is not (ScanOption.Surview or ScanOption.DualScout);
    }
}