using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.TubeHistory.Enums;

namespace NV.CT.Service.TubeHistory.Models
{
    public class FilterModel : ObservableObject
    {
        private bool _isTimeChecked;
        private bool _isMicroArcingCountChecked;
        private bool _isBigArcingCountChecked;
        private bool _isTubeNoChecked;
        private bool _isTubeSNChecked;
        private DateTime _startTime;
        private DateTime _endTime;
        private CompareType _microArcingCompareType;
        private int _microArcingCount;
        private CompareType _bigArcingCompareType;
        private int _bigArcingCount;
        private int _tubeNo;
        private string _tubeSN = string.Empty;

        public bool IsTimeChecked
        {
            get => _isTimeChecked;
            set => SetProperty(ref _isTimeChecked, value);
        }

        public bool IsMicroArcingCountChecked
        {
            get => _isMicroArcingCountChecked;
            set => SetProperty(ref _isMicroArcingCountChecked, value);
        }

        public bool IsBigArcingCountChecked
        {
            get => _isBigArcingCountChecked;
            set => SetProperty(ref _isBigArcingCountChecked, value);
        }

        public bool IsTubeNoChecked
        {
            get => _isTubeNoChecked;
            set => SetProperty(ref _isTubeNoChecked, value);
        }

        public bool IsTubeSNChecked
        {
            get => _isTubeSNChecked;
            set => SetProperty(ref _isTubeSNChecked, value);
        }

        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public CompareType MicroArcingCompareType
        {
            get => _microArcingCompareType;
            set => SetProperty(ref _microArcingCompareType, value);
        }

        public int MicroArcingCount
        {
            get => _microArcingCount;
            set => SetProperty(ref _microArcingCount, value);
        }

        public CompareType BigArcingCompareType
        {
            get => _bigArcingCompareType;
            set => SetProperty(ref _bigArcingCompareType, value);
        }

        public int BigArcingCount
        {
            get => _bigArcingCount;
            set => SetProperty(ref _bigArcingCount, value);
        }

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
    }
}