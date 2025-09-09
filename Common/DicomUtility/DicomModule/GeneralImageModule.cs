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
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;

namespace DoseReport.DicomModule
{
    public class GeneralImageModule:IDicomDatasetUpdater
    {
        public int InstanceNumber { get; set; }
        public string ImageType { get; set; } 

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.InstanceNumber, InstanceNumber);
            ds.AddOrUpdate(DicomTag.ImageType, ImageType);
        }
        public void Read(DicomDataset ds)
        {
            InstanceNumber = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.InstanceNumber);  
            ImageType = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ImageType);
        }
    }
}
