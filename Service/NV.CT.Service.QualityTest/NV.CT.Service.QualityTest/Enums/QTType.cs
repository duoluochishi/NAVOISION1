namespace NV.CT.Service.QualityTest.Enums
{
    public enum QTType
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,
        /// <summary>
        /// 一体化模体 摆模
        /// </summary>
        IntegrationPhantom,
        /// <summary>
        /// 轴向层厚
        /// </summary>
        SliceThicknessAxial,
        /// <summary>
        /// 螺旋层厚
        /// </summary>
        SliceThicknessHelical,
        /// <summary>
        /// 均匀性
        /// </summary>
        Homogeneity,
        /// <summary>
        /// 水模CT值
        /// </summary>
        CTOfWater,
        /// <summary>
        /// 水模噪声
        /// </summary>
        NoiseOfWater,
        /// <summary>
        /// 对比度标尺
        /// </summary>
        ContrastScale,
        /// <summary>
        /// MTF-XY
        /// </summary>
        MTF_XY,
        /// <summary>
        /// MTF-Z
        /// </summary>
        MTF_Z,
    }
}
