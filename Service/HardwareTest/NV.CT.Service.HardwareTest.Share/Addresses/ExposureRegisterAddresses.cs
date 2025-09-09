namespace NV.CT.Service.HardwareTest.Share.Addresses
{
    public static class ExposureRegisterAddresses
    {
        public static readonly uint WorkMode = 0x0000A064;
        public static readonly uint Voltage = 0x0000A068;
        public static readonly uint Current = 0x0000A088;
        public static readonly uint ExposureTime = 0x0000A0B4;
        public static readonly uint FrameTime = 0x0000A0B8;
        public static readonly uint ScanSeriesLength = 0x0000A0BC;
        public static readonly uint CTBoxMultiFunControl = 0x0001A0DC;
        public static readonly uint DelayExposureTime = 0x000A110;
        public static readonly uint AutoScanFlag = 0x0000A15C;
        public static readonly uint CTBoxSlaveDevTest = 0x0001A108;
        public static readonly uint CTBoxAngleTriggerEnable = 0x0001A0C8;
        public static readonly uint CTBoxTableTriggerEnable = 0x0001A0FC;
        public static readonly uint CTBoxChildNodesExposureEnable = 0x0001A074;
        public static readonly uint CTBoxChildNodesExposureEnableTest = 0x0001A114;
        public static readonly uint ScanMode = 0x0000A0F4;
        public static readonly uint ExposureMode = 0x0000A0F8;
        public static readonly uint ScanOption = 0x0000A0F0;
        public static readonly uint CTBoxWarmUp = 0x0001A0C4;
        public static readonly uint CTBoxExposureXraysNumber = 0x0001A0F0;
        public static readonly uint PrepareTimeout = 0x0001A084;
        public static readonly uint ExposureTimeout = 0x0001A088;
        public static readonly uint BowtieEnable = 0x0000A14C;
        public static readonly uint CollimatorZ = 0x0000A108;
        public static readonly uint CorrectionFrames = 0x0000A134;
        public static readonly uint FramesPerCycle = 0x0000A100;

        public static readonly uint TubeInterface1Filament = 0x0006A4C4;
        public static readonly uint TubeInterface2Filament = 0x0007A4C4;
        public static readonly uint TubeInterface3Filament = 0x0008A4C4;
        public static readonly uint TubeInterface4Filament = 0x0009A4C4;
        public static readonly uint TubeInterface5Filament = 0x000AA4C4;
        public static readonly uint TubeInterface6Filament = 0x000BA4C4;

        public static readonly uint PublicTubeInterfaceFocus = 0xA180;
        public static readonly uint TubeInterface1Focus = 0x0006A4A8;
        public static readonly uint TubeInterface2Focus = 0x0007A4A8;
        public static readonly uint TubeInterface3Focus = 0x0008A4A8;
        public static readonly uint TubeInterface4Focus = 0x0009A4A8;
        public static readonly uint TubeInterface5Focus = 0x000AA4A8;
        public static readonly uint TubeInterface6Focus = 0x000BA4A8;

        public static readonly uint TubeInterface1VelocitySwitch = 0x0006A4B0;
        public static readonly uint TubeInterface2VelocitySwitch = 0x0007A4B0;
        public static readonly uint TubeInterface3VelocitySwitch = 0x0008A4B0;
        public static readonly uint TubeInterface4VelocitySwitch = 0x0009A4B0;
        public static readonly uint TubeInterface5VelocitySwitch = 0x000AA4B0;
        public static readonly uint TubeInterface6VelocitySwitch = 0x000BA4B0;

        public static readonly uint TubeInterface1VelocityMode = 0x0006A4AC;
        public static readonly uint TubeInterface2VelocityMode = 0x0007A4AC;
        public static readonly uint TubeInterface3VelocityMode = 0x0008A4AC;
        public static readonly uint TubeInterface4VelocityMode = 0x0009A4AC;
        public static readonly uint TubeInterface5VelocityMode = 0x000AA4AC;
        public static readonly uint TubeInterface6VelocityMode = 0x000BA4AC;

        public static readonly uint TubeInterface1RotateSwitch = 0x0006A4B4;
        public static readonly uint TubeInterface2RotateSwitch = 0x0007A4B4;
        public static readonly uint TubeInterface3RotateSwitch = 0x0008A4B4;
        public static readonly uint TubeInterface4RotateSwitch = 0x0009A4B4;
        public static readonly uint TubeInterface5RotateSwitch = 0x000AA4B4;
        public static readonly uint TubeInterface6RotateSwitch = 0x000BA4B4;

        public static readonly uint TubeInterface1CalibrateSwitch = 0x0006A4C8;
        public static readonly uint TubeInterface2CalibrateSwitch = 0x0007A4C8;
        public static readonly uint TubeInterface3CalibrateSwitch = 0x0008A4C8;
        public static readonly uint TubeInterface4CalibrateSwitch = 0x0009A4C8;
        public static readonly uint TubeInterface5CalibrateSwitch = 0x000AA4C8;
        public static readonly uint TubeInterface6CalibrateSwitch = 0x000BA4C8;

        public static readonly uint ScanUID0 = 0x0000A13C;
        public static readonly uint ScanUID1 = 0x0000A140;
        public static readonly uint ScanUID2 = 0x0000A144;
        public static readonly uint ScanUID3 = 0x0000A148;

        public static readonly uint ScanControl = 0x0000A128;

        public static readonly uint TubeParamConfigEnd = 0x0000A158;

    }
}
