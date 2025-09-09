using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.Common;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource
{
    public partial class XRaySourceAddKVMAPairViewModel : ObservableObject
    {
        [ObservableProperty]
        private CoefficientModel _item;

        public XRaySourceAddKVMAPairViewModel()
        {
            Item = new();
        }

        [RelayCommand]
        private void OK()
        {
            var res = WeakReferenceMessenger.Default.Send(new AddKVMAPairMessage(Item));

            if (res.Response.Item1)
            {
                DialogHelper.Close();
            }
            else
            {
                DialogService.Instance.ShowError(res.Response.Item2);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogHelper.Close();
        }
    }
}