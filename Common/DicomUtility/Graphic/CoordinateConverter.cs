//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2024/11/27 14:41:44           V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.Graphic
{
    /// <summary>
    /// 默认情况下的坐标转换。
    /// 支持根据病人体位，在设备坐标系与病人坐标系的互相转换。
    /// 支持床位，点，向量的转换。
    /// </summary>
    public class CoordinateConverter
    {

        private static CoordinateConverter _instance;

        public static CoordinateConverter Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new CoordinateConverter();
                }
                return _instance;
            }
        }

        private Dictionary<PatientPosition, Matrix<double>> _TransformDictionary;
        private Dictionary<PatientPosition, Matrix<double>> _InvertTransformDictionary;


        private CoordinateConverter()
        {
            _TransformDictionary = new Dictionary<PatientPosition, Matrix<double>>();
            _InvertTransformDictionary = new Dictionary<PatientPosition, Matrix<double>>();
            InitMatrixTramsformForDifferentPatientPosition();
        }

        public (double X,double Y,double Z) TransformPointDeviceToPatient(PatientPosition pp, double x, double y, double z)
        {
            Vector<double> input = DenseVector.OfArray(new double[] { x, y, z, 1 });

            // 进行坐标转换
            Vector<double> result = _TransformDictionary[pp] * input;

            return (result[0], result[1], result[2]);
        }

        public Point3D TransformPointDeviceToPatient(PatientPosition pp, Point3D p)
        {
            var result = TransformPointDeviceToPatient(pp, p.X, p.Y, p.Z);
            return new Point3D(result.X, result.Y, result.Z);
        }

        public (double vx,double vy,double vz) TransformVectorDeviceToPatient(PatientPosition pp, double x,double y,double z)
        {
            Vector<double> input = DenseVector.OfArray(new double[] { x, y, z,0 });

            // 进行坐标转换
            Vector<double> result = _TransformDictionary[pp] * input;

            return (result[0], result[1], result[2]);
        }

        public Vector3D TransformVectorDeviceToPatient(PatientPosition pp, Vector3D v )
        {
            var result = TransformVectorDeviceToPatient(pp, v.X, v.Y, v.Z);
            return new Vector3D(result.vx, result.vy, result.vz);
        }

        public double TransformSliceLocationDeviceToPatient(PatientPosition pp, double sliceLocation)
        {
            return TransformPointDeviceToPatient(pp, 0, 0, sliceLocation).Z;
        }

        public (double X, double Y, double Z) TransformPointPatientToDevice(PatientPosition pp, double x, double y, double z)
        {
            Vector<double> input = DenseVector.OfArray(new double[] { x, y, z, 1 });

            // 进行坐标转换
            Vector<double> result = _InvertTransformDictionary[pp] * input;

            // 将齐次坐标转换回普通坐标
            //Vector<double> resultPoint = DenseVector.OfArray(new double[] { result[0], result[1], result[2] });

            return (result[0], result[1], result[2]);
        }

        public Point3D TransformPointPatientToDevice(PatientPosition pp, Point3D p)
        {
            var result = TransformPointPatientToDevice(pp, p.X, p.Y, p.Z);
            return new Point3D(result.X, result.Y, result.Z);
        }


        public (double vx, double vy, double vz) TransformVectorPatientToDevice(PatientPosition pp, double x, double y, double z)
        {
            Vector<double> input = DenseVector.OfArray(new double[] { x, y, z, 0 });
            // 进行坐标转换
            Vector<double> result = _InvertTransformDictionary[pp] * input;

            return (result[0], result[1], result[2]);
        }


        public Vector3D TransformVectorPatientToDevice(PatientPosition pp, Vector3D v)
        {
            var result = TransformVectorPatientToDevice(pp, v.X, v.Y, v.Z);
            return new Vector3D(result.vx, result.vy, result.vz);
        }

        public double TransformSliceLocationPatientToDevice(PatientPosition pp, double sliceLocation)
        {
            return TransformPointPatientToDevice(pp, 0, 0, sliceLocation).Z;
        }

        public TableDirection GetTableDirectionByImageOrder(PatientPosition pp, ImageOrders imageOrder)
        {
            var vz = imageOrder is ImageOrders.HeadFoot ? 1 : -1;

            var dirInDevice = TransformVectorPatientToDevice(pp, 0,0,vz);

            return dirInDevice.vz > 0 ? TableDirection.In : TableDirection.Out;
        }

        public ImageOrders GetImageOrderByTableDirection(PatientPosition pp, TableDirection tableDirection)
        {
            var vz = tableDirection is TableDirection.In ?  -1 :  1;
            var dirInPatient = TransformVectorDeviceToPatient(pp, 0,0, vz);

            return dirInPatient.vz > 0 ? ImageOrders.FootHead : ImageOrders.HeadFoot;
        }

        private void InitMatrixTramsformForDifferentPatientPosition()
        {
            _TransformDictionary.Add(PatientPosition.HFS, GetPatientPositionTransform(PatientPosition.HFS));
            _TransformDictionary.Add(PatientPosition.HFP, GetPatientPositionTransform(PatientPosition.HFP));
            _TransformDictionary.Add(PatientPosition.HFDL, GetPatientPositionTransform(PatientPosition.HFDL));
            _TransformDictionary.Add(PatientPosition.HFDR, GetPatientPositionTransform(PatientPosition.HFDR));
            _TransformDictionary.Add(PatientPosition.FFS, GetPatientPositionTransform(PatientPosition.FFS));
            _TransformDictionary.Add(PatientPosition.FFP, GetPatientPositionTransform(PatientPosition.FFP));
            _TransformDictionary.Add(PatientPosition.FFDL, GetPatientPositionTransform(PatientPosition.FFDL));
            _TransformDictionary.Add(PatientPosition.FFDR, GetPatientPositionTransform(PatientPosition.FFDR));

            _InvertTransformDictionary.Add(PatientPosition.HFS, GetPatientPositionTransform(PatientPosition.HFS));
            _InvertTransformDictionary.Add(PatientPosition.HFP, GetPatientPositionTransform(PatientPosition.HFP));
            _InvertTransformDictionary.Add(PatientPosition.HFDL, GetPatientPositionTransform(PatientPosition.HFDL));
            _InvertTransformDictionary.Add(PatientPosition.HFDR, GetPatientPositionTransform(PatientPosition.HFDR));
            _InvertTransformDictionary.Add(PatientPosition.FFS, GetPatientPositionTransform(PatientPosition.FFS));
            _InvertTransformDictionary.Add(PatientPosition.FFP, GetPatientPositionTransform(PatientPosition.FFP));
            _InvertTransformDictionary.Add(PatientPosition.FFDL, GetPatientPositionTransform(PatientPosition.FFDL));
            _InvertTransformDictionary.Add(PatientPosition.FFDR, GetPatientPositionTransform(PatientPosition.FFDR));

            foreach (var key in _InvertTransformDictionary.Keys)
            {
                var item = _InvertTransformDictionary[key];

                _InvertTransformDictionary[key] = item.Inverse();
            }
        }


        private Matrix<double> GetPatientPositionTransform(PatientPosition pp)
        {
            switch (pp)
            {
                case PatientPosition.HFS:
                    return _MatrixDevice * _MatrixHFS;
                case PatientPosition.HFP:
                    return _MatrixDevice * _MatrixHFP;
                case PatientPosition.HFDL:
                    return _MatrixDevice * _MatrixHFDL;
                case PatientPosition.HFDR:
                    return _MatrixDevice * _MatrixHFDR;
                case PatientPosition.FFS:
                    return _MatrixDevice * _MatrixFFS;
                case PatientPosition.FFP:
                    return _MatrixDevice * _MatrixFFP;
                case PatientPosition.FFDL:
                    return _MatrixDevice * _MatrixFFDL;
                case PatientPosition.FFDR:
                    return _MatrixDevice * _MatrixFFDR;
                default:
                    throw new Exception($"{pp} is not an acceptable PatientPosition");
            }
        }

        private Matrix<double> _MatrixHFS = DenseMatrix.OfArray(new double[,]{
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, 1 } });

        private Matrix<double> _MatrixHFP = DenseMatrix.OfArray(new double[,]{
           { -1, 0, 0, 0},
           { 0, -1, 0, 0},
           { 0, 0, 1, 0},
           { 0, 0, 0, 1}});
        private Matrix<double> _MatrixHFDL = DenseMatrix.OfArray(new double[,]{
           { 0, 1, 0, 0},
           { -1, 0, 0, 0},
           { 0, 0, 1, 0},
           { 0, 0, 0, 1}});
        private Matrix<double> _MatrixHFDR = DenseMatrix.OfArray(new double[,]{
           { 0, -1, 0, 0},
           { 1, 0, 0, 0},
           { 0, 0, 1, 0},
           { 0, 0, 0, 1}});
        private Matrix<double> _MatrixFFS = DenseMatrix.OfArray(new double[,]{
           { -1, 0, 0, 0},
           { 0, 1, 0, 0},
           { 0, 0, -1, 0},
           { 0, 0, 0, 1}});
        private Matrix<double> _MatrixFFP = DenseMatrix.OfArray(new double[,]{
           { 1, 0, 0, 0},
           { 0, -1, 0, 0},
           { 0, 0, -1, 0},
           { 0, 0, 0, 1}});
        private Matrix<double> _MatrixFFDL = DenseMatrix.OfArray(new double[,]{
           { 0, 1, 0, 0},
           { 1, 0, 0, 0},
           { 0, 0, -1, 0},
           { 0, 0, 0, 1}});
        private Matrix<double> _MatrixFFDR = DenseMatrix.OfArray(new double[,]{
           { 0, -1, 0, 0},
           { -1, 0, 0, 0},
           { 0, 0, -1, 0},
           { 0, 0, 0, 1}});

        private Matrix<double> _MatrixDevice = DenseMatrix.OfArray(new double[,]{
           { 1, 0, 0, 0},
           { 0, 1, 0, 0},
           { 0, 0, 1, 0},
           { 0, 0, 0, 1}});
    }
}
