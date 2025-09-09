using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.MPS.Configuration;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients
{
    public class CoefficientModel : ObservableObject
    {
        private uint _voltage;
        private uint _current;
        private ObservableCollection<SourceCoefficientModel> _sources;

        public CoefficientModel()
        {
            var count = SystemConfig.SourceComponentConfig.SourceComponent.SourceCount;
            _sources = new();

            for (uint i = 1; i <= count; i++)
            {
                _sources.Add(new(i, 1f, 1f));
            }
        }

        public uint Voltage
        {
            get => _voltage;
            set => SetProperty(ref _voltage, value);
        }

        public uint Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        public ObservableCollection<SourceCoefficientModel> Sources
        {
            get => _sources;
            set => SetProperty(ref _sources, value);
        }

        public override string ToString()
        {
            return $"{Voltage,4}kV{Current,8}mA";
        }
    }
}