using NVCTImageViewerInterop;
using RawDataHelperWrapper;
using System;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class ScanSeriesExtension
    {
        public static IEnumerable<RawData> ToEnumerableExposureData(this ScanSeries scanSeries) 
        {
            var sequenceDataList = scanSeries.SequenceDataList;

            foreach (var sequenceData in sequenceDataList)
            {
                foreach (var exposureData in sequenceData.ExpData)
                {
                    yield return exposureData;
                }
            }
        }

        public static IEnumerable<ImageRawDataInfo> ToEnumerableImageRawDataInfo(this ScanSeries scanSeries) 
        {
            var sequenceDataList = scanSeries.SequenceDataList;

            foreach (var sequenceData in sequenceDataList) 
            {
                foreach (var exposureData in sequenceData.ExpData) 
                {
                    yield return exposureData.ToImageRawDataInfo();
                }
            }
        }

        public static ImageRawDataInfo ToImageRawDataInfo(this RawData rawData) 
        {
            return new ImageRawDataInfo()
            {
                ID = rawData.Id,
                ImageSizeX = rawData.ImageSizeX,
                ImageSizeY = rawData.ImageSizeY,
                PixelType = rawData.PixelType switch
                {
                    RawDataHelperWrapper.PixelType.Ushort => NVCTImageViewerInterop.PixelType.Ushort,
                    RawDataHelperWrapper.PixelType.Float => NVCTImageViewerInterop.PixelType.Float,
                    _ => throw new ArgumentException(nameof(rawData.PixelType))
                },
                SupportInfo = new()
                {
                    SourceID = rawData.SourceId,
                    TablePosition = rawData.TablePosition,
                    GantryRotateAngle = rawData.GantryAngle,
                    FrameSeriesNumber = rawData.FrameNoInSeries,
                    Slope0 = rawData.Slope0,
                    Slope1 = rawData.Slope1
                },
                PixelData = rawData.Data
            };
        }

    }
}
