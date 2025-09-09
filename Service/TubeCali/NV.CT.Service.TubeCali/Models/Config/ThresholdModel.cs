namespace NV.CT.Service.TubeCali.Models.Config
{
    public class ThresholdModel
    {
        /// <summary>
        /// 热熔阈值，大于此值才能开启灯丝校准结果检查
        /// </summary>
        public double HeatCapacityThreshold { get; init; }

        /// <summary>
        /// Post_mA阈值，超过此值需要进行提醒
        /// <para>单位：毫安</para>
        /// </summary>
        public double MAThreshold { get; init; }
    }
}