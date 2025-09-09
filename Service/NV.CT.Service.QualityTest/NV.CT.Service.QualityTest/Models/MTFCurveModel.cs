using System.Collections.Generic;
using NV.CT.Service.QualityTest.Alg.Models;

namespace NV.CT.Service.QualityTest.Models
{
    public class MTFCurveModel
    {
        public double MTF0Value { get; init; }
        public double MTF2Value { get; init; }
        public double MTF10Value { get; init; }
        public double MTF50Value { get; init; }
        public List<Point2D> LastMTFArray { get; init; } = new();
    }
}