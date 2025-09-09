
using NV.CT.Alg.ScanReconCalculation.Scan.Offset;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;

namespace NV.CT.Service.AutoCali.Logic
{
    public class OffsetHelper
    {
        public static void ChangePostOffsetFrames(ScanParam scanParam)
        {
            var scanOption = scanParam.ScanOption;
            var frameTime = (int)scanParam.FrameTime;
            var postOffsetFrames = OffsetCalculator.GetPostOffset(scanOption, frameTime);
            scanParam.PostOffsetFrames = (uint)postOffsetFrames;
        }
    }
}
