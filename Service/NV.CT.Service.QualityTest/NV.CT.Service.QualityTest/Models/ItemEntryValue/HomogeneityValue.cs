using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValue
{
    public class HomogeneityValue : ViewModelBase
    {
        #region Field

        private double _firstOClock3Value;
        private double _firstOClock6Value;
        private double _firstOClock9Value;
        private double _firstOClock12Value;
        private double _mediumOClock3Value;
        private double _mediumOClock6Value;
        private double _mediumOClock9Value;
        private double _mediumOClock12Value;
        private double _lastOClock3Value;
        private double _lastOClock6Value;
        private double _lastOClock9Value;
        private double _lastOClock12Value;

        #endregion

        public double FirstOClock3Value
        {
            get => _firstOClock3Value;
            set => SetProperty(ref _firstOClock3Value, value);
        }

        public double FirstOClock6Value
        {
            get => _firstOClock6Value;
            set => SetProperty(ref _firstOClock6Value, value);
        }

        public double FirstOClock9Value
        {
            get => _firstOClock9Value;
            set => SetProperty(ref _firstOClock9Value, value);
        }

        public double FirstOClock12Value
        {
            get => _firstOClock12Value;
            set => SetProperty(ref _firstOClock12Value, value);
        }

        public double MediumOClock3Value
        {
            get => _mediumOClock3Value;
            set => SetProperty(ref _mediumOClock3Value, value);
        }

        public double MediumOClock6Value
        {
            get => _mediumOClock6Value;
            set => SetProperty(ref _mediumOClock6Value, value);
        }

        public double MediumOClock9Value
        {
            get => _mediumOClock9Value;
            set => SetProperty(ref _mediumOClock9Value, value);
        }

        public double MediumOClock12Value
        {
            get => _mediumOClock12Value;
            set => SetProperty(ref _mediumOClock12Value, value);
        }

        public double LastOClock3Value
        {
            get => _lastOClock3Value;
            set => SetProperty(ref _lastOClock3Value, value);
        }

        public double LastOClock6Value
        {
            get => _lastOClock6Value;
            set => SetProperty(ref _lastOClock6Value, value);
        }

        public double LastOClock9Value
        {
            get => _lastOClock9Value;
            set => SetProperty(ref _lastOClock9Value, value);
        }

        public double LastOClock12Value
        {
            get => _lastOClock12Value;
            set => SetProperty(ref _lastOClock12Value, value);
        }

        public void Clear()
        {
            FirstOClock3Value = default;
            FirstOClock6Value = default;
            FirstOClock9Value = default;
            FirstOClock12Value = default;
            MediumOClock3Value = default;
            MediumOClock6Value = default;
            MediumOClock9Value = default;
            MediumOClock12Value = default;
            LastOClock3Value = default;
            LastOClock6Value = default;
            LastOClock9Value = default;
            LastOClock12Value = default;
        }
    }
}