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
using FellowOakDicom.Imaging;
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;

namespace NV.CT.DicomUtility.DicomModule
{
    public class ImagePixelModule : IDicomDatasetUpdater
    {
        #region TYPE1
        public ushort SamplesPerPixel { get; set; }                                 //	(0028,0002)

        public PhotometricInterpretationCS PhotometricInterpretation { get; set; }      //(0028,0004)

        public ushort Rows { get; set; }

        public ushort Columns { get; set; }

        public ushort BitsAllocated { get; set; }
        public ushort BitsStored { get; set; }
        public ushort HighBit { get; set; }
        public ushort PixelRepresentation { get; set; }



        #endregion TYPE1
        #region TYPE2
        #endregion TYPE2


        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.SamplesPerPixel, SamplesPerPixel);
            ds.AddOrUpdate(DicomTag.PhotometricInterpretation, PhotometricInterpretation);
            ds.AddOrUpdate(DicomTag.Rows, Rows);
            ds.AddOrUpdate(DicomTag.Columns, Columns);
            ds.AddOrUpdate(DicomTag.BitsAllocated, BitsAllocated);
            ds.AddOrUpdate(DicomTag.BitsStored, BitsStored);
            ds.AddOrUpdate(DicomTag.HighBit, HighBit);
            ds.AddOrUpdate(DicomTag.PixelRepresentation, PixelRepresentation);
        }
        public void Read(DicomDataset ds)
        {
            SamplesPerPixel = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.SamplesPerPixel );
            Rows = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.Rows);
            Columns = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.Columns);
            BitsAllocated = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.BitsAllocated);
            BitsStored = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.BitsStored);
            HighBit = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.HighBit);
            PixelRepresentation = DicomContentHelper.GetDicomTag<ushort>(ds, DicomTag.PixelRepresentation);

            PhotometricInterpretation = DicomContentHelper.GetDicomTag<PhotometricInterpretationCS>(ds, DicomTag.PhotometricInterpretation);
        }
    }
}
