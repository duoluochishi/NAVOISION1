using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums.ShieldableComponent;

namespace NV.CT.Service.HardwareTest.Models.Integrations.ComponentEnablement
{
    public class ComponentModel : ObservableObject
    {
        private bool _isAllEnabled;

        public bool IsAllEnabled
        {
            get => _isAllEnabled;
            set => SetProperty(ref _isAllEnabled, value);
        }

        public required string Name { get; init; }
        public required ShieldableComponentType ComponentType { get; init; }
        public required DetailedEnableModel[] DetailedItems { get; init; }
    }
}