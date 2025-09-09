//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomSQ;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomModule
{
    public class SRDocumentSeriesModule:IDicomDatasetUpdater
    {
        #region TYPE1
        public ModalityCS Modality { get; set; }                    //(0008,0060)
        public string SeriesInstanceUID { get; set; }           //(0020,000E)
        public int SeriesNumber { get; set; }                //(0020,0011)
        public string SeriesDescription { get; set; }
        #endregion TYPE1

        #region TYPE2
        public ReferencedPerformedProcedureStepSequence ReferencedPerformedProcedureStepSequence { get; set; }          //(0008,1111)

        #endregion TYPE2
        public SRDocumentSeriesModule()
        {
            ReferencedPerformedProcedureStepSequence = new ReferencedPerformedProcedureStepSequence();
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.Modality,Modality);
            ds.AddOrUpdate(DicomTag.SeriesInstanceUID, SeriesInstanceUID);
            ds.AddOrUpdate(DicomTag.SeriesNumber, SeriesNumber);
            ds.AddOrUpdate(DicomTag.SeriesDescription, SeriesDescription);

            ReferencedPerformedProcedureStepSequence.Update(ds);
        }
        public void Read(DicomDataset ds)
        {
            Modality = DicomContentHelper.GetDicomTag<ModalityCS>(ds, DicomTag.Modality);
            SeriesInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyInstanceUID);
            SeriesNumber = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.SeriesNumber);
            SeriesDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SeriesDescription);
            ReferencedPerformedProcedureStepSequence.Read(ds);
        }
    }
}
