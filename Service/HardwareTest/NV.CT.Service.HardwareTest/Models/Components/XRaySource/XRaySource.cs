using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using NV.CT.Service.HardwareTest.Share.Enums.Components;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource
{
    public partial class XRayOriginSource : AbstractSource
    {
        [ObservableProperty]
        private uint index;
        [ObservableProperty]
        private XRaySourceStatus status = XRaySourceStatus.Online;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RealtimeInfomation))]
        private float heatCapacity;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RealtimeInfomation))]
        private float oilTemperature;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RealtimeInfomation))]
        private float voltage;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RealtimeInfomation))]
        private float current;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RealtimeInfomation))]
        private float exposureTime;
        [ObservableProperty]
        private bool isChecked;

        public string RealtimeInfomation 
            => $" {Voltage.ToString("F1")}  -  {Current.ToString("F1")}  -  {ExposureTime.ToString("F1")}  -  {HeatCapacity.ToString("F1")}  -  {OilTemperature.ToString("F1")} ";

    }
}
