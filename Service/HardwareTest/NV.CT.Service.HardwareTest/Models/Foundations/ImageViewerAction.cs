using MaterialDesignThemes.Wpf;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Foundations
{
    public class ImageViewerMutexAction : AbstractImageViewerAction
    {
        public ImageViewerMutexAction(ImageViewerActionType actionType, bool isChecked, PackIconKind icon, string tooltip)
        {
            ActionType = actionType;
            IsChecked = isChecked;
            Icon = icon;
            Tooltip = tooltip;
        }
    }

    public class ImageViewerNormalAction : AbstractImageViewerAction
    {
        public ImageViewerNormalAction(ImageViewerActionType actionType, bool isChecked, PackIconKind icon, string tooltip)
        {
            ActionType = actionType;
            IsChecked = isChecked;
            Icon = icon;
            Tooltip = tooltip;
        }
    }

}
