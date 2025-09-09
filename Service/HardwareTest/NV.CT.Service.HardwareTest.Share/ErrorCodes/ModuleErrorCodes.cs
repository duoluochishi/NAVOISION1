namespace NV.CT.Service.HardwareTest.Share.ErrorCodes
{
    /// <summary>
    /// 硬件测试模块码：029
    /// 通用错误
    /// 错误码格式: MRS029000001
    /// </summary>
    public static class HardwareTestErrorCodes
    {
        public const string MRS_HardwareTest_InitializationFailure = "MRS029000001";
        public const string MRS_HardwareTest_ProxyBadConnection = "MRS029000002";
    }

    /// <summary>
    /// 硬件测试模块码：029
    /// 球管测试功能码：B01
    /// 错误码格式: MRS029B01001
    /// </summary>
    public static class DataAcquistionErrorCodes
    {
        public const string MRS_HardwareTest_DataAcquisition_CalibrationFailure = "MRS029B01002";
        public const string MRS_HardwareTest_DataAcquisition_CorrectionFailure = "MRS029B01003";
        public const string MRS_HardwareTest_DataAcquisition_CollimatorSyncFailure = "MRS029B01004";
    }

    /// <summary>
    /// 硬件测试模块码：029
    /// 球管测试功能码：B02
    /// 错误码格式: MRS029B02001
    /// </summary>
    public static class ImageChainErrorCodes
    {
        public const string MRS_HardwareTest_ImageChain_ReconIDMatchFailure = "MRS029B02001";
    }

    /// <summary>
    /// 硬件测试模块码：029
    /// 球管测试功能码：A04
    /// 错误码格式: MRS029A04001
    /// </summary>
    public static class TubeTestingErrorCodes
    {
        
    }

    /// <summary>
    /// 硬件测试模块码：029
    /// 限束器测试功能码：A05
    /// 错误码格式: MRS029A05001
    /// </summary>
    public static class CollimatorTestingErrorCodes
    {
        public const string MRS_HardwareTest_CollimatorTesting_CalibrationFailure = "MRS029A05001";
    }





}
