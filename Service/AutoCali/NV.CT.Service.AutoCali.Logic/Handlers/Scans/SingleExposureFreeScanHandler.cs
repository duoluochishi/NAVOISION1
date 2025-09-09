using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;

namespace NV.CT.Service.AutoCali.Logic.Handlers.Scans
{
    /// <summary>
    /// 单光源曝光
    /// </summary>
    public class SingleExposureFreeScanHandler : FreeScanHandler
    {
        public SingleExposureFreeScanHandler(XRaySourceIndex xRaySourceIndex, ILogService logger, IMessagePrintService uiLogger)
            : base(logger, uiLogger)
        {
            XRaySourceIndex = xRaySourceIndex;

            _logger = logger;
            _uiLogger = uiLogger;
        }

        /// <summary>
        /// 单光源曝光，默认 1号源，0代表所有源曝光
        /// </summary>
        public XRaySourceIndex XRaySourceIndex { get; set; } = XRaySourceIndex.XRaySource01;

        public override FreeScanViewModel GetFreeScan(ScanReconParam scanReconParam)
        {
            var freeScan = base.GetFreeScan(scanReconParam);

            var exposureParams = freeScan.XDataAcquisitionParameters.ExposureParams;
            //设置单光源曝光
            exposureParams.XRaySourceIndex = (uint)XRaySourceIndex;
            this._uiLogger.PrintToConsole($"Used the XRay Source#{exposureParams.XRaySourceIndex}");

            ValidateParams(freeScan);

            return freeScan;
        }

        private bool ValidateParams(FreeScanViewModel freeScan)
        {
            var exposureParams = freeScan.XDataAcquisitionParameters.ExposureParams;
            if (exposureParams.FrameTime < MinFrameTime)
            {
                throw new ArgumentException($"FrameTime must be greater than {MinFrameTime} when Single XRay Expousre.");
            }

            return true;
        }

        /// <summary>
        /// 射线源个数
        /// </summary>
        public const uint XRaySourceCount = 24;

        /// <summary>
        /// 最小帧时间(微秒)，默认3毫秒
        /// </summary>
        private const uint MinFrameTime = 3 * XRaySourceCount;//微秒,ms
    }
}
