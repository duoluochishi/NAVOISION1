//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Models
{
    public class DevicePart
    {
        public PartStatus Status { get; set; }
    }

    public class Gantry : DevicePart
    {
        /// <summary>
        /// 当前位置角度值，单位0.01度
        /// </summary>
        public uint Position { get; set; }

        /// <summary>
        /// 当前运动速度值，单位0.01度/s
        /// </summary>
        public uint Speed { get; set; }

        /// <summary>
        /// 前后罩是否关闭
        /// </summary>
        public bool FrontRearCoverClosed { get; set; }
    }

    public class TubeIntf : DevicePart
    {
        /// <summary>
        /// 球管编号
        /// </summary>
        public int Number { get; set; }
    }

    public class Table : DevicePart
    {
        /// <summary>
        /// 床水平运动速度
        /// </summary>
        public uint HorizontalSpeed { get; set; }

        /// <summary>
        /// 床竖直运动速度
        /// </summary>
        public uint VerticalSpeed { get; set; }

        /// <summary>
        /// 床水平位置
        /// </summary>
        public int HorizontalPosition { get; set; }

        /// <summary>
        /// 床垂直位置
        /// </summary>
        public uint VerticalPosition { get; set; }

        public bool Locked { get; set; }

        public int AxisXPosition { get; internal set; }

        public uint AxisXSpeed { get; internal set; }

    }

    public class Bowtie : DevicePart
    {
        /// <summary>
        /// 限速器开度 0-100%
        /// </summary>
        public int OpeningValue { get; set; }

        /// <summary>
        /// 是否开启波太
        /// </summary>
        public bool UseBowtie { get; set; }
    }

    /// <summary>
    /// 射线源
    /// </summary>
    public class RaySource
    {
        /// <summary>
        /// 油温
        /// </summary>
        public float OilTemperature { get; set; }

        /// <summary>
        /// 热容量
        /// </summary>
        public float HeatCapacity { get; set; }
    }

    public class Tube : DevicePart
    {
        /// <summary>
        /// 球管编号
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 射线源
        /// </summary>
        public RaySource RaySource { get; set; }
    }

    /// <summary>
    /// 探测器
    /// </summary>
    public class Detector
    {
        /// <summary>
        /// 探测器模组（16个模组）
        /// </summary>
        public DetectorModule[] DetectorModules { get; set; } = Enumerable.Range(1, 16).Select(i => new DetectorModule(i)).ToArray();

        /// <summary>
        /// 采集卡（两块）
        /// </summary>
        public AcqCard[] AcqCards { get; set; } = Enumerable.Range(0, 2).Select(i => new AcqCard()).ToArray();
    }

    public class AcqCard
    {
        /// <summary>
        /// 注册表接口状态
        /// </summary>
        public PartStatus RegisterInterfaceStatus { get; set; }

        /// <summary>
        /// 内存接口状态
        /// </summary>
        public PartStatus MemoryInterfaceStatus { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature { get; set; }
    }

    public class DetectorModule
    {
        public DetectorModule(int number)
        {
            Number = number;
            ProcessingBoards = Enumerable.Range(0, 4).Select(i => new ProcessingBoard()).ToArray();
            DetectBoards = Enumerable.Range(0, 4).Select(i => new DetectBoard()).ToArray();
            TemperatureControlBoard = new TemperatureControlBoard();
        }

        public int Number { get; private set; }

        public ProcessingBoard[] ProcessingBoards { get; set; }

        public DetectBoard[] DetectBoards { get; set; }

        public TemperatureControlBoard TemperatureControlBoard { get; set; }

        public PartStatus TransmissionBoardStatus { get; set; }

        public DetAcqMode DetAcqMode { get; set; }
    }

    public class TemperatureControlBoard: AbstractPart
    {
        /// <summary>
        /// 通道功率（当前有4个通道）
        /// </summary>
        public int[] Powers { get; set; } = new int[4];

        /// <summary>
        /// 湿度
        /// </summary>
        public int Humidity { get; set; }
    }

    public class ProcessingBoard : AbstractPart
    {
        public int Temperature { get; set; }
    }

    public class DetectBoard : AbstractPart
    {
        /// <summary>
        /// 上芯片温度
        /// </summary>
        public int Chip1Temperature { get; set; }

        /// <summary>
        /// 下芯片温度
        /// </summary>
        public int Chip2Temperature { get; set; }
    }
}
