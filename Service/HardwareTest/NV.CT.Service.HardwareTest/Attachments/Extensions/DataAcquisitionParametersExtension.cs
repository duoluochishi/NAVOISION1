using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Share.Enums;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class DataAcquisitionParametersExtension
    {
        public static DataAcquisitionParams ToProxyParam(this DataAcquisitionParameters parameters)
        {
            //代理参数
            var proxyDataAcquisitionParams = new DataAcquisitionParams();
            var proxyExposureParams = proxyDataAcquisitionParams.ExposureParams;
            var proxyDetectorParams = proxyDataAcquisitionParams.DetectorParams;
            //输入参数
            var inputExposureParams = parameters.ExposureParameters;
            var inputDetectorParams = parameters.DetectorParameters;
            //更新曝光参数
            proxyExposureParams.AutoScan = true;
            proxyExposureParams.kVs = inputExposureParams.KVs;
            proxyExposureParams.mAs = inputExposureParams.MAs;
            proxyExposureParams.ExposureTime = inputExposureParams.ExposureTime;
            proxyExposureParams.FrameTime = inputExposureParams.FrameTime;
            proxyExposureParams.TotalFrames = inputExposureParams.TotalFrames;
            proxyExposureParams.AutoDeleteNumber = inputExposureParams.AutoDeleteNumber;
            proxyExposureParams.ScanOption = inputExposureParams.ScanOption;
            proxyExposureParams.ScanMode = inputExposureParams.ScanMode;
            proxyExposureParams.ExposureMode = inputExposureParams.ExposureMode;
            proxyExposureParams.ExposureTriggerMode = inputExposureParams.ExposureTriggerMode;
            proxyExposureParams.TwelveExposureModeXRaySourceStartIndex = (uint)inputExposureParams.TwelveExposureModeXRaySourceStartIndex;
            proxyExposureParams.EnergySpectrumMode = inputExposureParams.EnergySpectrumMode;
            proxyExposureParams.Bowtie = inputExposureParams.Bowtie == CommonSwitch.Enable;
            proxyExposureParams.CollimatorOffsetEnable = inputExposureParams.CollimatorOffsetEnable == CommonSwitch.Enable;
            proxyExposureParams.CollimatorSwitch = inputExposureParams.CollimatorSwitch == CommonSwitch.Enable;
            proxyExposureParams.CollimatorOpenMode = (uint)inputExposureParams.CollimatorOpenType.ToOpenMode();
            proxyExposureParams.CollimatorOpenWidth = inputExposureParams.CollimatorOpenType.ToOpenWidth();
            proxyExposureParams.TableTriggerEnable = (uint)inputExposureParams.TableTriggerEnable;
            proxyExposureParams.FramesPerCycle = inputExposureParams.FramesPerCycle;     
            proxyExposureParams.ExposureDelayTime = inputExposureParams.ExposureDelayTime;
            proxyExposureParams.XRaySourceIndex = (uint)inputExposureParams.XRaySourceIndex;
            proxyExposureParams.Focal = inputExposureParams.Focus;
            proxyExposureParams.ExposureRelatedChildNodesConfig = inputExposureParams.ExposureRelatedChildNodesConfig;
            proxyExposureParams.SlaveDevTest = inputExposureParams.SlaveDevTest;
            proxyExposureParams.PostOffsetFrames = inputExposureParams.PostOffsetFrames;
            proxyExposureParams.PrepareTimeout = inputExposureParams.PrepareTimeout;
            proxyExposureParams.ExposureTimeout = inputExposureParams.ExposureTimeout;
            proxyExposureParams.ActiveExposureSourceCount = inputExposureParams.ActiveExposureSourceCount;

            proxyExposureParams.ECGEnable = inputExposureParams.ECGEnable is CommonSwitch.Enable;
            proxyExposureParams.ECGStartPosition = inputExposureParams.ECG_GateStartPosition;
            proxyExposureParams.ECGStopPosition = inputExposureParams.ECG_GateStopPosition;

            proxyExposureParams.GantrySpeed = inputExposureParams.ECGEnable is CommonSwitch.Enable 
                ? (uint)(inputExposureParams.ECG_ActualGantryVelocity * 100): inputExposureParams .GantryVelocity;
            proxyExposureParams.GantryStartPosition = inputExposureParams.GantryStartAngle;
            proxyExposureParams.GantryDirection = inputExposureParams.GantryDirection;
            proxyExposureParams.AllowErrorXRaySourceCount = inputExposureParams.AllowErrorXRaySourceCount;

            //更新探测器参数
            proxyDetectorParams.CurrentDoubleRead = inputDetectorParams.CurrentDoubleRead;
            proxyDetectorParams.ImageFlip = (uint)inputDetectorParams.ImageFlip;
            proxyDetectorParams.WriteConfig = (uint)inputDetectorParams.WriteConfig;
            proxyDetectorParams.CurrentImageMode = inputDetectorParams.CurrentImageMode;
            proxyDetectorParams.CurrentGain = inputDetectorParams.CurrentGain;
            proxyDetectorParams.CurrentBinning = inputDetectorParams.CurrentBinning;
            proxyDetectorParams.CurrentShutterMode = inputDetectorParams.CurrentShutterMode;
            proxyDetectorParams.DelayExposureTime = inputDetectorParams.DelayExposureTime;
            proxyDetectorParams.ExposureTime = inputExposureParams.ExposureTime;
            proxyDetectorParams.FrameTime = inputDetectorParams.FrameTime;
            proxyDetectorParams.CurrentAcquisitionMode = inputDetectorParams.CurrentAcquisitionMode;
            proxyDetectorParams.ReadDealyTime = inputDetectorParams.ReadDealyTime;
            proxyDetectorParams.HeartBeatTimeInterval = inputDetectorParams.HeartBeatTimeInterval;
            proxyDetectorParams.PrintfEnable = (uint)inputDetectorParams.PrintfEnable;
            proxyDetectorParams.TargetTemperature = inputDetectorParams.TargetTemperature;
            proxyDetectorParams.CurrentScatteringGain = inputDetectorParams.CurrentScatteringGain;
            proxyDetectorParams.CurrentSpiTime = inputDetectorParams.CurrentSpiTime;
            proxyDetectorParams.EncodeMode = inputDetectorParams.EncodeMode;
            proxyDetectorParams.RDelay = inputDetectorParams.RDelay;
            proxyDetectorParams.TDelay = inputDetectorParams.TDelay;            
            proxyDetectorParams.PreOffsetEnable = (int)inputDetectorParams.PreOffsetEnable;
            proxyDetectorParams.PreOffsetAcqTotalFrame = inputDetectorParams.PreOffsetAcqTotalFrame;
            proxyDetectorParams.PreOffsetAcqStartVaildFrame = inputDetectorParams.PreOffsetAcqStartVaildFrame;

            return proxyDataAcquisitionParams;
        }
    }
}