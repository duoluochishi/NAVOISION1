using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using NV.CT.Service.Common.Controls.Enums;

namespace NV.CT.Service.Common.Controls.Models.Foundations.Abstractions
{
    public abstract partial class AbstractImageViewerAction : ObservableObject
    {
        [ObservableProperty]
        private ImageViewerActionType actionType;
        [ObservableProperty]
        private bool isChecked;
        [ObservableProperty]
        private PackIconKind icon;
    }
}
