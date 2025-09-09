using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValue
{
    public class SingleValueValue : ViewModelBase
    {
        #region Field

        private double _firstValue;
        private double _mediumValue;
        private double _lastValue;

        #endregion

        public double FirstValue
        {
            get => _firstValue;
            set => SetProperty(ref _firstValue, value);
        }

        public double MediumValue
        {
            get => _mediumValue;
            set => SetProperty(ref _mediumValue, value);
        }

        public double LastValue
        {
            get => _lastValue;
            set => SetProperty(ref _lastValue, value);
        }

        public void Clear()
        {
            FirstValue = default;
            MediumValue = default;
            LastValue = default;
        }
    }
}