using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Messages;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.DataAcquisition
{
    public partial class DrawHorizontalLineViewModel : ObservableObject
    {
        private readonly ILogService logService;

        public DrawHorizontalLineViewModel(ILogService logService)
        {
            this.logService = logService;
        }

        #region Fields

        private const uint MinImageHeight = 0;
        private const uint MaxImageHeight = 288;
        private const uint MagicValue = 43;        

        #endregion

        #region Properties

        [ObservableProperty]
        private double inputAxisYValue = 0;

        partial void OnInputAxisYValueChanged(double oldValue, double newValue)
        {
            if (newValue > MaxImageHeight) 
            {
                this.InputAxisYValue = MaxImageHeight; return;
            }

            if (newValue < MinImageHeight) 
            {
                this.InputAxisYValue = MinImageHeight; return;
            }
        }

        #endregion

        [RelayCommand]
        private void DrawHorizontalLine() 
        {
            /** 传递画线消息 **/
            WeakReferenceMessenger.Default.Send(new DrawHorizontalLineMessage(InputAxisYValue));
            /** 关闭 **/
            DialogHelper.Close();
        }

    }
}
