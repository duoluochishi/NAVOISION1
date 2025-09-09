using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SelfCheck
{
    public partial class SelfCheckPartDetailedModel : ObservableObject
    {
        [ObservableProperty]
        private string _itemName = string.Empty;

        [ObservableProperty]
        private DetailedSelfCheckItemType _itemType;

        [ObservableProperty]
        private SelfCheckStatus _status;
    }
}