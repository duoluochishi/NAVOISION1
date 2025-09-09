using NV.CT.Service.Common.Framework;
using NV.CT.Service.QualityTest.Enums;

namespace NV.CT.Service.QualityTest.Models
{
    public sealed class ConfigModel : ViewModelBase
    {
        /// <summary>
        /// 设置可进入报告的类型
        /// </summary>
        public ReportAllowType ReportAllowType { get; set; }

        /// <summary>
        /// 是否跳过校验摆膜正确与否，默认为<see langword="false"/>
        /// </summary>
        public bool SkipPhantomValidate { get; set; }
    }
}