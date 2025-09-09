using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Options;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums.Components;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource
{
    public partial class XRaySourceParameters : ObservableObject
    {
        private readonly ILogService logService;
        private readonly XRaySourceConfigOptions xraySourceConfigService;

        public XRaySourceParameters()
        {
            /** get services from DI **/
            this.logService = Ioc.Default.GetRequiredService<ILogService>();
            this.xraySourceConfigService = Ioc.Default.GetRequiredService<IOptions<XRaySourceConfigOptions>>().Value; 
            /** Configure **/
            this.ConfigureTubeParameters();
        }

        private void ConfigureTubeParameters()
        {
            CycleCount = xraySourceConfigService.CycleCount;
            CycleInterval = xraySourceConfigService.CycleInterval;
            HeatCapacityUpperLimit = xraySourceConfigService.HeatCapacityUpperLimit;
            HeatCapacityLowerLimit = xraySourceConfigService.HeatCapacityLowerLimit;     
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCycleModeValid))]
        private XRaySourceTestMode currentXRaySourceTestMode = XRaySourceTestMode.Single;
        [ObservableProperty]
        private uint cycleCount = XRaySourceDefaults.CycleCount;
        [ObservableProperty]
        private uint cycleInterval = XRaySourceDefaults.CycleInterval;
        [ObservableProperty]
        private uint heatCapacityUpperLimit = XRaySourceDefaults.HeatCapacityUpperLimit;
        [ObservableProperty]
        private uint heatCapacityLowerLimit = XRaySourceDefaults.HeatCapacityLowerLimit;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCycleModeValid))]
        private bool canTestModeSwitch = true;
        public bool IsCycleModeValid => (CurrentXRaySourceTestMode == XRaySourceTestMode.Cycle) && CanTestModeSwitch;

    }
}
