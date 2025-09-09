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
    public class ImagePlaneModule : IDicomDatasetUpdater
    {
        private const string Seperator = @"\";
        public double[] ImagePosition { get; set; }
        public double[] ImageOrientation { get; set; }

        public double[] PixelSpacing { get; set; }

        public ImagePlaneModule()
        {
            ImagePosition = new double[3];
            ImageOrientation = new double[6];
            double[] ps = { 1, 1 };
            PixelSpacing = ps;
            PixelSpacing = new double[] { 1, 1 };
        }

        public void Update(DicomDataset ds)
        {
            if(ImagePosition is not null && ImagePosition.Length == 3)
            {
                ds.AddOrUpdate(DicomTag.ImagePositionPatient,
                    ImagePosition[0] + Seperator + ImagePosition[1] + Seperator + ImagePosition[2]);
            }
            if (ImageOrientation is not null && ImageOrientation.Length == 6)
            {
                ds.AddOrUpdate(DicomTag.ImageOrientationPatient,
                    string.Join(Seperator, ImageOrientation));
            }
            if (PixelSpacing is not null && PixelSpacing.Length == 2) {
                ds.AddOrUpdate(DicomTag.PixelSpacing,
                    PixelSpacing[0] + Seperator + PixelSpacing[1]);
            }
        }

        public void Read(DicomDataset ds)
        {
            ImagePosition = DicomContentHelper.GetDicomTags<double>(ds, DicomTag.ImagePositionPatient);
            ImageOrientation = DicomContentHelper.GetDicomTags<double>(ds, DicomTag.ImageOrientationPatient);
            PixelSpacing = DicomContentHelper.GetDicomTags<double>(ds, DicomTag.PixelSpacing);

        }
    }
}
