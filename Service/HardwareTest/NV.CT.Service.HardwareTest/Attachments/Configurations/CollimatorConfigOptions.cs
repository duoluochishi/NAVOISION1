namespace NV.CT.Service.HardwareTest.Attachments.Configurations
{
    public class CollimatorConfigOptions
    {
        //限束器数量
        public int CollimatorSourceCount { get; set; }
        //全开宽度
        public int FullOpeningWidth { get; set; }
        //最小遮挡
        public uint MinMoveStep { get; set; }
        //最大前遮挡
        public uint MaxFrontBladeMoveStep { get; set; }
        //最大后遮挡
        public uint MaxRearBladeMoveStep { get; set; }
        //最大波太
        public uint MaxBowtie { get; set; }
        //等待限束器到达时间
        public int WaitForArriving { get; set; }
        //迭代间隔
        public int IterationInterval { get; set; }
    }
}
