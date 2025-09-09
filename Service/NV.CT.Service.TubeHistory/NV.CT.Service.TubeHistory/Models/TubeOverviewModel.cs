using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.TubeHistory.Models
{
    public class TubeOverviewModel : ObservableObject
    {
        private int _tubeNo;
        private string _tubeSN = string.Empty;
        private double _totalExposureTime;

        public int TubeNo
        {
            get => _tubeNo;
            set => SetProperty(ref _tubeNo, value);
        }

        public string TubeSN
        {
            get => _tubeSN;
            set => SetProperty(ref _tubeSN, value);
        }

        /// <summary>
        /// 曝光累计值（ms）
        /// </summary>
        public double TotalExposureTime
        {
            get => _totalExposureTime;
            set => SetProperty(ref _totalExposureTime, value);
        }
    }
}