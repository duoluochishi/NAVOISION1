using RawDataHelperWrapper;
using System;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    /// <summary>
    /// 图像矩形
    /// </summary>
    public struct ImageRectangle
    {
        public int top;
        public int bottom;
        public int left;
        public int right;
    }

    /// <summary>
    /// 图像的像素值的动态范围
    /// </summary>
    public struct ImageDynamicRange
    {
        public float Max;
        public float Min;
    }

    /// <summary>
    /// 图像质量指标
    /// </summary>
    public struct ImageQualityIndex
    {
        public float average;
        public float standardDeviation;
        public float signalNoiseRatio;
    }

    public static class ImageUniversalHelper
    {

        /// <summary>
        /// 计算动态范围和质量指标
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="rawDataInfo"></param>
        /// <returns></returns>
        public unsafe static (ImageDynamicRange, ImageQualityIndex) CalculateImageDynamicRangeAndQualityIndex(
            ImageRectangle rectangle, RawData rawDataInfo)
        {
            ImageDynamicRange imageDynamicRange = new ImageDynamicRange();
            ImageQualityIndex imageQualityIndex = new ImageQualityIndex();

            int width = rectangle.right - rectangle.left;
            int height = rectangle.bottom - rectangle.top;

            float[] temp = new float[width * height];

            if (rawDataInfo.PixelType == PixelType.Ushort)
            {
                var dataSpan = new Span<ushort>(rawDataInfo.Data.ToPointer(), rawDataInfo.ImageSizeX * rawDataInfo.ImageSizeY);

                for (int i = rectangle.top; i < rectangle.bottom; i++)
                {
                    for (int j = rectangle.left; j < rectangle.right; j++)
                    {
                        temp[(i - rectangle.top) * width + (j - rectangle.left)] = dataSpan[i * rawDataInfo.ImageSizeX + j];
                    }
                }
            }
            else if (rawDataInfo.PixelType == PixelType.Float) 
            {
                var dataSpan = new Span<float>(rawDataInfo.Data.ToPointer(), rawDataInfo.ImageSizeX * rawDataInfo.ImageSizeY);

                for (int i = rectangle.top; i < rectangle.bottom; i++)
                {
                    for (int j = rectangle.left; j < rectangle.right; j++)
                    {
                        temp[(i - rectangle.top) * width + (j - rectangle.left)] = dataSpan[i * rawDataInfo.ImageSizeX + j];
                    }
                }
            }

            imageDynamicRange.Min = temp.Min();
            imageDynamicRange.Max = temp.Max();
            //计算质量指标 - 均值
            imageQualityIndex.average = temp.Average();
            //计算质量指标 - 标准差
            double sum = 0;
            for (int i = 0; i < width * height; i++) 
            {
                sum += (imageQualityIndex.average - temp[i]) * (imageQualityIndex.average - temp[i]);
            }
            imageQualityIndex.standardDeviation = (float)Math.Sqrt(sum / temp.Length);
            //计算质量指标 - 信噪比
            imageQualityIndex.signalNoiseRatio = (float)(20 * Math.Log10(imageQualityIndex.average / imageQualityIndex.standardDeviation));

            return (imageDynamicRange, imageQualityIndex);
        }

    }

}
