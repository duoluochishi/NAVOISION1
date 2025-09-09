using System;
using System.IO;
using NV.MPS.ImageIO;

namespace NV.CT.Service.QualityTest.Utilities
{
    internal static class ImageControlUtility
    {
        public static bool IsRawImage(string filePath)
        {
            var strings = filePath.Split(".");
            var isRawData = strings.Length > 0 && (String.CompareOrdinal(strings[^1], "raw") == 0);
            return isRawData;
        }

        public static ImageData CreateImageData(string filePath)
        {
            ImageData imageData;
            var imageDataParser = new ImageDataParser();

            if (IsRawImage(filePath))
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var data = new byte[fileStream.Length];
                _ = fileStream.Read(data, 0, data.Length);
                fileStream.Dispose();
                imageData = imageDataParser.ParseImageByRawData(data);
            }
            else
            {
                imageData = imageDataParser.ParseImage(filePath);
            }

            return imageData;
        }
    }
}