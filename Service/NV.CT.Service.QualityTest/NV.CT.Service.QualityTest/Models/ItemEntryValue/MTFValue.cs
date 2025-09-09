using System.Collections.Generic;
using System.Text.Json.Serialization;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.QualityTest.Alg.Models;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValue
{
    public class MTFValue : ViewModelBase
    {
        #region Field

        private double _firstMTF0Value;
        private double _firstMTF2Value;
        private double _firstMTF10Value;
        private double _firstMTF50Value;
        private double _mediumMTF0Value;
        private double _mediumMTF2Value;
        private double _mediumMTF10Value;
        private double _mediumMTF50Value;
        private double _lastMTF0Value;
        private double _lastMTF2Value;
        private double _lastMTF10Value;
        private double _lastMTF50Value;

        #endregion

        public double FirstMTF0Value
        {
            get => _firstMTF0Value;
            set => SetProperty(ref _firstMTF0Value, value);
        }

        public double FirstMTF2Value
        {
            get => _firstMTF2Value;
            set => SetProperty(ref _firstMTF2Value, value);
        }

        public double FirstMTF10Value
        {
            get => _firstMTF10Value;
            set => SetProperty(ref _firstMTF10Value, value);
        }

        public double FirstMTF50Value
        {
            get => _firstMTF50Value;
            set => SetProperty(ref _firstMTF50Value, value);
        }

        [JsonIgnore]
        internal List<Point2D> FirstMTFArray { get; set; } = new();

        public double MediumMTF0Value
        {
            get => _mediumMTF0Value;
            set => SetProperty(ref _mediumMTF0Value, value);
        }

        public double MediumMTF2Value
        {
            get => _mediumMTF2Value;
            set => SetProperty(ref _mediumMTF2Value, value);
        }

        public double MediumMTF10Value
        {
            get => _mediumMTF10Value;
            set => SetProperty(ref _mediumMTF10Value, value);
        }

        public double MediumMTF50Value
        {
            get => _mediumMTF50Value;
            set => SetProperty(ref _mediumMTF50Value, value);
        }

        [JsonIgnore]
        internal List<Point2D> MediumMTFArray { get; set; } = new();

        public double LastMTF0Value
        {
            get => _lastMTF0Value;
            set => SetProperty(ref _lastMTF0Value, value);
        }

        public double LastMTF2Value
        {
            get => _lastMTF2Value;
            set => SetProperty(ref _lastMTF2Value, value);
        }

        public double LastMTF10Value
        {
            get => _lastMTF10Value;
            set => SetProperty(ref _lastMTF10Value, value);
        }

        public double LastMTF50Value
        {
            get => _lastMTF50Value;
            set => SetProperty(ref _lastMTF50Value, value);
        }

        [JsonIgnore]
        internal List<Point2D> LastMTFArray { get; set; } = new();

        public void Clear()
        {
            FirstMTF0Value = default;
            FirstMTF2Value = default;
            FirstMTF10Value = default;
            FirstMTF50Value = default;
            FirstMTFArray.Clear();
            MediumMTF0Value = default;
            MediumMTF2Value = default;
            MediumMTF10Value = default;
            MediumMTF50Value = default;
            MediumMTFArray.Clear();
            LastMTF0Value = default;
            LastMTF2Value = default;
            LastMTF10Value = default;
            LastMTF50Value = default;
            LastMTFArray.Clear();
        }
    }
}