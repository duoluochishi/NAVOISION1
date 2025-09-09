using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Xml.Serialization;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    [XmlRoot("DetectBoard")]
    public partial class DetectBoardSource : ObservableObject
    {
        [ObservableProperty]
        private bool inUse;
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
            InUse = false;

        }
    }
}
