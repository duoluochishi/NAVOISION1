//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/14/04 14:02:21    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.DicomUtility.Graphic;

namespace NV.CT.Alg.ScanReconCalculation.Recon.Target.Base
{
    public abstract class TargetReconCalculatorBase : ITargetReconCalculator
    {
        private readonly ILogger<TargetReconCalculatorBase>? _logger;

        public TargetReconCalculatorBase()
        {
            _logger = CTS.Global.ServiceProvider?.GetService<ILogger<TargetReconCalculatorBase>>();
        }

        public abstract bool CanAccept(TargetReconInput input);

        public virtual TargetReconOutput GetTargetReconParams(TargetReconInput input)
        {
            var result = new TargetReconOutput();

            var roi_d = GetTargetReconValuesInDevice(input);
            //通用的重建是否靶重建的判断: 重建中心不在ISO中心 或 重建Fov不是指定的几种预定义FOV （该条件暂屏蔽或非全域重建，即重建开始、结束位置不是可重建区域的开始结束位置）。
            bool isCenterChanged = input.CenterFirstX != 0 || input.CenterFirstY != 0;
            bool isFoVPredefined = TargetReconCommonConfig.PredefinedFov.Any(x => x == input.FoVLengthHor);
            //var volumeMin = input.ReconVolumeStartPosition > input.ReconVolumeEndPosition ? input.ReconVolumeEndPosition : input.ReconVolumeStartPosition;
            //var volumeMax = input.ReconVolumeStartPosition > input.ReconVolumeEndPosition ? input.ReconVolumeStartPosition : input.ReconVolumeEndPosition;
            //if(roi_d.zMin < volumeMin)
            //{
            //    _logger?.LogWarning($"the recon min CenterZ in device is less than ReconVolumn min :roi_d.zMin:{roi_d.zMin} < volumeMin:{volumeMin}");
            //}
            //if(roi_d.zMax > volumeMax)
            //{
            //    _logger?.LogWarning($"the recon max CenterZ in device is greater than ReconVolumn max :roi_d.zMin:{roi_d.zMax} < volumeMax:{volumeMax}");
            //}
            //bool isFullRecon = volumeMin==roi_d.zMin && volumeMax==roi_d.zMax;

            result.IsTargetRecon = isCenterChanged || !isFoVPredefined;//|| !isFullRecon; //// 

            //保险起见，值都设置上。
            result.roiFovCenterX = roi_d.roiX;
            result.roiFovCenterY = roi_d.roiY;

            //TablePostionMin和TablePositionMax需要考虑前后删图
            //TablePositionMin对应小锥角方向偏移，TablePositionMax对应大锥角方向偏移
            result.TablePositionMin = roi_d.zMin + GetSmallSideOffsetD2V(input);
            result.TablePositionMax = roi_d.zMax + GetLargeSideOffsetD2V(input);

            //result.TotalFov = GetTotalFOV(input);
            return result;
        }        


        protected (int roiX,int roiY,int zMin,int zMax) GetTargetReconValuesInDevice(TargetReconInput input)
        {
            if (input.CenterFirstX != input.CenterLastX || input.CenterFirstY != input.CenterLastY)
            {
                _logger?.LogWarning($"CenterFirst X/Y:{input.CenterFirstX}/{input.CenterFirstY} is different to CenterLast X/Y:{ input.CenterLastX}/{ input.CenterLastY }");
            }

            var centerFirst = new Point3D(input.CenterLastX, input.CenterFirstY, input.CenterFirstZ);
            var centerLast = new Point3D(input.CenterLastX, input.CenterLastY, input.CenterLastZ);
            var centerFirst_D = CoordinateConverter.Instance.TransformPointPatientToDevice(input.PatientPosition, centerFirst);
            var centerLast_D = CoordinateConverter.Instance.TransformPointPatientToDevice(input.PatientPosition, centerLast);

            var tp_min = centerFirst_D.Z > centerLast_D.Z ? centerLast_D.Z : centerFirst_D.Z;
            var tp_max = centerFirst_D.Z > centerLast_D.Z ? centerFirst_D.Z : centerLast_D.Z;


            return ((int)centerFirst_D.X, (int)centerFirst_D.Y, (int)tp_min, (int)tp_max);
        }

        protected abstract int GetSmallSideOffsetD2V(TargetReconInput input);

        protected abstract int GetLargeSideOffsetD2V(TargetReconInput input);
    }
}
