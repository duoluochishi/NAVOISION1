namespace NV.CT.Service.QualityTest.Alg.Models
{
    public readonly struct Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// X
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Z
        /// </summary>
        public double Z { get; }

        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }
    }
}