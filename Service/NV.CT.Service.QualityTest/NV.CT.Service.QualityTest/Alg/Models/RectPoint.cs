namespace NV.CT.Service.QualityTest.Alg.Models
{
    internal struct RectPoint
    {
        public RectPoint()
        {
            LeftTop = new Point2D();
            RightBottom = new Point2D();
        }

        public RectPoint(Point2D leftTop, Point2D rightBottom)
        {
            LeftTop = leftTop;
            RightBottom = rightBottom;
        }

        /// <summary>
        /// 左上点坐标
        /// </summary>
        public Point2D LeftTop { get; init; }

        /// <summary>
        /// 右下点坐标
        /// </summary>
        public Point2D RightBottom { get; init; }
    }
}