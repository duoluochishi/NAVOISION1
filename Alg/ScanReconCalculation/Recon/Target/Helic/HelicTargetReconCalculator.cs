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

namespace NV.CT.Alg.ScanReconCalculation.Recon.Target.Helic
{
    public class HelicTargetReconCalculator : TargetReconCalculatorBase
    {
        public override bool CanAccept(TargetReconInput input)
        {
            return input.ScanOption is FacadeProxy.Common.Enums.ScanOption.Helical;
        }

        protected override int GetSmallSideOffsetD2V(TargetReconInput input)
        {
            //螺旋扫小锥角方向可重建区域与曝光床位之间的偏移,并额外多扫一个探测器长度
            return input.FullSW / 2 - input.SmallAngleDeleteLength;
        }

        protected override int GetLargeSideOffsetD2V(TargetReconInput input)
        {
            //螺旋扫大锥角方向可重建区域与曝光床位之间的偏移,并额外多扫一个探测器长度
            return input.FullSW / 2 - input.CollimatedSW + input.LargeAngleDeleteLength;
        }
    }

}
