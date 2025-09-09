using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace NV.CT.Service.HardwareTest.Models.Components.Collimator
{
    public partial class CollimatorCalibrationIterativeResult : ObservableObject
    {
        /** 限束器编号 **/
        [ObservableProperty]
        private string collimatorName = string.Empty;
        /** 限束器码值迭代结果 **/
        [ObservableProperty]     
        private int iterativceCollimatorPosition;
        /** 限束器在探测器位置投影迭代结果 **/
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Offset))]
        [NotifyPropertyChangedFor(nameof(CalibrationCompleted))]
        private double iterativceDetectorPosition; 
        /** 校准目标位置 **/
        public double TargetDetectorPosition { get; set; } = 288;
        /** 是否完成校准 **/
        public bool CalibrationCompleted => Math.Abs(IterativceDetectorPosition - TargetDetectorPosition) < 1;
        /** 偏差 **/
        public double Offset => IterativceDetectorPosition - TargetDetectorPosition;

        public void Reset() 
        {
            TargetDetectorPosition = 288;
            IterativceCollimatorPosition = 0;
            IterativceDetectorPosition = 0;            
        }
    }
}
