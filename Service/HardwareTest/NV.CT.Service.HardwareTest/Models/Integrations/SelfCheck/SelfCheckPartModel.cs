using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SelfCheck
{
    public partial class SelfCheckPartModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isChecked;

        [ObservableProperty]
        private string _partName = string.Empty;

        [ObservableProperty]
        private SelfCheckPartType _partType;

        [ObservableProperty]
        private SelfCheckStatus _status;

        [ObservableProperty]
        private ObservableCollection<SelfCheckPartDetailedModel> _detailedItems = [];
    }
}