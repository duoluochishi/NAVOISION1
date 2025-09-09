//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;

namespace NV.CT.DicomUtility.DicomModule
{
    public class PatientMedicalModule : IDicomDatasetUpdater
    {
        public string PatientState { get; set; }
        public ushort PregnancyStatus { get; set; }
        public string MedicalAlerts { get; set; }
        public string Allergies { get; set; }
        public SmokingStatusCS SmokingStatus { get; set; }
        public string SpecialNeeds​ { get; set; }

        public string AdditionalPatientHistory { get; set;}

        //public SQ PertinentDocumentsSequence


        public void Read(DicomDataset ds)
        {
            PatientState = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientState);
            PregnancyStatus = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.PregnancyStatus);
            MedicalAlerts = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.MedicalAlerts);
            Allergies = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.Allergies);
            SmokingStatus = DicomContentHelper.GetDicomTag<SmokingStatusCS>(ds, DicomTag.SmokingStatus);
            SpecialNeeds​ = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SpecialNeeds​);
            AdditionalPatientHistory = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.AdditionalPatientHistory);
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientState, PatientState);
            ds.AddOrUpdate(DicomTag.PregnancyStatus, PregnancyStatus);
            ds.AddOrUpdate(DicomTag.MedicalAlerts, MedicalAlerts);
            ds.AddOrUpdate(DicomTag.Allergies, Allergies);
            ds.AddOrUpdate(DicomTag.SmokingStatus, SmokingStatus);
            ds.AddOrUpdate(DicomTag.SpecialNeeds​, SpecialNeeds​);
            ds.AddOrUpdate(DicomTag.AdditionalPatientHistory, AdditionalPatientHistory);
        }
    }
}
