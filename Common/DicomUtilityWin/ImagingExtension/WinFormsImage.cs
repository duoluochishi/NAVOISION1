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
using FellowOakDicom.Imaging.Render;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO;
using System.Drawing.Imaging;

namespace NV.CT.DicomUtility.ImagingExtension
{
    public sealed class WinFormsImage : ImageDisposableBase<Bitmap>
    {
        public WinFormsImage(int width, int height)
            : base(width, height, new PinnedIntArray(width * height), (Bitmap)null)
        {
        }

        private WinFormsImage(int width, int height, PinnedIntArray pixels, Bitmap image)
            : base(width, height, pixels, image)
        {
        }

        public override void Render(int components, bool flipX, bool flipY, int rotation)
        {
            PixelFormat format = ((components == 4) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb);
            int stride = GetStride(Width, format);
            _image = new Bitmap(Width, Height, stride, format, Pixels.Pointer);
            RotateFlipType rotateFlipType = GetRotateFlipType(flipX, flipY, rotation);
            if (rotateFlipType != 0)
            {
                _image.RotateFlip(rotateFlipType);
            }
        }

        public override void DrawGraphics(IEnumerable<IGraphic> graphics)
        {
            using Graphics graphics2 = Graphics.FromImage(base._image);
            foreach (IGraphic graphic in graphics)
            {
                Image image = graphic.RenderImage(null).As<Image>();
                graphics2.DrawImage(image, graphic.ScaledOffsetX, graphic.ScaledOffsetY, graphic.ScaledWidth, graphic.ScaledHeight);
            }
        }

        public override IImage Clone()
        {
            return new WinFormsImage(Width, Height, new PinnedIntArray(Pixels.Data), (_image is null) ? null : new Bitmap(_image));
        }

        private static int GetStride(int width, PixelFormat format)
        {
            int num = (((int)(format & (PixelFormat)65280) >> 8) + 7) / 8;
            return 4 * ((width * num + 3) / 4);
        }

        private static RotateFlipType GetRotateFlipType(bool flipX, bool flipY, int rotation)
        {
            if (flipX && flipY)
            {
                return rotation switch
                {
                    90 => RotateFlipType.Rotate270FlipNone,
                    180 => RotateFlipType.RotateNoneFlipNone,
                    270 => RotateFlipType.Rotate90FlipNone,
                    _ => RotateFlipType.Rotate180FlipNone,
                };
            }

            if (flipX)
            {
                return rotation switch
                {
                    90 => RotateFlipType.Rotate90FlipX,
                    180 => RotateFlipType.Rotate180FlipX,
                    270 => RotateFlipType.Rotate270FlipX,
                    _ => RotateFlipType.RotateNoneFlipX,
                };
            }

            if (flipY)
            {
                return rotation switch
                {
                    90 => RotateFlipType.Rotate270FlipX,
                    180 => RotateFlipType.RotateNoneFlipX,
                    270 => RotateFlipType.Rotate90FlipX,
                    _ => RotateFlipType.Rotate180FlipX,
                };
            }

            return rotation switch
            {
                90 => RotateFlipType.Rotate90FlipNone,
                180 => RotateFlipType.Rotate180FlipNone,
                270 => RotateFlipType.Rotate270FlipNone,
                _ => RotateFlipType.RotateNoneFlipNone,
            };
        }
    }
}
