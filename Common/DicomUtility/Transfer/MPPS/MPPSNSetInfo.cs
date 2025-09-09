//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/13 17:22:23     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DicomUtility.DicomCodeStringLib;

namespace NV.CT.DicomUtility.Transfer.MPPS
{
    /// <summary>
    /// MPPS N-Create 更新信息，用于MPPS中N-Set过程创建的Completed 或Discontinued信息
    /// </summary>
    public class MPPSNSetInfo
    {
        public string AffectedInstanceUid { get; set; } = string.Empty;

        public DateTime PerformedProcedureStepEndDateTime { get; set; } = DateTime.Now;
        public PerformedProcedureStepStatusCS PerformedProcedureStepStatus { get; set; } = PerformedProcedureStepStatusCS.COMPLETED;
        public string PerformedProcedureStepDescription { get; set; } = string.Empty;

        public List<PerformedSeriesInfo> SeriesInfos { get; set; }


        public MPPSNSetInfo()
        {
            SeriesInfos = new List<PerformedSeriesInfo>();
        }
    }
}
