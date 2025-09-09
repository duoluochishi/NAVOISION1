using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    public class DetectorTargetTemperatureModel : ObservableObject
    {
        /// <summary>
        /// 探测器模组编号
        /// <para>从1开始</para>
        /// </summary>
        public int DetectorIndex { get; init; }

        public DetectBoardSetTempModel[] Detects { get; init; } = [];
    }
}