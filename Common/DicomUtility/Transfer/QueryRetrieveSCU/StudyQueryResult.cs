//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 14:41:53     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom;
using NV.CT.DicomUtility.Contract;

namespace NV.CT.DicomUtility.Transfer.QueryRetrieveSCU
{
    public class StudyQueryResult
    {
        public string PatientName { get; set; }
        public string PatientID { get; set;}
        
        public DateTime PatientBirthDateTime { get;set; }

        public string PatientSex { get; set; }

        public string AccessionNumber { get; set; }

        public string StudyID { get; set; }

        public string StudyInstanceUID { get; set; }
        public string StudyDescription { get; set; }

        public DateTime StudyDateTime { get; set; }

        public string ReferringPhysicianName { get; set; }

        public string ModalitiesInStudy { get; set; }

        public int NumberOfStudyRelatedInstances { get; set; }

        public int NumberOfStudyRelatedSeries { get; set; }


        public static void AttachEmptyStudyDataset(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientName, "");
            ds.AddOrUpdate(DicomTag.PatientID, "");
            ds.AddOrUpdate(DicomTag.PatientBirthDate, "");
            ds.AddOrUpdate(DicomTag.PatientBirthTime, "");
            ds.AddOrUpdate(DicomTag.PatientSex, "");
            ds.AddOrUpdate(DicomTag.AccessionNumber, "");
            ds.AddOrUpdate(DicomTag.StudyID, "");
            ds.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            ds.AddOrUpdate(DicomTag.StudyDescription, "");
            ds.AddOrUpdate(DicomTag.StudyDate, "");
            ds.AddOrUpdate(DicomTag.StudyTime, "");
            ds.AddOrUpdate(DicomTag.ReferringPhysicianName, "");
            ds.AddOrUpdate(DicomTag.ModalitiesInStudy, "");
            ds.AddOrUpdate(DicomTag.NumberOfStudyRelatedInstances, "");
            ds.AddOrUpdate(DicomTag.NumberOfStudyRelatedSeries, "");
        }

        public static StudyQueryResult GetStudyQueryResult(DicomDataset ds)
        {
            var result = new StudyQueryResult();

            result.PatientName = DicomContentHelper.GetDicomTag<string>(ds,DicomTag.PatientName);
            result.PatientID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientID);
            result.PatientBirthDateTime = DicomContentHelper.GetDicomDateTime(ds, DicomTag.PatientBirthDate, DicomTag.PatientBirthTime);
            result.PatientSex = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientSex);
            result.AccessionNumber = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.AccessionNumber);
            result.StudyID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyID);
            result.StudyInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyInstanceUID);
            result.StudyDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyDescription);
            result.StudyDateTime = DicomContentHelper.GetDicomDateTime(ds, DicomTag.StudyDate, DicomTag.StudyTime);
            result.ReferringPhysicianName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ReferringPhysicianName);
            result.ModalitiesInStudy = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ModalitiesInStudy);
            result.NumberOfStudyRelatedInstances = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.NumberOfStudyRelatedInstances);
            result.NumberOfStudyRelatedSeries = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.NumberOfStudyRelatedSeries);

            return result;
        }
    }
}
