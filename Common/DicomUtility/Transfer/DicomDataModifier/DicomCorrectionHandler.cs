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

namespace NV.CT.DicomUtility.Transfer.DicomDataModifier
{
    /// <summary>
    /// Dicom Export 基本信息修改类
    /// </summary>
    public class DicomCorrectionHandler : IDicomDataModificationHandler
    {
        private Dictionary<(string, string), DicomDataset> DataSetCorrectionDic;

        public DicomCorrectionHandler()
        {
            DataSetCorrectionDic = new Dictionary<(string, string), DicomDataset>();
        }
        public void ModifyDicomFile(DicomFile dcmFile)
        {
            ModifyDicomData(dcmFile.Dataset);
        }

        public void ModifyDicomData(DicomDataset dataset)
        {
            var studyInstanceUid = DicomContentHelper.GetDicomTag<string>(dataset, DicomTag.StudyInstanceUID);
            var seriesInstanceUid = DicomContentHelper.GetDicomTag<string>(dataset, DicomTag.SeriesInstanceUID);
            if (DataSetCorrectionDic[(studyInstanceUid, seriesInstanceUid)] is null)
            {
                return;
            }

            foreach (var item in DataSetCorrectionDic[(studyInstanceUid, seriesInstanceUid)])
            {
                dataset.AddOrUpdate(item);
            }
        }

        public void AddCorrection<T>(string studyInstanceUid, string seriesInstanceUid, DicomTag dicomTag, T[] value)
        {
            if (!DataSetCorrectionDic.ContainsKey((studyInstanceUid, seriesInstanceUid)))
            {
                DataSetCorrectionDic[(studyInstanceUid, seriesInstanceUid)] = new DicomDataset();
            }
            DataSetCorrectionDic[(studyInstanceUid, seriesInstanceUid)].AddOrUpdate(dicomTag, value);
        }

        public void ClearCorrection()
        {
            DataSetCorrectionDic.Clear();
        }
    }
}
