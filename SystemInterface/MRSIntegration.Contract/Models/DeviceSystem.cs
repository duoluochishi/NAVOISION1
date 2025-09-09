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

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Models
{
    public class DeviceSystem
    {
        public SystemStatus SystemStatus { get; set; }

        public RealtimeStatus RealtimeStatus { get; set; }

        /// <summary>
        /// 电源柜控制板
        /// </summary>
        public DevicePart PDU { get; set; }

        public DevicePart CTBox { get; set; }

        public DevicePart IFBox { get; set; }

        /// <summary>
        /// 运动控制板
        /// </summary>
        public Gantry Gantry { get; set; }

        public DevicePart AuxBoard { get; set; }

        public DevicePart ExtBoard { get; set; }

        /// <summary>
        /// 高压接口板(6个)
        /// </summary>
        public List<TubeIntf> TubeInfts { get; set; }

        /// <summary>
        /// 床信息
        /// </summary>
        public Table Table { get; set; }

        /// <summary>
        /// 球管
        /// </summary>
        public List<Tube> Tubes { get; set; }

        ///// <summary>
        ///// 探测器模组（16个）
        ///// </summary>
        //public List<Detector> Detectors { get; set; }

        public DevicePart ControlBox { get; set; }

        /// <summary>
        /// 实时重建
        /// </summary>
        public DevicePart RTDRecon { get; set; }

        /// <summary>
        /// 探测器子部件
        /// </summary>
        public Detector Detector { get; set; }

        /// <summary>
        /// 门控状态
        /// </summary>
        public bool DoorClosed { get; set; }

        /// <summary>
        /// 探测器温度正常状态
        /// </summary>
        public bool IsDetectorTemperatureNormalStatus { get; set; }
    }
}
