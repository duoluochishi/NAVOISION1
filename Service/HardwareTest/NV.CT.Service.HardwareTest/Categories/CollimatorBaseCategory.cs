using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Models.MotionControl.Collimator;
using System;

namespace NV.CT.Service.HardwareTest.Categories
{
    public partial class CollimatorBaseCategory : ObservableObject
    {    
        [ObservableProperty]
        public CollimatorSourceIndex sourceIndex = CollimatorSourceIndex.All;
        [ObservableProperty]
        public CollimatorMessageType messageType = CollimatorMessageType.Normal;
        [ObservableProperty]
        public CollimatorMotorType motorType = CollimatorMotorType.Bowtie;
        [ObservableProperty]
        public uint moveStep;

        partial void OnSourceIndexChanged(CollimatorSourceIndex oldValue, CollimatorSourceIndex newValue)
        {
            CollimatorSourceIndexChanged?.Invoke(newValue);
        }

        public event Action<CollimatorSourceIndex>? CollimatorSourceIndexChanged;

    }
}
