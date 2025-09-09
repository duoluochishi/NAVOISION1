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
using NV.CT.DicomUtility.DicomSQ;

namespace NV.CT.DicomUtility.DicomModule
{
    public class VisitIdentification​Module : IDicomDatasetUpdater
    {
        public string AdmissionID​ { get; set; }

        public IssuerOfAdmissionIDSequence IssuerOfAdmissionIDSequence {get;set;}

        public string InstitutionName { get; set; }

        public string InstitutionAddress { get; set; }

        public VisitIdentificationModule()
        {
            IssuerOfAdmissionIDSequence = new IssuerOfAdmissionIDSequence();
        }


        // public SQ IssuerofAdmissionIDSequence​ {get;set;} 
        public void Read(DicomDataset ds)
        {
            AdmissionID​ = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.AdmissionID​);
            InstitutionName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.InstitutionName);
            InstitutionAddress = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.InstitutionAddress);
            IssuerOfAdmissionIDSequence.Read(ds);
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.AdmissionID​, AdmissionID​);
            ds.AddOrUpdate(DicomTag.InstitutionName, InstitutionName);
            ds.AddOrUpdate(DicomTag.InstitutionAddress, InstitutionAddress);
            IssuerOfAdmissionIDSequence.Update(ds);
        }
    }
}
