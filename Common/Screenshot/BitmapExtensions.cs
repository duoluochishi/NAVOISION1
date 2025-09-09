using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace NV.CT.Screenshot;

public static class BitmapExtensions
{
    public static BitmapSource? ToBitmapSource(this System.Drawing.Bitmap bitmap)
    {
        try
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 保存图片到文件
    /// </summary>
    /// <param name="bitmapSource">位图</param>
    /// <param name="filePath">123.png</param>
    public static void SaveToFile(this BitmapSource bitmapSource, string filePath)
    {
        BitmapEncoder encoder = new PngBitmapEncoder(); ;
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        using var stream = new FileStream(filePath, FileMode.Create);
        encoder.Save(stream);
    }
}