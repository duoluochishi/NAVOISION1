//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/10 15:47:12     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.DicomUtility.Transfer.ModalityWorklist
{
    /// <summary>
    /// Worklist查询过滤器，支持的过滤项如类中所示
    /// </summary>
    public class WorklistFilter
    {
        public string WorkFlowID { get; }

        public string PatientID { get; }

        public string PatientName { get; }

        public string StationAE { get; }

        public string StationName { get; }

        public string Modality { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public WorklistFilter(string workflowID, DateTime startDate,DateTime endDate, string patientID = null, string patientName = null, string stationAE = null, string stationName = null, string modality = null)
        {
            WorkFlowID = workflowID;
            PatientID = patientID;
            PatientName = patientName;
            StationAE = stationAE;
            StationName = stationName;
            Modality = modality;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
