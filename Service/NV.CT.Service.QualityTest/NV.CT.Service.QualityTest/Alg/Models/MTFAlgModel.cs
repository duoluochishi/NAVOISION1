using System;

namespace NV.CT.Service.QualityTest.Alg.Models
{
    internal class MTFAlgModel
    {
        /// <summary>
        /// MTF=0% 对应线对数值
        /// </summary>
        public double MTF0 { get; internal init; }

        /// <summary>
        /// MTF=2% 对应线对数值
        /// </summary>
        public double MTF2 { get; internal init; }

        /// <summary>
        /// MTF=10% 对应线对数值
        /// </summary>
        public double MTF10 { get; internal init; }

        /// <summary>
        /// MTF=50% 对应线对数值
        /// </summary>
        public double MTF50 { get; internal init; }

        /// <summary>
        /// MTF数组
        /// </summary>
        public Point2D[] MTFArray { get; internal init; } = Array.Empty<Point2D>();
    }
}