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

using NV.CT.Alg.ScanReconCalculation.Recon.Target.Base;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Recon.Target.Axial
{
    public class AxialTargetReconCalculator : TargetReconCalculatorBase
    {

        public override bool CanAccept(TargetReconInput input)
        {
            return input.ScanOption is 
                ScanOption.Axial 
                or ScanOption.NVTestBolus 
                or ScanOption.NVTestBolusBase 
                or ScanOption.TestBolus;
        }

        protected override int GetSmallSideOffsetD2V(TargetReconInput input)
        {
            //轴扫小锥角方向可重建区域与曝光床位之间的偏移
            return input.FullSW / 2 - input.SmallAngleDeleteLength;
        }
        protected override int GetLargeSideOffsetD2V(TargetReconInput input)
        {
            //轴扫大锥角方向可重建区域与曝光床位之间的偏移
            return input.FullSW / 2 - input.CollimatedSW + input.LargeAngleDeleteLength;
        }
    }
}
