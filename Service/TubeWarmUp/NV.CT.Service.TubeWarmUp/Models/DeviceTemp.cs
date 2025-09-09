using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.TubeWarmUp.Models
{
    public class DeviceTemp : ViewModelBase
    {
        public DeviceTemp(string name, ViewModelBase part)
        {
            this.Name = name;
            part.PropertyChanged += Part_PropertyChanged;
        }

        private void Part_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is DetectorModule detectorModule && 
                e.PropertyName == nameof(DetectorModule.MasterBoardTemp))
            {
                this.Temp = detectorModule.MasterBoardTemp;
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }
        private float temp;
        public float Temp
        {
            get { return temp; }
            set { SetProperty(ref temp, value); }
        }


    }
}
