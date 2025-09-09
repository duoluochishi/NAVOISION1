namespace NV.CT.Service.QualityTest.Alg.Models
{
    internal class PhantomBalls
    {
        /// <summary>
        /// 顶部球
        /// </summary>
        public Point3D BallTop { get; internal init; }

        /// <summary>
        /// 底部球
        /// </summary>
        public Point3D BallBottom { get; internal init; }

        /// <summary>
        /// 左侧球
        /// </summary>
        public Point3D BallLeft { get; internal init; }

        /// <summary>
        /// 右侧球
        /// </summary>
        public Point3D BallRight { get; internal init; }
    }
}