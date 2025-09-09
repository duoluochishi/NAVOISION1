using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients
{
    public class SourceCoefficientModel : ObservableObject
    {
        private double _kvFactor;
        private double _maFactor;

        public SourceCoefficientModel(uint id, double kvFactor, double maFactor)
        {
            Id = id;
            _kvFactor = kvFactor;
            _maFactor = maFactor;
        }

        public uint Id { get; set; }

        public double KVFactor
        {
            get => _kvFactor;
            set => SetProperty(ref _kvFactor, value);
        }

        public double MAFactor
        {
            get => _maFactor;
            set => SetProperty(ref _maFactor, value);
        }
    }
}