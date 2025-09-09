using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.TubeCali.Enums;

namespace NV.CT.Service.TubeCali.Models
{
    public class SourceComponent : ObservableObject
    {
        private bool _isChecked;
        private ComponentCaliStatus _status;
        private float _oilTemperature;
        private float _thermalCapacity;
        private float _voltage;
        private float _current;
        private float _exposureTime;

        /// <summary>
        ///  射线源编号，从1开始
        /// </summary>
        public int Number { get; init; }

        /// <summary>
        /// 所属的Tube板编号，从1开始
        /// </summary>
        public int NumberOfTubeInterface { get; init; }

        /// <summary>
        /// 在所属的Tube板内的射线源编号，从1开始
        /// </summary>
        public int NumberInTubeInterface { get; init; }

        /// <summary>
        /// 各寄存器地址
        /// </summary>
        public TubeAddress Address { get; init; } = null!;

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public ComponentCaliStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// 油温
        /// <para>单位：度(°C)</para>
        /// </summary>
        public float OilTemperature
        {
            get => _oilTemperature;
            set => SetProperty(ref _oilTemperature, value);
        }

        /// <summary>
        /// 热容量
        /// </summary>
        public float ThermalCapacity
        {
            get => _thermalCapacity;
            set => SetProperty(ref _thermalCapacity, value);
        }

        /// <summary>
        /// 电压
        /// <para>单位：kV</para>
        /// </summary>
        public float Voltage
        {
            get => _voltage;
            set => SetProperty(ref _voltage, value);
        }

        /// <summary>
        /// 电流
        /// <para>单位：mA</para>
        /// </summary>
        public float Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        /// <summary>
        /// Exposure Time
        /// <para>单位：ms</para>
        /// </summary>
        public float ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }
    }
}