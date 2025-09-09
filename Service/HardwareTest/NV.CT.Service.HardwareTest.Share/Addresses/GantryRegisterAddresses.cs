namespace NV.CT.Service.HardwareTest.Share.Addresses
{
    public static class GantryRegisterAddresses
    {
        /** 系统状态寄存器 **/
        public static readonly uint SystemStaus = 0x0004A000;
        /** 错误寄存器 **/
        public static readonly uint Error = 0x0004A054;
        /** 运动启停 **/
        public static readonly uint MoveSwitch = 0x0004A098;
        /** 运动模式 **/
        public static readonly uint MoveMode = 0x0004A09C;
        /** 运动到匀速时的最大速度 **/
        public static readonly uint MaximumVelocity = 0x0004A0AC;
        /** 运动方向 **/
        public static readonly uint RotateDirection = 0x0004A0B0;
    }
}
