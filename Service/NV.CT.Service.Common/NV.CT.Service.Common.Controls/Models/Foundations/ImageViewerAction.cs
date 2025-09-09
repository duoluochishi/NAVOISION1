using MaterialDesignThemes.Wpf;
using NV.CT.Service.Common.Controls.Enums;
using NV.CT.Service.Common.Controls.Models.Foundations.Abstractions;

namespace NV.CT.Service.Common.Controls.Models.Foundations
{
    public class ImageViewerMutexAction : AbstractImageViewerAction
    {
        public ImageViewerMutexAction(ImageViewerActionType actionType, bool isChecked, PackIconKind icon)
        {
            ActionType = actionType;
            IsChecked = isChecked;
            Icon = icon;
        }
    }

    public class ImageViewerNormalAction : AbstractImageViewerAction
    {
        public ImageViewerNormalAction(ImageViewerActionType actionType, bool isChecked, PackIconKind icon)
        {
            ActionType = actionType;
            IsChecked = isChecked;
            Icon = icon;
        }
    }

}
