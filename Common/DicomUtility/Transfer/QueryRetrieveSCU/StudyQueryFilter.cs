//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 14:42:54     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom;
using NV.CT.DicomUtility.DicomCodeStringLib;

namespace NV.CT.DicomUtility.Transfer.QueryRetrieveSCU
{

    /// <summary>
    /// Study级别查询条件，当前只支持PatientID, PatientName, 日期范围
    /// </summary>
    public class StudyQueryFilter
    {
        public string PatientName { get; set; } = string.Empty;

        public string PatientId { get;set; } = string.Empty;

        public string PatientSex { get; set; } = string.Empty;
        
        public string AccessionNumber { get; set;} = string.Empty;

        public string ReferringPhysicianName { get; set; } = string.Empty; 

        public DateTime StudyDateStart { get; set; } = DateTime.Today;

        public DateTime StudyDateEnd { get; set; } = DateTime.Today;

        public void AdaptDicomDataset(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientID, PatientId);
            ds.AddOrUpdate(DicomTag.PatientName, PatientName);
            ds.AddOrUpdate(DicomTag.StudyDate, new DicomDateRange(StudyDateStart.Date,StudyDateEnd.Date.AddDays(1)));
            ds.AddOrUpdate(DicomTag.PatientSex, PatientSex);
            ds.AddOrUpdate(DicomTag.AccessionNumber, AccessionNumber);
            ds.AddOrUpdate(DicomTag.ReferringPhysicianName, ReferringPhysicianName);
        }
            
    }
}
