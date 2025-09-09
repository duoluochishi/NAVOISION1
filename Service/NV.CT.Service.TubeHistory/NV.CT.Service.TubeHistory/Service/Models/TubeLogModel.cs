using System;
using System.ComponentModel;
using System.Threading.Channels;

namespace NV.CT.Service.TubeHistory.Service.Models
{
    public class ScanParam
    {
        public uint[] KV { get; set; }
        public uint[] MA { get; set; }
        public uint ExposureTime { get; set; }
        public uint FrameTime { get; set; }
        public uint ExposureDelayTime { get; set; }
        public bool AutoScan { get; set; }
        public uint FramesPerCycle { get; set; }
        public string ScanOption { get; set; }//int
        public string ScanMode { get; set; }//int
        public string ExposureMode { get; set; }//int
        public string ExposureTriggerMode { get; set; }//int
        public uint TriggerBegin { get; set; }
        public uint TriggerEnd { get; set; }
        public uint CollimatorZ { get; set; }
        public uint CollimatorOpenMode { get; set; }
        public bool BowtieEnable { get; set; }
        public uint GantryStartPos { get; set; }
        public uint GantryEndPos { get; set; }
        public uint GantryDirection { get; set; }
        public uint GantryAcceleration { get; set; }
        public uint GantryAccTime { get; set; }
        public uint GantrySpeed { get; set; }
        public uint TableStartPos { get; set; }
        public uint TableEndPos { get; set; }
        public uint ExposureStartPos { get; set; }
        public uint ExposureEndPos { get; set; }
        public string TableDirection { get; set; }//int
        public uint TableAcceleration { get; set; }
        public uint TableAccTime { get; set; }
        public uint TableHorizontal { get; set; }
        public uint TableSpeed { get; set; }
        public uint TableFeed { get; set; }
        public uint XRayFocus { get; set; }
        public uint PreVoiceId { get; set; }
        public uint PostVoiceId { get; set; }
        public uint PreVoicePlayTime { get; set; }
        public uint PreVoiceDelayTime { get; set; }
        public uint WarmUp { get; set; }
        public uint[] TubeNumber { get; set; }
        public uint PreOffsetFrame { get; set; }
        public uint PostOffsetFrame { get; set; }
        public uint ScanLoops { get; set; }
        public uint ScanLooptime { get; set; }
        public uint AutoDeleteNum { get; set; }
        public uint TotalFrames { get; set; }
        public string ScanUID { get; set; }
        public string Gain { get; set; }//int
        public string BodyCategory { get; set; }//int
        public string BodyPart { get; set; }//int
        public string Binning { get; set; }//int
        public string[] TubePosition { get; set; }//int
        public float Pitch { get; set; }
        public uint ScanNumber { get; set; }
        public string RawDataType { get; set; }//int
        public string ContrastBolusAgent { get; set; }
        public float ContrastBolusVolume { get; set; }
        public float ContrastFlowRate { get; set; }
        public float ContrastFlowDuration { get; set; }
        public float ContrastBolusIngredientConcentration { get; set; }
        public float ScanFov { get; set; }
        public string RawDataDirectory { get; set; }
        public uint CollimatorSliceWidth { get; set; }
        public uint ReconVolumeStartPos { get; set; }
        public uint ReconVolumeEndPosition { get; set; }
        public uint MultiParamInfo { get; set; }
        public uint PreOffsetFrameTime { get; set; }
        public uint TableHeight { get; set; }
        public uint RDelay { get; set; }
        public uint TDelay { get; set; }
        public uint SpotDelay { get; set; }
        public string FunctionMode { get; set; }//int
        public uint SmallAngleDeleteLength { get; set; }
        public uint LargeAngleDeleteLength { get; set; }
        public bool CollimatorOffsetEnable { get; set; }
        public string ProtocolType { get; set; }
        public string EnergySpectrumMode { get; set; }
        public uint ECGRWaveDelayTime { get; set; }
        public uint AllowErrorXRaySourceCount { get; set; }
    }
    public class DoseInfo
    {
        public float kV { get; set; }
        public float mA { get; set; }
        public float mS { get; set; }
    }
    public class TubeStatus
    {
        public float HeatCap { get; set; }
        public float OilTemp { get; set; }

    }
    public class TubeLogEntityModel
    {
        public DateTime dateTime;
        public int tubeID;
        public string tubeSN;
        public ScanParam scanParam;
        public DoseInfo doseInfo;
        public TubeStatus tubeStatusBefore;
        public TubeStatus tubeStatusAfter;
        public int viewCount;
        public int tinyArcingCount;
        public DateTime UsingBeginTime;
        public DateTime UsingEndTime;
        public TemperatureInfo tempInfoBefore;
        public TemperatureInfo tempInfoAfter;
        public TubeArcing tubeArcing;
    }
    public class TubeLife
    {
        public int tubeID { get; set; }
        public string tubeSN { get; set; }
        public float totalMs { get; set; }
        public int totalMin { get; set; }
        public int totalHour { get; set; }
        public DateTime UsingBeginTime { get; set; }
    }
    public class TemperatureInfo
    {
        public TemperatureInfo()
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Detector1 = new DetectorTemperature();
            Detector2 = new DetectorTemperature();
            Detector3 = new DetectorTemperature();
            Detector4 = new DetectorTemperature();
            Detector5 = new DetectorTemperature();
            Detector6 = new DetectorTemperature();
            Detector7 = new DetectorTemperature();
            Detector8 = new DetectorTemperature();
            Detector9 = new DetectorTemperature();
            Detector10 = new DetectorTemperature();
            Detector11 = new DetectorTemperature();
            Detector12 = new DetectorTemperature();
            Detector13 = new DetectorTemperature();
            Detector14 = new DetectorTemperature();
            Detector15 = new DetectorTemperature();
            Detector16 = new DetectorTemperature();
        }

        public string DateTime { get; set; }
        public DetectorTemperature Detector1 { get; set; }
        public DetectorTemperature Detector2 { get; set; }
        public DetectorTemperature Detector3 { get; set; }
        public DetectorTemperature Detector4 { get; set; }
        public DetectorTemperature Detector5 { get; set; }
        public DetectorTemperature Detector6 { get; set; }
        public DetectorTemperature Detector7 { get; set; }
        public DetectorTemperature Detector8 { get; set; }
        public DetectorTemperature Detector9 { get; set; }
        public DetectorTemperature Detector10 { get; set; }
        public DetectorTemperature Detector11 { get; set; }
        public DetectorTemperature Detector12 { get; set; }
        public DetectorTemperature Detector13 { get; set; }
        public DetectorTemperature Detector14 { get; set; }
        public DetectorTemperature Detector15 { get; set; }
        public DetectorTemperature Detector16 { get; set; }
    }
    public class DetectorTemperature
    {
        public DetectorTemperature()
        {
            Channel1 = new ChannelTemperature();
            Channel2 = new ChannelTemperature();
            Channel3 = new ChannelTemperature();
            Channel4 = new ChannelTemperature();
        }

        public ChannelTemperature Channel1 { get; set; }
        public ChannelTemperature Channel2 { get; set; }
        public ChannelTemperature Channel3 { get; set; }
        public ChannelTemperature Channel4 { get; set; }
        public int Humidity { get; set; }
    }
    public class ChannelTemperature
    {
        /// <summary>
        /// 处理版
        /// </summary>
        public int ProcessBoard { get; set; }

        /// <summary>
        /// 制冷片功率
        /// </summary>
        public int PwmPower { get; set; }

        /// <summary>
        /// 检出板上芯片
        /// </summary>
        public int DetectionBoardUpperChip { get; set; }

        /// <summary>
        /// 检出板下芯片
        /// </summary>
        public int DetectionBoardLowerChip { get; set; }
    }
    public class TubeArcing
    {
        public int TubeSmallArcing1 { get; set; }
        public int TubeLargeArcing1 { get; set; }
        public int preSlope1 { get; set; }
        public int TubeSmallArcing2 { get; set; }
        public int TubeLargeArcing2 { get; set; }
        public int preSlope2 { get; set; }
        public int TubeSmallArcing3 { get; set; }
        public int TubeLargeArcing3 { get; set; }
        public int preSlope3 { get; set; }
        public int skip { get; set; }
    }
}
