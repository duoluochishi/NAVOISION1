//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/02/02 09:42:22    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry;


public interface IScanGantryCalculator
{
    bool CanAccept(GantryControlInput input);

    GantryControlOutput GetGantryControlOutput(GantryControlInput input);

}
