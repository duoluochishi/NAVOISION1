//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 15:12:22     V1.0.0       李勇
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
    public class SeriesQueryResult
    {
        public string Modality { get; set; }    

        public DateTime SeriesDateTime { get; set; }

        public int NumberOfSeriesRelatedInstances { get; set; }

        public int SeriesNumber { get; set; }

        public string SeriesDescription { get; set; }

        public string SeriesInstanceUID { get; set; }

        public static void AttachEmptySeriesDataset(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            ds.AddOrUpdate(DicomTag.SeriesDescription, "");
            ds.AddOrUpdate(DicomTag.SeriesNumber, "");
            ds.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, "");
            ds.AddOrUpdate(DicomTag.SeriesDate, "");
            ds.AddOrUpdate(DicomTag.SeriesTime, "");
            ds.AddOrUpdate(DicomTag.Modality, "");
        }

        public static SeriesQueryResult GetSeriesQueryResult(DicomDataset ds)
        {
            SeriesQueryResult result = new SeriesQueryResult();

            result.SeriesInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SeriesInstanceUID);
            result.SeriesDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SeriesDescription);
            result.SeriesNumber = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.SeriesNumber);
            result.NumberOfSeriesRelatedInstances = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.NumberOfSeriesRelatedInstances);
            result.SeriesDateTime = DicomContentHelper.GetDicomDateTime(ds, DicomTag.SeriesDate,DicomTag.SeriesTime);   
            result.Modality = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.Modality);

            return result;
        }
    }
}
