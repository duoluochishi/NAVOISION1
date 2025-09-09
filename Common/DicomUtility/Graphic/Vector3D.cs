using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.Graphic
{
    public struct Vector3D:IEquatable<Vector3D>
    {
        public double X;
        public double Y;
        public double Z;
        public Vector3D()
        {
        }
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Vector3D other)
        {
            return X==other.X && Y==other.Y && Z==other.Z;
        }
    }
}
