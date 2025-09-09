using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.Common.Controls.Attachments.EventArguments;
using System;

namespace NV.CT.Service.Common.Controls.ViewModels.Universal
{
    public partial class UniversalPopUpViewModel : ObservableObject
    {
        [ObservableProperty]
        private object currentShowContent = null!;

        /** 调整窗体事件 **/
        public event Action<AdjustWindowEventArgs>? AdjustWindowEvent;

        public void InvokeAdjustWindowPositionEvent(AdjustWindowEventArgs args) 
        {
            this.AdjustWindowEvent?.Invoke(args);
        }
    }
}
