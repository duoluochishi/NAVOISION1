using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Foundations.Abstractions
{
    public abstract partial class AbstractImageViewerAction : ObservableObject
    {
        [ObservableProperty]
        private ImageViewerActionType actionType;
        [ObservableProperty]
        private bool isChecked;
        [ObservableProperty]
        private PackIconKind icon;
        [ObservableProperty]
        private string? tooltip;
    }
}
