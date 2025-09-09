using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Data
{
    internal class RangeDataModel(
            SystemEnvironmentPartType partType,
            ComponentTemperatureHumidityType componentType,
            string title,
            string unit) : SystemEnvironmentDataAbstract(partType, SystemEnvironmentDataType.TemperatureHumidityRange, title)
    {
        private double _upperLimit;
        private double _lowerLimit;
        private double _value;

        public RangeDataModel(
                SystemEnvironmentPartType partType,
                ComponentTemperatureHumidityType componentType,
                string title,
                string unit,
                double upperLimit,
                double lowerLimit) : this(partType, componentType, title, unit)
        {
            UpperLimit = upperLimit;
            LowerLimit = lowerLimit;
        }

        /// <summary>
        /// 部件温湿度类型
        /// </summary>
        public ComponentTemperatureHumidityType ComponentType { get; } = componentType;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; } = unit;

        /// <summary>
        /// 上限
        /// </summary>
        public double UpperLimit
        {
            get => _upperLimit;
            set
            {
                if (SetProperty(ref _upperLimit, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// 下限
        /// </summary>
        public double LowerLimit
        {
            get => _lowerLimit;
            set
            {
                if (SetProperty(ref _lowerLimit, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// 当前值
        /// </summary>
        public double Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        /// 是否在设定的有效范围内
        /// </summary>
        public bool IsValid => Value >= LowerLimit && Value <= UpperLimit;
    }
}