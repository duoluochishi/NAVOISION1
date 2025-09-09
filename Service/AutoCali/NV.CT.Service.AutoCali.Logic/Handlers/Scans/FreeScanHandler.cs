using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;

namespace NV.CT.Service.AutoCali.Logic.Handlers.Scans
{
    /// <summary>
    /// 自由协议扫描处理逻辑
    /// </summary>
    public class FreeScanHandler
    {
        public FreeScanHandler(ILogService logger, IMessagePrintService uiLogger)
        {
            _logger = logger;
            _uiLogger = uiLogger;
        }

        public virtual FreeScanViewModel GetFreeScan(ScanReconParam scanReconParam)
        {
            ScanParam scanParam = scanReconParam.ScanParameter;
            var freeScan = CreateFreeScanViewModel(scanReconParam);
            freeScan.RegisterProxyEvents();

            var freeScanParams = freeScan.XDataAcquisitionParameters;

            freeScanParams.FuncMode = scanParam.FunctionMode;//
            freeScanParams.XRawDataType = scanParam.RawDataType;

            freeScanParams.StudyUID = scanReconParam.Study.StudyInstanceUID;
            freeScanParams.ScanUID = scanParam.ScanUID;

            #region Exposure Params

            var exposureParams = freeScanParams.ExposureParams;
            exposureParams.ScanMode = scanParam.ScanMode;
            exposureParams.kVs[0] = scanParam.kV[0];
            exposureParams.mAs[0] = (uint)(scanParam.mA[0] / 1000.0f);

            exposureParams.TotalFrames = scanParam.TotalFrames;
            exposureParams.FramesPerCycle = scanParam.FramesPerCycle;
            exposureParams.PostOffsetFrames = scanParam.PostOffsetFrames;

            //不再对上游用户提供接口配置是否关心限束器
            //parameters.ExposureParams.BowtieSwitch = bowtieSwitch;
            exposureParams.Bowtie = scanParam.BowtieEnable;
            exposureParams.ExposureMode = scanParam.ExposureMode;

            exposureParams.FrameTime = scanParam.FrameTime / 1000.0f;
            exposureParams.ExposureTime = scanParam.ExposureTime / 1000.0f;

            exposureParams.Focal = scanParam.Focal;

            var CollimitorZ = scanParam.CollimatorZ;
            exposureParams.CollimatorOpenWidth = CollimitorZ;

            //[todo]需要使用配置来下参是否关心限束器
            exposureParams.CollimatorSwitch = true;
            //parameters.ExposureParams.CollimatorSwitch = collimatorSwitch;//是否下发CollimatroZ参数
            //开口模式，1:基于小锥角模式（256,242,128,64）；2：基于中心位置模式（288，128,64）
            exposureParams.CollimatorOpenMode = 288 == CollimitorZ ? 2u : 1;

            //机架的起始位置
            exposureParams.GantryDirection = GantryDirection.Clockwise;
            exposureParams.GantryStartPosition = scanParam.GantryStartPosition;
            exposureParams.GantrySpeed = scanParam.GantrySpeed;
            exposureParams.GantryAccTime = scanParam.GantryAccelerationTime;
            #endregion Exposure Params

            #region Detector Params

            var detectorParams = freeScanParams.DetectorParams;
            //nvSync Delay control
            detectorParams.RDelay = scanParam.RDelay / 1000.0f;
            detectorParams.TDelay = scanParam.TDelay / 1000.0f;
            detectorParams.SpotDelay = scanParam.SpotDelay / 1000.0f;

            detectorParams.FrameTime = scanParam.FrameTime / 1000.0f;
            detectorParams.ExposureTime = scanParam.ExposureTime / 1000.0f;

            detectorParams.CurrentGain = scanParam.Gain;
            detectorParams.CurrentScatteringGain = ScatteringDetectorGain_Default;

            var preOffsetFrames = scanParam.PreOffsetFrames;
            if (preOffsetFrames > 1)
            {
                detectorParams.PreOffsetEnable = 1;
                detectorParams.PreOffsetAcqTotalFrame = (int)preOffsetFrames;
                detectorParams.PreOffsetAcqStartVaildFrame = 3;//default
            }

            #endregion Detector Params

            return freeScan;
        }

        protected virtual FreeScanViewModel CreateFreeScanViewModel(ScanReconParam scanReconParam)
        {
            ScanParam scanParam = scanReconParam.ScanParameter;
            bool isDark = scanParam.kV[0] == 0;

            var freeScan = new FreeScanViewModel(_logger, _uiLogger);
            if (isDark)
            {
                freeScan.SetExposureChildNodesConfig(FreeScanViewModel.CONST_DarkExposureChildNodesConfig);
            }
            return freeScan;
        }

        /// <summary>
        /// 散射探测器使用的增益模式默认值：Fix16Pc
        /// </summary>
        public static ScatteringDetectorGain ScatteringDetectorGain_Default = ScatteringDetectorGain.Fix16Pc;

        protected ILogService _logger;
        protected IMessagePrintService _uiLogger;
    }
}
