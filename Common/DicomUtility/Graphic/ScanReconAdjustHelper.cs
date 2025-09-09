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
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.Graphic
{
    public static class ScanReconAdjustHelper
    {
        /// <summary>
        /// 首先调整根据扫描长度约束，对rtd重建扫描长度进行调整，然后调整相关recon
        /// </summary>
        /// <returns></returns>
        public static bool AdjustLocationParam(LocationParam[] boxes, ScanOption scanOption,double sliceThickness)
        {
            return true;
            /*
            var rtdBox = boxes.FirstOrDefault(x=>x.IsChild = false);
            if (rtdBox is null)
            {
                return false;           //当前变化参数中没有rtd，不涉及扫描长度变化,重建参数不变。
            }

            if(rtdBox.IsOffline ) {
                return false;           //当前参数中rtd isoffline，即扫描已完成。不再需要根据扫描限制调整
            }

            var rtdCenterFirstZ = rtdBox.CenterFirstZ;
            var rtdCenterLastZ = rtdBox.CenterLastZ;
            var rtdLength = rtdCenterLastZ - rtdCenterFirstZ;
            var absRtdLength = Math.Abs(rtdLength);
            var absCorrectedLength = scanOption is ScanOption.AXIAL ? ScanLengthCorrectionHelper.GetCorrectedAxialScanLength(absRtdLength) :
                ScanLengthCorrectionHelper.GetCorrectedHelicalScanLength(absRtdLength, sliceThickness);
            var correctedLength = rtdLength == absRtdLength? absCorrectedLength: -absCorrectedLength;
            var correctedCenterZ = rtdCenterFirstZ + correctedLength;

            foreach ( var x in boxes )
            {
                if(x.CenterFirstZ == rtdCenterFirstZ)
                {
                    x.CenterFirstZ = correctedCenterZ;
                }

                if(x.CenterLastZ == rtdCenterLastZ)
                {
                    x.CenterLastZ = correctedCenterZ;
                }
            }
            return true;
            */
        }
    }


}
