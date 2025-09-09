using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic
{
    public partial class FreeScanViewModel : ObservableObject
    {
        public async void Simulator()
        {
            DataAcquisition_CTBoxConnectionChanged(this, new(false));
            await Task.Delay(100);
            DataAcquisition_CTBoxConnectionChanged(this, new(true));

            var device = DataAcquisitionProxy.Instance.DeviceSystem;
            //device.DoorClosed = false;
            DataAcquisition_CycleStatusChanged(this, new(device));
            await Task.Delay(100);
            //device.DoorClosed = true;
            DataAcquisition_CycleStatusChanged(this, new(device));

            await Task.Delay(3000);
            DataAcquisition_RawDataSaved(this, new RawImageSavedEventArgs() { IsFinished = false, TotalCount = 100, FinishCount = 1 });
            await Task.Delay(1000);
            DataAcquisition_RawDataSaved(this, new RawImageSavedEventArgs() { IsFinished = false, TotalCount = 100, FinishCount = 10 });
            await Task.Delay(1000);
            DataAcquisition_RawDataSaved(this, new RawImageSavedEventArgs() { IsFinished = true, TotalCount = 100, FinishCount = 100 });
        }
    }
}


