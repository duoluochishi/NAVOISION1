using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;

namespace NV.CT.Service.AutoCali.Logic.Handlers.Scans
{
    /// <summary>
    /// 静态扫描，扫描过程中不转动机架，不移动床
    /// </summary>
    public class StaticScanHandler : FreeScanHandler
    {
        public StaticScanHandler(ILogService logger, IMessagePrintService uiLogger)
            : base(logger, uiLogger)
        {
            _logger = logger;
            _uiLogger = uiLogger;
        }

        public override FreeScanViewModel GetFreeScan(ScanReconParam scanReconParam)
        {
            var freeScan = base.GetFreeScan(scanReconParam);
            var exposureChildNodesConfig = FreeScanViewModel.CONST_StaticExposureChildNodesConfig;
            freeScan.SetExposureChildNodesConfig(exposureChildNodesConfig);//不关心子节点（机架）的Ready状态
            var exposureParams= freeScan.XDataAcquisitionParameters.ExposureParams;
            exposureParams.GantrySpeed = 0;//设置机架速度为0，才能实现机架不运动

            this._uiLogger.PrintToConsole($"[{nameof(StaticScanHandler)}] Used ExposureChildNodesConfig({exposureChildNodesConfig}), GantrySpeed({exposureParams.GantrySpeed}), GantryStartPosition({exposureParams.GantryStartPosition})， GantryEndPosition({exposureParams.GantryEndPosition})");

            return freeScan;
        }
    }
}
