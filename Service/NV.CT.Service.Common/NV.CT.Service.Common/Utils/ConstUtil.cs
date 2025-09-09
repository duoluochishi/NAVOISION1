using System;
using NV.MPS.Configuration;

namespace NV.CT.Service.Common.Utils
{
    public static class ConstUtil
    {
        /// <summary>
        /// Collimator每排的宽度
        /// <para>单位: 毫米(mm)</para>
        /// </summary>
        /// <remarks>
        /// TODO: 对于使用 0.165，还是使用实际计算的值，尚无定论。目前暂时使用实际值并进行了一些精度转换，等统一确定后，MPS会提供通用项，届时此属性值将会删除
        /// </remarks>
        public static double PerCollimatorSliceWidth { get; } = Math.Round(Math.Ceiling(SystemConfig.DetectorConfig.Detector.RelativeResolution) / 1000, 3);
    }
}