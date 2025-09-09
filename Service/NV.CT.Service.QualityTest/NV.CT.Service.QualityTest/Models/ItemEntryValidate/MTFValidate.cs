using NV.CT.Service.QualityTest.Models.ItemEntryValue;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValidate
{
    public sealed class MTFValidate : ValidateBase
    {
        #region Field

        private readonly double _maxMTF0Value;
        private readonly double _maxMTF2Value;
        private readonly double _maxMTF10Value;
        private readonly double _maxMTF50Value;
        private readonly double _minMTF0Value;
        private readonly double _minMTF2Value;
        private readonly double _minMTF10Value;
        private readonly double _minMTF50Value;

        #endregion

        public double MaxMTF0Value
        {
            get => _maxMTF0Value;
            init => _maxMTF0Value = CoerceValue(value);
        }

        public double MaxMTF2Value
        {
            get => _maxMTF2Value;
            init => _maxMTF2Value = CoerceValue(value);
        }

        public double MaxMTF10Value
        {
            get => _maxMTF10Value;
            init => _maxMTF10Value = CoerceValue(value);
        }

        public double MaxMTF50Value
        {
            get => _maxMTF50Value;
            init => _maxMTF50Value = CoerceValue(value);
        }

        public double MinMTF0Value
        {
            get => _minMTF0Value;
            init => _minMTF0Value = CoerceValue(value);
        }

        public double MinMTF2Value
        {
            get => _minMTF2Value;
            init => _minMTF2Value = CoerceValue(value);
        }

        public double MinMTF10Value
        {
            get => _minMTF10Value;
            init => _minMTF10Value = CoerceValue(value);
        }

        public double MinMTF50Value
        {
            get => _minMTF50Value;
            init => _minMTF50Value = CoerceValue(value);
        }

        public double ReferenceMTF0Value { get; init; }
        public double ReferenceMTF2Value { get; init; }
        public double ReferenceMTF10Value { get; init; }
        public double ReferenceMTF50Value { get; init; }

        public bool Validate(MTFValue value)
        {
            if (value.FirstMTF0Value < MinMTF0Value || value.FirstMTF0Value > MaxMTF0Value ||
                value.FirstMTF2Value < MinMTF2Value || value.FirstMTF2Value > MaxMTF2Value ||
                value.FirstMTF10Value < MinMTF10Value || value.FirstMTF10Value > MaxMTF10Value ||
                value.FirstMTF50Value < MinMTF50Value || value.FirstMTF50Value > MaxMTF50Value)
            {
                return false;
            }

            if (value.MediumMTF0Value < MinMTF0Value || value.MediumMTF0Value > MaxMTF0Value ||
                value.MediumMTF2Value < MinMTF2Value || value.MediumMTF2Value > MaxMTF2Value ||
                value.MediumMTF10Value < MinMTF10Value || value.MediumMTF10Value > MaxMTF10Value ||
                value.MediumMTF50Value < MinMTF50Value || value.MediumMTF50Value > MaxMTF50Value)
            {
                return false;
            }

            if (value.LastMTF0Value < MinMTF0Value || value.LastMTF0Value > MaxMTF0Value ||
                value.LastMTF2Value < MinMTF2Value || value.LastMTF2Value > MaxMTF2Value ||
                value.LastMTF10Value < MinMTF10Value || value.LastMTF10Value > MaxMTF10Value ||
                value.LastMTF50Value < MinMTF50Value || value.LastMTF50Value > MaxMTF50Value)
            {
                return false;
            }

            return true;
        }
    }
}