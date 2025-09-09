using NV.CT.FacadeProxy.Common.Enums;
using System;

namespace NV.CT.Service.TubeWarmUp.DAL.Dtos
{
    [Serializable]
    public class ScanParamDto
    {
        public ScanParamDto()
        {
            ExposureTime = 5000;
            FrameTime = 10000;
            FramesPerCycle = 1080;
            TotalFrames = 2400;
            ScanOption = ScanOption.Axial;
            ExposureMode = ExposureMode.Three;
            TableDirection = TableDirection.In;
            TableSpeed = 0;
            TableFeed = 0;
            DelayTime = 0;
            AutoScan = true;
            ScanLength = 1000;
            PreOffsetFrames = 10;
            PostOffsetFrames = 10;
        }

        public uint ExposureTime { get; set; }
        public uint FrameTime { get; set; }
        public uint FramesPerCycle { get; set; }
        public uint TotalFrames { get; set; }
        public ScanOption ScanOption { get; set; }
        public ScanMode ScanMode { get; set; }
        public ExposureMode ExposureMode { get; set; }
        public TableDirection TableDirection { get; set; }
        public uint TableSpeed { get; set; }
        public uint TableFeed { get; set; }
        public uint DelayTime { get; set; }
        public bool AutoScan { get; set; }
        public uint ScanLength { get; set; }
        public int ScanPositionStart { get; set; }
        public int ScanPositionEnd { get; set; }
        public uint Pitch { get; set; }
        public Gain Gain { get; set; }
        public uint TableHeight { get; set; }
        public uint VoiceId { get; set; }
        public uint PreOffsetFrames { get; set; }
        public uint PostOffsetFrames { get; set; }
        public uint CollimitorX { get; set; }
        public uint CollimitorZ { get; set; }
    }
}