//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using FellowOakDicom;
using System;

namespace NV.CT.DicomUtility.DicomModule
{
    public class PatientModule: IDicomDatasetUpdater
    {
        #region TYPE1
        #endregion TYPE1

        #region TYPE2

        public string PatientName { get; set; }             //(0010,0010)
        public string PatientID { get; set; }               //(0010,0020)
        public DateTime PatientBirthDate { get; set; }        //(0010,0030)
        public PatientSexCS PatientSex { get; set; }              //(0010,0040)   CS=>PatientSexCS

        #endregion TYPE2

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientName, PatientName);
            ds.AddOrUpdate(DicomTag.PatientID, PatientID);
            ds.AddOrUpdate(DicomTag.PatientBirthDate, PatientBirthDate);
            ds.AddOrUpdate(DicomTag.PatientSex, PatientSex);
        }
        public void Read(DicomDataset ds)
        {
            PatientName = DicomContentHelper.GetDicomTag<string>(ds,DicomTag.PatientName);
            PatientID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientID);
            PatientBirthDate = ds.GetDateTime(DicomTag.PatientBirthDate,DicomTag.PatientBirthTime);
            PatientSex = DicomContentHelper.GetDicomTag<PatientSexCS>(ds, DicomTag.PatientSex);

        }
    }
}