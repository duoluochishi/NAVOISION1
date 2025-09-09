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
    /// <summary>
    /// Only type 1 listed
    /// </summary>
    public class CTImageModule: IDicomDatasetUpdater
    {
        public string ImageType { get; set; } = "";

        public ushort SamplesPerPixel { get;set; }

        public ushort BitsAllocated { get; set; }
        public ushort BitsStored { get; set; }
        public ushort HighBit { get; set; }

        public double RescaleIntercept { get; set; }

        public double RescaleSlope { get; set; }

        public PhotometricInterpretationCS PhotometricInterpretation { get; set; }

        public CTImageModule() { }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.ImageType, ImageType);
            ds.AddOrUpdate(DicomTag.SamplesPerPixel, SamplesPerPixel);
            ds.AddOrUpdate(DicomTag.PhotometricInterpretation, PhotometricInterpretation);
            ds.AddOrUpdate(DicomTag.BitsAllocated, BitsAllocated);
            ds.AddOrUpdate(DicomTag.BitsStored, BitsStored);
            ds.AddOrUpdate(DicomTag.HighBit, HighBit);
            ds.AddOrUpdate(DicomTag.RescaleIntercept, RescaleIntercept);
            ds.AddOrUpdate(DicomTag.RescaleSlope, RescaleSlope);
        }

        public void Read(DicomDataset ds)
        {
            ImageType = DicomContentHelper.GetDicomTag<string>(ds,DicomTag.ImageType);
            SamplesPerPixel = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.SamplesPerPixel);
            BitsAllocated = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.BitsAllocated);
            BitsStored = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.BitsStored);
            HighBit = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.HighBit);
            RescaleIntercept = DicomContentHelper.GetDicomTag<double>(ds, DicomTag.RescaleIntercept);
            RescaleSlope = DicomContentHelper.GetDicomTag<double>(ds, DicomTag.RescaleSlope);
            PhotometricInterpretation = DicomContentHelper.GetDicomTag<PhotometricInterpretationCS>(ds,DicomTag.PhotometricInterpretation);
        }
    }
}
