using System.Xml.Serialization;

namespace NV.CT.Service.HardwareTest.Share.Addresses
{
    public static class IfBoxRegisterAddresses
    {
        /** 系统状态 **/
        public static readonly uint SystemStatus = 0x0002A000;

        /** 对应不同的曝光模式配置 **/
        public static readonly uint IdxSource0 = 0x0002A080;
        public static readonly uint IdxSource1 = 0x0002A084;
        public static readonly uint IdxSource2 = 0x0002A088;
        public static readonly uint IdxSource3 = 0x0002A08C;
        public static readonly uint IdxSource4 = 0x0002A090;
        public static readonly uint IdxSource5 = 0x0002A094;
        public static readonly uint IdxSource6 = 0x0002A098;
        public static readonly uint IdxSource7 = 0x0002A09C;
        /** 对应不同的曝光模式配置 **/
        public static readonly uint IdxFocus0 = 0x0002A0A0;
        public static readonly uint IdxFocus1 = 0x0002A0A4;
        public static readonly uint IdxFocus2 = 0x0002A0A8;
        public static readonly uint IdxFocus3 = 0x0002A0AC;
        public static readonly uint IdxFocus4 = 0x0002A0B0;
        public static readonly uint IdxFocus5 = 0x0002A0B4;
        public static readonly uint IdxFocus6 = 0x0002A0B8;
        public static readonly uint IdxFocus7 = 0x0002A0BC;

        /** 轮流曝光模式时，轮流变化的步数 **/
        public static readonly uint PipeModeLength = 0x0002A0C0;
        /** 角度触发、床同步、角度转动方向、床位移方向 **/
        public static readonly uint PositionSet = 0x0002A0D4;
        /** 床门控触发使能 **/
        public static readonly uint TableTriggerEnable = 0x0001A0FC;
        /** 曝光序列循环次数，默认设置为1 **/
        public static readonly uint SeriesCycles = 0x0002A134;
        /** 位置触发控制值 默认设置为0 **/
        public static readonly uint PositionTriggerCtrl = 0x0002A0D0;
        /** 实际PostOffset张数 **/
        public static readonly uint PostOffsetFix = 0x0002A13C;
        /** PostOffset序列长度 **/
        public static readonly uint PostSeriseLength = 0x0002A130;
        /** CorrectionFrames **/
        public static readonly uint CorrectionFrames = 0x0000A134;
        /** 曝光序列张数 **/
        public static readonly uint FrameExposed = 0x0002A0C4;
        /** 限束器源编号  广播所有节点为0xFF **/
        public static readonly uint IFBoxCollimatorSource = 0x0002A110;
        /** 限束器焦点编号  广播所有节点为0xFF **/
        public static readonly uint IFBoxCollimatorFocus = 0x0002A114;
        /** 发送nvsynca_F消息 **/
        public static readonly uint NvSyncAFFlag = 0x0002A118;
        /** 实际运动步数 **/
        public static readonly uint NvSyncAFFStep = 0x0002A11C;
        /** 执行命令 **/
        public static readonly uint NvSyncAFExecute = 0x0002A120;
        /** 门灯控制 **/
        public static readonly uint LightSwitch = 0x0002A20C;

        /**************************** 限束器 ******************************/

        /** 限束器源编号 **/
        public static readonly uint IfBoxSourceTube = 0x0002A110;
        /** 限束器焦点编号 **/
        public static readonly uint IfBoxFocusTube = 0x0002A114;
        /** 电机位置使能寄存器 **/
        public static readonly uint IfBoxMotorPositionFeedback = 0x0002A004;
        /** ifbox开始向波太限束器下发校准表（使能本参数前，确保校准表已经下发至ifbox）**/
        public static readonly uint IfBoxCalibrationEnable = 0x0002A210;
        /** 电机位置使能寄存器 **/
        public static readonly uint IfBoxCollimatorOpeningMode = 0x0002A214;
        /** 电机位置使能寄存器 **/
        public static readonly uint IfBoxCollimatorOpeningWidth = 0x0002A218;
        /** 电机位置使能寄存器 **/
        public static readonly uint IfBoxCollimatorOpeningExecute = 0x0002A220;
        /** 
         * Position1 - bit0-bit15 前遮挡 bit16-bit32 后遮挡
         * Position2 - bit0-bit15 波太电机位置
         * **/
        public static readonly uint IfBoxControlPosition1_1st = 0x0002A14C;
        public static readonly uint IfBoxControlPosition2_1st = 0x0002A150;
        public static readonly uint IfBoxControlPosition1_2nd = 0x0002A154;
        public static readonly uint IfBoxControlPosition2_2nd = 0x0002A158;
        public static readonly uint IfBoxControlPosition1_3rd = 0x0002A15C;
        public static readonly uint IfBoxControlPosition2_3rd = 0x0002A160;
        public static readonly uint IfBoxControlPosition1_4th = 0x0002A164;
        public static readonly uint IfBoxControlPosition2_4th = 0x0002A168;
        public static readonly uint IfBoxControlPosition1_5th = 0x0002A16C;
        public static readonly uint IfBoxControlPosition2_5th = 0x0002A170;
        public static readonly uint IfBoxControlPosition1_6th = 0x0002A174;
        public static readonly uint IfBoxControlPosition2_6th = 0x0002A178;
        public static readonly uint IfBoxControlPosition1_7th = 0x0002A17C;
        public static readonly uint IfBoxControlPosition2_7th = 0x0002A180;
        public static readonly uint IfBoxControlPosition1_8th = 0x0002A184;
        public static readonly uint IfBoxControlPosition2_8th = 0x0002A188;
        public static readonly uint IfBoxControlPosition1_9th = 0x0002A18C;
        public static readonly uint IfBoxControlPosition2_9th = 0x0002A190;
        public static readonly uint IfBoxControlPosition1_10th = 0x0002A194;
        public static readonly uint IfBoxControlPosition2_10th = 0x0002A198;
        public static readonly uint IfBoxControlPosition1_11th = 0x0002A19C;
        public static readonly uint IfBoxControlPosition2_11th = 0x0002A1A0;
        public static readonly uint IfBoxControlPosition1_12th = 0x0002A1A4;
        public static readonly uint IfBoxControlPosition2_12th = 0x0002A1A8;
        public static readonly uint IfBoxControlPosition1_13th = 0x0002A1AC;
        public static readonly uint IfBoxControlPosition2_13th = 0x0002A1B0;
        public static readonly uint IfBoxControlPosition1_14th = 0x0002A1B4;
        public static readonly uint IfBoxControlPosition2_14th = 0x0002A1B8;
        public static readonly uint IfBoxControlPosition1_15th = 0x0002A1BC;
        public static readonly uint IfBoxControlPosition2_15th = 0x0002A1C0;
        public static readonly uint IfBoxControlPosition1_16th = 0x0002A1C4;
        public static readonly uint IfBoxControlPosition2_16th = 0x0002A1C8;
        public static readonly uint IfBoxControlPosition1_17th = 0x0002A1CC;
        public static readonly uint IfBoxControlPosition2_17th = 0x0002A1D0;
        public static readonly uint IfBoxControlPosition1_18th = 0x0002A1D4;
        public static readonly uint IfBoxControlPosition2_18th = 0x0002A1D8;
        public static readonly uint IfBoxControlPosition1_19th = 0x0002A1DC;
        public static readonly uint IfBoxControlPosition2_19th = 0x0002A1E0;
        public static readonly uint IfBoxControlPosition1_20th = 0x0002A1E4;
        public static readonly uint IfBoxControlPosition2_20th = 0x0002A1E8;
        public static readonly uint IfBoxControlPosition1_21th = 0x0002A1EC;
        public static readonly uint IfBoxControlPosition2_21th = 0x0002A1F0;
        public static readonly uint IfBoxControlPosition1_22th = 0x0002A1F4;
        public static readonly uint IfBoxControlPosition2_22th = 0x0002A1F8;
        public static readonly uint IfBoxControlPosition1_23th = 0x0002A1FC;
        public static readonly uint IfBoxControlPosition2_23th = 0x0002A200;
        public static readonly uint IfBoxControlPosition1_24th = 0x0002A204;
        public static readonly uint IfBoxControlPosition2_24th = 0x0002A208;

    }
}
