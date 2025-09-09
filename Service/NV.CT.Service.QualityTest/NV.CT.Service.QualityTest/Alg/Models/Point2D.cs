namespace NV.CT.Service.QualityTest.Alg.Models
{
    public readonly struct Point2D
    {
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// X
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y
        /// </summary>
        public double Y { get; }

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }
}