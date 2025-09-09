using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    public class DetectBoardSetTempModel : ObservableObject
    {
        private double _setValue = 26.6;
        private double _currentSetValue;
        private double _upTemp;
        private double _downTemp;
        private int _power;
        private bool _isUpValid;
        private bool _isDownValid;

        /// <summary>
        /// Index
        /// <para>从1开始</para>
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 准备给板子下发的设定值
        /// <para>单位：度(°C)</para>
        /// </summary>
        public double SetValue
        {
            get => _setValue;
            set => SetProperty(ref _setValue, value);
        }

        /// <summary>
        /// 当前已经给板子下发的设定值
        /// <para>单位：度(°C)</para>
        /// </summary>
        public double CurrentSetValue
        {
            get => _currentSetValue;
            set
            {
                if (SetProperty(ref _currentSetValue, value))
                {
                    IsUpValid = Valid(value, UpTemp);
                    IsDownValid = Valid(value, DownTemp);
                }
            }
        }

        /// <summary>
        /// 上温度
        /// <para>单位：度(°C)</para>
        /// </summary>
        public double UpTemp
        {
            get => _upTemp;
            set
            {
                if (SetProperty(ref _upTemp, value))
                {
                    IsUpValid = Valid(CurrentSetValue, value);
                }
            }
        }

        /// <summary>
        /// 下温度
        /// <para>单位：度(°C)</para>
        /// </summary>
        public double DownTemp
        {
            get => _downTemp;
            set
            {
                if (SetProperty(ref _downTemp, value))
                {
                    IsDownValid = Valid(CurrentSetValue, value);
                }
            }
        }

        /// <summary>
        /// 功率
        /// </summary>
        public int Power
        {
            get => _power;
            set => SetProperty(ref _power, value);
        }

        /// <summary>
        /// 上温度是否在设定的有效范围内
        /// </summary>
        public bool IsUpValid
        {
            get => _isUpValid;
            private set => SetProperty(ref _isUpValid, value);
        }

        /// <summary>
        /// 下温度是否在设定的有效范围内
        /// </summary>
        public bool IsDownValid
        {
            get => _isDownValid;
            private set => SetProperty(ref _isDownValid, value);
        }

        private bool Valid(double setValue, double tempValue)
        {
            var range = ((double)SystemConfig.DetectorTemperatureConfig.FloatRange).ReduceTen();
            return Math.Abs(setValue - tempValue) <= range;
        }
    }
}