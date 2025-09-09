using NVCTImageViewerInterop;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.DesignPatterns
{
    internal static class DetectorMapsFactory
    {
        private const int ImageSizeX = 10240;
        private const int SourceCount = 24;
        private const int DetectorModuleCount = 64;

        public static IEnumerable<DetectorMap> CreateDetectorMaps() 
        {
            for (int i = 1; i <= SourceCount; i++) 
            {
                var detectorMap = new DetectorMap()
                {
                    SourceID = i,
                    DetectorPositionList = new()
                };

                for (int j = 1; j <= DetectorModuleCount; j++) 
                {
                    int moduleWidth = ImageSizeX / DetectorModuleCount;

                    var detectorPosition = new DetectorPosition()
                    {
                        DetectorID = j,
                        StartIndex = 1 + moduleWidth * (j - 1),
                        EndIndex = moduleWidth * j
                    };

                    detectorMap.DetectorPositionList.Add(detectorPosition);
                }

                yield return detectorMap;
            }
        }
    }
}
