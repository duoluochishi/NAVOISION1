//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/14 10:32:32     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.Transfer.MPPS
{
    public class PerformedSeriesInfo
    {
        public string RetrieveAETitle { get; set; } = string.Empty;
        public string SeriesDescription { get; set; } = string.Empty;
        public string PerformingPhysicianName { get; set; } = string.Empty;
        public string OperatorsName { get; set; } = string.Empty;
        public string ProtocolName { get; set; } = string.Empty;
        public string SeriesInstanceUID { get; set; } = string.Empty;
        public string ReferencedSOPClassUID { get; set; } = string.Empty;
        public string ReferencedSOPInstanceUID { get; set; } = string.Empty;

    }
}
