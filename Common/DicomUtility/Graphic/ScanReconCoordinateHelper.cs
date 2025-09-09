//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;
using System.Drawing.Text;
using TubePos = NV.CT.FacadeProxy.Common.Enums.TubePosition;

namespace NV.CT.DicomUtility.Graphic
{
    public static class ScanReconCoordinateHelper
    {

        /// <summary>
        /// 根据患者体位与曝光球管位置，获取定位像RTD重建方向。
        /// 根据最新与蒋老师讨论内容进行定义上的修改。
        /// </summary>
        /// <param name="pp"></param>
        /// <param name="tubePosition"></param>
        /// <returns></returns>

        public static double[] GetDefaultTopoReconOrientation(PatientPosition pp, TubePos tubePosition)
        {
            return TopoDirectionSpecification.GetTopoDirection(pp,tubePosition);
        }

        /// <summary>
        /// 根据患者体位，球管方向，床位及扫描FOV，获取定位像RTD重建参数。
        /// 范围一个三维double数组，即TOPO图像中心点位置。
        /// </summary>
        /// <returns></returns>
        public static double[] GetTopoReconParamByScanRange(PatientPosition pp, double scanStart, double scanLength, ImageOrders imageOrder)
        {
            var topoStartZ = CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(pp, scanStart);

            if (imageOrder is ImageOrders.HeadFoot)
            {
                return new double[] { 0, 0, topoStartZ - scanLength / 2 };
            }
            return new double[] { 0, 0, topoStartZ + scanLength / 2 };
        }
        public static double[] GetTopoReconParamByScanRange(PatientPosition pp, double scanStart, double scanEnd)
        {
            var topoStartZ = CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(pp, scanStart);
            var topoEndZ = CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(pp, scanEnd);

            return new double[] { 0, 0, (topoStartZ + topoEndZ) / 2 };
        }




        /// <summary>
        /// 获取不同体位下断层扫描默认重建方向
        /// </summary>
        /// <param name="pp"></param>
        /// <returns></returns>
        public static double[] GetDefaultTomoReconOrientation(PatientPosition pp)
        {
            var horizontalVecDevice = new Vector3D(1, 0, 0);
            var verticalVecDevice = new Vector3D(0, 1, 0);

            var horizontalVecPatient = CoordinateConverter.Instance.TransformVectorDeviceToPatient(pp, horizontalVecDevice);
            var verticalVecPatient = CoordinateConverter.Instance.TransformVectorDeviceToPatient(pp, verticalVecDevice);

            return new double[]{
                horizontalVecPatient.X,
                horizontalVecPatient.Y,
                horizontalVecPatient.Z,
                verticalVecPatient.X,
                verticalVecPatient.Y,
                verticalVecPatient.Z};
        }

        public static double[] GetTomoDefaultFirstLastCenterByScanRange(PatientPosition pp, ImageOrders imageOrder, double pos1, double pos2)
        {

            var tp1 = CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(pp, pos1);
            var tp2 = CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(pp, pos2);
            double[] result = new double[] { 0, 0, 0, 0, 0, 0 };
            var v1 = tp1 > tp2 ? tp1 : tp2;
            var v2 = tp1 > tp2 ? tp2 : tp1;

            if (imageOrder is ImageOrders.HeadFoot)
            {
                result[2] = v1;
                result[5] = v2;
            }
            else
            {
                result[2] = v2;
                result[5] = v1;
            }
            return result;
        }

        public static (double FirstZ, double LastZ) GetFirstLastZByScanRange(PatientPosition pp, ImageOrders imageOrder, double pos1, double pos2)
        {
            var tp1 = CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(pp, pos1);
            var tp2 = CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(pp, pos2);

            if (imageOrder is ImageOrders.HeadFoot)
            {
                return tp1 > tp2 ? (tp1, tp2) : (tp2, tp1);
            }
            return tp1 > tp2 ? (tp2, tp1) : (tp1, tp2);
        }


    }
}
