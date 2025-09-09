using NV.CT.Service.HardwareTest.Models.Components.Detector;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class DetectorSourceExtensions
    {
        public static DetectBoardsUpdateHistory ToDetectBoardsUpdateHistory(this IEnumerable<DetectBoardSource> detectBoardSources)
        {
            var detectBoardsUpdateHistory = new DetectBoardsUpdateHistory()
            {
                DetectBoardSources = detectBoardSources.ToArray()
            };
            return detectBoardsUpdateHistory;
        }

        public static IEnumerable<DetectBoardSource> ToDetectBoardSources(this DetectBoardsUpdateHistory detectBoardsUpdateHistory) 
        {
            return detectBoardsUpdateHistory.DetectBoardSources;
        }

    }
}
