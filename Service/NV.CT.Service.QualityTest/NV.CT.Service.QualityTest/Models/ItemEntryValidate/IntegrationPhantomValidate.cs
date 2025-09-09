using System;
using System.Linq;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.QualityTest.Models.ItemEntryValue;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValidate
{
    public sealed class IntegrationPhantomValidate : ValidateBase
    {
        public float ZMaxValue { get; init; }
        public float ZMinValue { get; init; }
        public float CenterMaxValue { get; init; }
        public float CenterMinValue { get; init; }

        public bool Validate(IntegrationPhantomValue value)
        {
            var zMax = value.Balls.Max(i => i.Z);
            var zMin = value.Balls.Min(i => i.Z);
            var zValue = zMax - zMin;

            if (zValue < ZMinValue || zValue > ZMaxValue)
            {
                value.ErrorMsg = Quality_Lang.Quality_UC_PhantomTilt;
                return false;
            }

            var xAvg = value.Balls.Average(i => i.X);
            var yAvg = value.Balls.Average(i => i.Y);
            var xValue = Math.Abs(xAvg - (value.ImageWidth / 2d));
            var yValue = Math.Abs(yAvg - (value.ImageHeight / 2d));

            if (xValue < CenterMinValue || xValue > CenterMaxValue || yValue < CenterMinValue || yValue > CenterMaxValue)
            {
                value.ErrorMsg = Quality_Lang.Quality_UC_PhantomNoCenter;
                return false;
            }

            value.ErrorMsg = string.Empty;
            return true;
        }
    }
}