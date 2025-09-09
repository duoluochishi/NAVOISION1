namespace NV.CT.Service.HardwareTest.Share.Enums.Components
{
    public enum XRaySourceTestStatus
    {
        NormalStop,
        RunningSingleTest,
        RunningCycleTest,
        SuspendCycleTest,
    }

    public enum XRaySourceTestMode 
    {
        Single,
        Cycle
    }

    //public enum XRaySourceIndex
    //{
    //    All = 0,
    //    XRaySource01,
    //    XRaySource02,
    //    XRaySource03,
    //    XRaySource04,
    //    XRaySource05,
    //    XRaySource06,
    //    XRaySource07,
    //    XRaySource08,
    //    XRaySource09,
    //    XRaySource10,
    //    XRaySource11,
    //    XRaySource12,
    //    XRaySource13,
    //    XRaySource14,
    //    XRaySource15,
    //    XRaySource16,
    //    XRaySource17,
    //    XRaySource18,
    //    XRaySource19,
    //    XRaySource20,
    //    XRaySource21,
    //    XRaySource22,
    //    XRaySource23,
    //    XRaySource24
    //}

    public enum XRaySourceDoseType
    {
        kV,
        mA,
        ms
    }

    public enum ScanSwitch
    {
        Start,
        Stop
    }

    public enum XRaySourceHistoryDataType 
    {
        kV,
        mA,
        ms,
        HeatCap,
        OilTemp
    }

    public enum XRaySourceStatus
    {
        Offline,
        Online,
        Error,
        CalibrationComplete,
        CalibrationFailed
    }

    public enum XRayPromptLightSwitch 
    {     
        OFF = 0,
        ON = 1
    }

    public enum XRaySourceKVMACoefficientStartStep
    {
        Stop,
        GantryMove,
        Exposure,
    }
}
