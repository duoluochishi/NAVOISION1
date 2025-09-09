namespace NV.CT.Service.HardwareTest.Attachments.Configurations
{
    public class XRaySourceConfigOptions
    {
        //Tube板数量
        public uint TubeInterfaceCount { get; set; }
        //每个Tube板上的射线源数量 
        public uint XRaySourceCountPerTubeInterface { get; set; }
        //电压 
        public uint Voltage{ get; set; }
        //电流 
        public uint Current { get; set; }
        //曝光时间 
        public uint ExposureTime { get; set; }
        //帧时间 
        public uint FrameTime { get; set; }
        //序列长度 
        public uint SeriesLength { get; set; }
        //延迟曝光时间 
        public uint DelayExposureTime { get; set; }
        //子节点Ready使能
        public uint ExposureRelatedChildNodesConfig { get; set; }
        //周期循环数 
        public uint CycleCount { get; set; }
        //周期间隔 
        public uint CycleInterval { get; set; }
        //热容上限 
        public uint HeatCapacityUpperLimit { get; set; }
        //热容下限 
        public uint HeatCapacityLowerLimit { get; set; }
    }
}
