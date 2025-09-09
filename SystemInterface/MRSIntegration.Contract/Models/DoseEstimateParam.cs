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
    public struct DoseEstimateParam
    {
        public ScanOption ScanOption { get; set; }

        public CTS.Enums.BodyPart BodyPart { get; set; }

        public ExposureMode ExposureMode { get; set; }
        public int KV { get; set; }

        public int MA { get; set; }

        public int FramePerCycle { get; set; }

        public float ExposureTime { get; set; }

        /// <summary>
        /// Topo/Axial
        /// </summary>
        public float TableFeed { get; set; }

        /// <summary>
        /// Helical
        /// </summary>
        public float Pitch { get; set; }

        public float ScanLength { get; set; }

        public uint CollimatorOpenWidth { get; set; }
    }
}