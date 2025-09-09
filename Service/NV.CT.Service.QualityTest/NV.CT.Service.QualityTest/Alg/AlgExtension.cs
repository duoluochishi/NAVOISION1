using NV.CT.Service.QualityTest.Alg.Models;
using NV.MPS.Native.QualityTest;

namespace NV.CT.Service.QualityTest.Alg
{
    internal static class AlgExtension
    {
        public static Point3D ToPoint3D(this BallPosition pos)
        {
            return new Point3D(pos.X, pos.Y, pos.Z);
        }

        public static Point2D ToPoint2D(this NVPOINT pos)
        {
            return new Point2D(pos.X, pos.Y);
        }

        public static RectPoint ToRectPoint(this NVRECT pos)
        {
            return new RectPoint(pos.LeftTop.ToPoint2D(), pos.RightBottom.ToPoint2D());
        }

        public static NVPOINT ToNVPOINT(this Point2D pos)
        {
            return new NVPOINT { X = (int)pos.X, Y = (int)pos.Y };
        }

        public static NVRECT ToNVRECT(this RectPoint pos)
        {
            return new NVRECT { LeftTop = pos.LeftTop.ToNVPOINT(), RightBottom = pos.RightBottom.ToNVPOINT() };
        }
    }
}