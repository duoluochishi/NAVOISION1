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
    public class GeneralSeriesModule : IDicomDatasetUpdater
    {
        public ModalityCS Modality { get; set; }                                    //(0008,0060)

        public string SeriesInstanceUID { get; set; } = "";                           //(0020,000E)

        public int SeriesNumber { get; set; }                                       //(0020,0011)

        public string SeriesDescription { get; set; } = string.Empty; //(0008,103E)

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.Modality, Modality);
            ds.AddOrUpdate(DicomTag.SeriesInstanceUID, SeriesInstanceUID);
            ds.AddOrUpdate(DicomTag.SeriesNumber, SeriesNumber);
            ds.AddOrUpdate(DicomTag.SeriesDescription, SeriesDescription);
        }

        public void Read(DicomDataset ds)
        {
            Modality = DicomContentHelper.GetDicomTag<ModalityCS>(ds, DicomTag.Modality);
            SeriesInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SeriesInstanceUID);
            SeriesNumber = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.SeriesNumber);
            SeriesDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SeriesDescription);
        }
    }
}
