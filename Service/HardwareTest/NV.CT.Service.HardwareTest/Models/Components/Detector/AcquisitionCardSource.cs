using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    public partial class AcquisitionCardSource : ObservableObject
    {
        [ObservableProperty]
        private uint serialNumber;
        [ObservableProperty]
        private PartStatus status = PartStatus.Disconnection;
        [ObservableProperty]
        private double temperature;
        [ObservableProperty]
        public string firmwareVersion = "0.0.0.0";
    }
}
