//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/23 16:55:37           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Models
{
    public class LocationParam
    {
        public string ScanID { get; set; } = string.Empty;

        public string LocationSeriesUID { get; set; } = string.Empty;

        public string LocationSeriesName { get; set; } = string.Empty;

        public double CenterFirstX { get; set; }

        public double CenterFirstY { get; set; }

        public double CenterFirstZ { get; set; }

        public double CenterLastX { get; set; }

        public double CenterLastY { get; set; }

        public double CenterLastZ { get; set; }

        public double FoVLengthHor { get; set; }

        public double FoVLengthVer { get; set; }

        public double FOVDirectionHorX { get; set; }

        public double FOVDirectionHorY { get; set; }

        public double FOVDirectionHorZ { get; set; }

        public double FOVDirectionVerX { get; set; }

        public double FOVDirectionVerY { get; set; }

        public double FOVDirectionVerZ { get; set; }

        public bool IsSquareFixed { get; set; }

        public bool IsChild { get; set; }

        public PerformStatus Status { get; set; }

        public FailureReasonType FailureReasonType { get; set; }

        public bool IsSelected { get; set; }
    }
}