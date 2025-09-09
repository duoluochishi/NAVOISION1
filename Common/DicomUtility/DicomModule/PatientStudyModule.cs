//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomModule
{
    /// <summary>
    /// 全是3，想用啥加啥
    /// </summary>
    public class PatientStudyModule : IDicomDatasetUpdater
    {
        public string PatientAge { get; set; }                       //(0010,1010)
        public double PatientSize { get; set; }                       //(0010,1020)
        public double PatientWeight { get; set; }                       //(0010,1030)

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientAge, PatientAge);
            ds.AddOrUpdate(DicomTag.PatientSize, PatientSize);
            ds.AddOrUpdate(DicomTag.PatientWeight, PatientWeight);
        }
        public void Read(DicomDataset ds)
        {
            PatientAge = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientAge);
            PatientSize = DicomContentHelper.GetDicomTag<double>(ds, DicomTag.PatientSize);
            PatientWeight = DicomContentHelper.GetDicomTag<double>(ds, DicomTag.PatientWeight);
        }
    }
}
