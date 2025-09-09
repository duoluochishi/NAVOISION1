//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/22 13:16:32     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Buffer;
using FellowOakDicom.IO;
using FellowOakDicom.Printing;
using FellowOakDicom;
using System.Drawing.Imaging;

namespace NV.CT.DicomUtility.Transfer.PrintSCU
{
    /// <summary>
    /// 打印任务.
    /// 一次打印任务一个FilmSession，记录当前打印的张数、色彩选择、优先级、介质类型等信息。
    /// 一个FilmSession多个FilmBox，一个FilmBox对应一整张胶片内容。
    ///     FilmBox关键属性：
    ///         ImageDisplayFormat，定义了当前胶片内容的排版方式。
    ///         FilmOrientation，定义了胶片方向。（横向、纵向）
    ///         FilmSizeID，定义胶片尺寸（比如8INX10IN, 10INX12IN, 14INX17IN, 24CMX24CM）
    ///         MagnificationType，定义缩放类型，比如（BILINEAR, CUBIC, NONE, REPLICATE）
    ///         BorderDensity，定义边框颜色
    ///         EmptyImageDensity，定义空白图像密度
    /// 一个FilmBox中有多个ImageBox，其中ImageBox的个数与FilmBox的ImageDisplayFormat强相关，ImageBox个数不大于ImageDisplayFormat中布局的图像数量。
    ///     ImageBox关键属性：
    ///         ImageBoxPosition，这个应该是默认计算的，定义了当前ImageBox在FilmBox布局中的位置。
    ///         Polarity，极性，一般为Normal，若为Reverse打印图像为反色。
    ///         ImageSequence中的ImageDataset记录图像的数据相关信息。
    ///     ImageDataset内容：有灰度与彩色区别。
    ///         row,column,BitsAllocated,BitsStored,HighBit,PixelRepresentation,PlanarConfiguration,SamplePerPixel,PhotoMetricinterpretation
    /// 当前打印任务做如下约定：
    ///     1. FilmBox统一使用"STANDARD\\1,1", 只有一个ImgeBox，将当前页面中所有图像排版调整后，渲染为图片，灰度化后打印。     
    /// </summary>
    public class PrintJob
    {
        public FilmSession FilmSession { get; private set; }

        public PrintJob(string workflowID, bool isColor = false, int numberOfCopies = 1, string mediumType = "PAPER")
        {
            FilmSession = new FilmSession(DicomUID.BasicFilmSession)
            {
                FilmSessionLabel = workflowID,
                MediumType = mediumType,
                NumberOfCopies = numberOfCopies,
                IsColor = isColor,
            };
        }
        /// <summary>
        /// 根据约定，FilmBox统一使用"STANDARD\\1,1", 只有一个ImgeBox
        /// 因此简化FilmBox与ImageBox的初始化过程，
        /// </summary>
        public void FastAddImage(Bitmap img, string orientation = "PORTRAIT", string filmSize = "A4")
        {
            ///添加基本FilmBox
            var filmBox = new FilmBox(FilmSession, null, DicomTransferSyntax.ExplicitVRLittleEndian)
            {
                ImageDisplayFormat = "STANDARD\\1,1",
                FilmOrientation = orientation,
                FilmSizeID = filmSize,
                MagnificationType = "NONE",
                BorderDensity = "BLACK",
                EmptyImageDensity = "BLACK"
            };
            filmBox.Initialize();
            filmBox.Remove(DicomTag.ReferencedImageBoxSequence);

            if (FilmSession.IsColor)
            {
                AddColorImage(filmBox, img);
            }
            else
            {
                AddGreyscaleImage(filmBox, img);
            }

            FilmSession.BasicFilmBoxes.Add(filmBox);
        }


        private void AddGreyscaleImage(FilmBox filmBox, Bitmap bitmap, int index = 0)
        {
            if (filmBox is null)
            {
                throw new InvalidOperationException("Start film box first!");
            }
            if (index < 0 || index > filmBox.BasicImageBoxes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Image box index out of range");
            }

            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb
                && bitmap.PixelFormat != PixelFormat.Format32bppRgb)
            {
                throw new ArgumentException("Not supported bitmap format", nameof(bitmap));
            }

            var dataset = new DicomDataset();
            dataset.Add<ushort>(DicomTag.Columns, (ushort)bitmap.Width)
                .Add<ushort>(DicomTag.Rows, (ushort)bitmap.Height)
                .Add<ushort>(DicomTag.BitsAllocated, 8)
                .Add<ushort>(DicomTag.BitsStored, 8)
                .Add<ushort>(DicomTag.HighBit, 7)
                .Add(DicomTag.PixelRepresentation, (ushort)PixelRepresentation.Unsigned)
                .Add(DicomTag.PlanarConfiguration, (ushort)PlanarConfiguration.Interleaved)
                .Add<ushort>(DicomTag.SamplesPerPixel, 1)
                .Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Monochrome2.Value);

            var pixelData = DicomPixelData.Create(dataset, true);

            var pixels = GetGreyBytes(bitmap);
            var buffer = new MemoryByteBuffer(pixels.Data);

            pixelData.AddFrame(buffer);

            var imageBox = filmBox.BasicImageBoxes[index];
            imageBox.ImageSequence = dataset;

            pixels.Dispose();
        }
        private void AddColorImage(FilmBox filmBox, Bitmap bitmap, int index = 0)
        {
            if (filmBox is null)
            {
                throw new InvalidOperationException("Start film box first!");
            }
            if (index < 0 || index > filmBox.BasicImageBoxes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Image box index out of range");
            }

            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb
                && bitmap.PixelFormat != PixelFormat.Format32bppRgb)
            {
                throw new ArgumentException("Not supported bitmap format", nameof(bitmap));
            }

            var dataset = new DicomDataset();
            dataset.Add<ushort>(DicomTag.Columns, (ushort)bitmap.Width)
                .Add<ushort>(DicomTag.Rows, (ushort)bitmap.Height)
                .Add<ushort>(DicomTag.BitsAllocated, 8)
                .Add<ushort>(DicomTag.BitsStored, 8)
                .Add<ushort>(DicomTag.HighBit, 7)
                .Add(DicomTag.PixelRepresentation, (ushort)PixelRepresentation.Unsigned)
                .Add(DicomTag.PlanarConfiguration, (ushort)PlanarConfiguration.Interleaved)
                .Add<ushort>(DicomTag.SamplesPerPixel, 3)
                .Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);

            var pixelData = DicomPixelData.Create(dataset, true);

            var pixels = GetColorbytes(bitmap);
            var buffer = new MemoryByteBuffer(pixels.Data);

            pixelData.AddFrame(buffer);

            var imageBox = filmBox.BasicImageBoxes[index];
            imageBox.ImageSequence = dataset;

            pixels.Dispose();
        }


        private unsafe PinnedByteArray GetGreyBytes(Bitmap bitmap)
        {
            var pixels = new PinnedByteArray(bitmap.Width * bitmap.Height);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            var srcComponents = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var dstLine = (byte*)pixels.Pointer;
            var srcLine = (byte*)data.Scan0.ToPointer();

            for (int i = 0; i < data.Height; i++)
            {
                for (int j = 0; j < data.Width; j++)
                {
                    var pixel = srcLine + j * srcComponents;
                    int grey = (int)(pixel[0] * 0.3 + pixel[1] * 0.59 + pixel[2] * 0.11);
                    dstLine[j] = (byte)grey;
                }

                srcLine += data.Stride;
                dstLine += data.Width;
            }
            bitmap.UnlockBits(data);

            return pixels;
        }

        private unsafe PinnedByteArray GetColorbytes(Bitmap bitmap)
        {
            var pixels = new PinnedByteArray(bitmap.Width * bitmap.Height * 3);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            var srcComponents = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var dstLine = (byte*)pixels.Pointer;
            var srcLine = (byte*)data.Scan0.ToPointer();

            for (int i = 0; i < data.Height; i++)
            {
                for (int j = 0; j < data.Width; j++)
                {
                    var srcPixel = srcLine + j * srcComponents;
                    var dstPixel = dstLine + j * 3;

                    //convert from bgr to rgb
                    dstPixel[0] = srcPixel[2];
                    dstPixel[1] = srcPixel[1];
                    dstPixel[2] = srcPixel[0];
                }

                srcLine += data.Stride;
                dstLine += data.Width * 3;
            }
            bitmap.UnlockBits(data);

            return pixels;
        }
    }
}
