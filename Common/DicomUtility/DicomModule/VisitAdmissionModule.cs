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

namespace NV.CT.DicomUtility.DicomModule
{
    public class VisitAdmissionModule : IDicomDatasetUpdater
    {
        public string AdmittingDiagnosesDescription​ { get; set; }
        public void Read(DicomDataset ds)
        {
            AdmittingDiagnosesDescription​ = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.AdmittingDiagnosesDescription​);
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.AdmittingDiagnosesDescription​, AdmittingDiagnosesDescription​);
        }
    }
}
