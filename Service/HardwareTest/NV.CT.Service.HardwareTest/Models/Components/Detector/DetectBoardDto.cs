using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    public partial class DetectBoardDto : ObservableObject
    {
        [ObservableProperty]
        private bool _using;
        [ObservableProperty]
        private uint detectorModuleID;
        [ObservableProperty]
        private string seriesNumber = string.Empty;
        [ObservableProperty]
        private DateTime installTime = DateTime.Now;
        [ObservableProperty]
        private DateTime retireTime;

        public void Retire() 
        {
            RetireTime = DateTime.Now;
            Using = false;
        }
    }
}
