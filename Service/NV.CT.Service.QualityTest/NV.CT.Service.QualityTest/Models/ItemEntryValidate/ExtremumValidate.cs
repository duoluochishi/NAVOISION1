using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValidate
{
    public sealed class ExtremumValidate : ValidateBase
    {
        #region Field

        private readonly double _maxValue;
        private readonly double _minValue;

        #endregion

        public double MaxValue
        {
            get => _maxValue;
            init => _maxValue = CoerceValue(value);
        }

        public double MinValue
        {
            get => _minValue;
            init => _minValue = CoerceValue(value);
        }

        public double ReferenceValue { get; init; }

        public bool Validate(IList<double> values)
        {
            var min = values.Min();
            var max = values.Max();
            return min >= MinValue && max <= MaxValue;
        }
    }
}