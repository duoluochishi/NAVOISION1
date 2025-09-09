using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.TubeWarmUp.Models
{
    public class Detector : ViewModelBase
    {
        private int dectorModuleLength = 16;
        public Detector()
        {
            this.DetectorModule = new ObservableCollection<DetectorModule>();
            for (int i = 0; i < dectorModuleLength; i++)
            {
                this.DetectorModule.Add(new DetectorModule(i + 1));
            }
        }
        private ObservableCollection<DetectorModule> detectorModule;

        public ObservableCollection<DetectorModule> DetectorModule
        {
            get { return detectorModule; }
            set { SetProperty(ref detectorModule, value); }
        }
    }
    public class DetectorModule : ViewModelBase
    {
        public DetectorModule(int number)
        {
            this.Number = number;
        }
        /// <summary>
        /// 编号
        /// </summary>
        private int number;

        public int Number
        {
            get { return number; }
            set { SetProperty(ref number, value); }
        }
        /// <summary>
        /// 主控板温度
        /// </summary>
        private float masterBoardTemp;

        public float MasterBoardTemp
        {
            get { return masterBoardTemp; }
            set { SetProperty(ref masterBoardTemp, value); }
        }
    }
}
