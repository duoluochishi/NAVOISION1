//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//  修改日期           版本号       创建人
// 2024/10/17 09:42:22    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Bolus;

/// <summary>
/// 小剂量轴扫机架旋转计算，按照不停方案。
/// NVTestBolus
/// </summary>
public class TestBolusGantryCalculator : IScanGantryCalculator
{
    public bool CanAccept(GantryControlInput input)
    {
        return input.ScanOption is ScanOption.NVTestBolus;
    }

    public GantryControlOutput GetGantryControlOutput(GantryControlInput input)
    {
        var output = new GantryControlOutput
        {
            GantrySpeed = 0,
            GantryStartPos = input.CurrentGantryPos,
            GantryEndPos = input.CurrentGantryPos,
            GantryDirection = 0,
            GantryAccTime = 0
        };

        return output;
    }
}