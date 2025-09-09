//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DicomUtility.Contract;
using FellowOakDicom;
using System;

namespace NV.CT.DicomUtility.DicomModule
{
    public class GeneralStudyModule:IDicomDatasetUpdater
    {

        #region TYPE1
        public string StudyInstanceUID { get; set; }            //(0020,000D)
        #endregion TYPE1

        #region TYPE2
        public DateTime StudyDate { get; set; }                 //(0008,0020)
        public DateTime StudyTime { get; set; }                 //(0008,0030)
        public string AccessionNumber { get; set; }             //(0008,0050)

        public string ReferringPhysicianName { get; set; }      //(0008,0090)
        public string StudyID { get; set; }                     //(0020,0010)

        #endregion TYPE2


        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.StudyInstanceUID, StudyInstanceUID);
            ds.AddOrUpdate(DicomTag.StudyDate, StudyDate);
            ds.AddOrUpdate(DicomTag.StudyTime, StudyTime);
            ds.AddOrUpdate(DicomTag.AccessionNumber, AccessionNumber);
            ds.AddOrUpdate(DicomTag.ReferringPhysicianName, ReferringPhysicianName);
            ds.AddOrUpdate(DicomTag.StudyID, StudyID);
        }

        public void Read(DicomDataset ds)
        {
            StudyInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyInstanceUID);
            StudyDate = DicomContentHelper.GetDicomDateTime(ds, DicomTag.StudyDate,DicomTag.StudyTime);
            StudyTime = StudyDate;
            AccessionNumber = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.AccessionNumber);
            ReferringPhysicianName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ReferringPhysicianName);
            StudyID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyID);
        }
    }
}