using RawDataHelperWrapper;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    internal static class DetectorMapExtension
    {
        public static IEnumerable<NVCTImageViewerInterop.DetectorMap> ToImageViewerDetectorMaps(this IEnumerable<DetectorMap> detectorMaps) 
        {
            foreach (var inputMap in detectorMaps)
            {
                yield return new NVCTImageViewerInterop.DetectorMap()
                {
                    SourceID = inputMap.SourceID,
                    DetectorPositionList = inputMap.PositionMapList.ToImageViewerDetectorPositions().ToList()
                };
            }
        }

        public static IEnumerable<NVCTImageViewerInterop.DetectorPosition> ToImageViewerDetectorPositions(this IEnumerable<DetectorPositionMap> detectorPositionMaps) 
        {
            foreach (var inputPositionMap in detectorPositionMaps) 
            {
                yield return new NVCTImageViewerInterop.DetectorPosition() 
                {
                    DetectorID = inputPositionMap.DetectorID,
                    StartIndex = inputPositionMap.StartIndex,
                    EndIndex = inputPositionMap.EndIndex            
                };
            }
        }

    }
}
