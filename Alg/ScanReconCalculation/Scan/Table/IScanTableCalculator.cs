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


namespace NV.CT.Alg.ScanReconCalculation.Scan.Table;

public interface IScanTableCalculator
{
    public const double TimeScaleSecToUS = 1000000;

    bool CanAccept(TableControlInput input);
    
    TableControlOutput CalculateTableControlInfo(TableControlInput input);

    double GetPreOffsetD2V(TableControlInput input);

    double GetPostOffsetD2V(TableControlInput input);

    double GetPreOffsetT2D(TableControlInput input);

    double GetPostOffsetT2D(TableControlInput input);

    /// <summary>
    /// 修正扫描长度方法
    /// 根据当前扫描类型与参数 ，对扫描长度进行修正。
    /// 注意这里的长度是带符号的，即返回值应为ReconVolumnBeginPos +返回值= ReconVolumePosition
    /// 当前仅轴扫进行校正。
    /// Bolus按照轴扫单圈可重建区域返回。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    double TryCorrectReconVolumnLength(TableControlInput input);
    /// <summary>
    /// 提供一个方便使用的扫描长度计算方法
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    double TryCorrectReconVolumnLength(TableControlInput input,double newScanLength);
}
