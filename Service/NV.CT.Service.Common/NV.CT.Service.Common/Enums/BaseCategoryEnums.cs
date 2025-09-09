namespace NV.CT.Service.Enums
{
    /** 焦点大小 **/
    public enum FocusSize
    {
        Small = 0,
        Big
    }

    /** 旋阳速度 **/
    public enum RotateSpeedMode
    {
        Low = 0,
        High
    }

    /** 限束器信息类型 **/
    public enum CollimatorMessageType 
    {
        Normal,
        Home
    }

    /** 限束器源编号 **/
    public enum CollimatorSourceIndex
    {
        All = 0,
        Collimator01,
        Collimator02,
        Collimator03,
        Collimator04,
        Collimator05,
        Collimator06,
        Collimator07,
        Collimator08,
        Collimator09,
        Collimator10,
        Collimator11,
        Collimator12,
        Collimator13,
        Collimator14,
        Collimator15,
        Collimator16,
        Collimator17,
        Collimator18,
        Collimator19,
        Collimator20,
        Collimator21,
        Collimator22,
        Collimator23,
        Collimator24
    }

    /** 限束器电机类型 **/
    public enum CollimatorMotorType 
    {
        Bowtie = 0,
        FrontBlade = 1,
        RearBlade = 2
    }

    /** 门状态 **/
    public enum DoorStatus
    {
        Open,
        Closed
    }

    /** 探测器状态 **/
    public enum DetectorStatus 
    {
        NotReady = 0,
        Ready
    }

}
