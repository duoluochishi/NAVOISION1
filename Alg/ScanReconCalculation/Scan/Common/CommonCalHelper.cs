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

using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Configuration;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Common;


/// <summary>
/// 通用计算Helper
/// </summary>
public static class CommonCalHelper
{
    public const double DoubleTollerance = 0.00001;
    public const int DoubleModPow = 2;

    public static int GetTableDirectionFactor(TableDirection tableDirection)
    {
        return tableDirection is TableDirection.In ? -1 : 1;
    }

    public static double GetAccelerateLength(double speed, double acc)
    {
        if(acc == 0)
        {
            throw new InvalidOperationException("Acceration value can't be 0");
        }

        return speed * speed / (2 * acc);
    }

    public static double GetAccelerateTime(double speed, double acc)
    {
        if (acc == 0)
        {
            throw new InvalidOperationException("Acceration value can't be 0");
        }
        return speed / acc;
    }

    public static double GetCycleTime(double frameTime,int framesPerCycle,int expSourceCount)
    {
        return frameTime * framesPerCycle / expSourceCount;
    }

    public static bool IsDoubleMod(double a,double b)
    {

        var v = Math.Abs(a / b) + DoubleTollerance;
        return Math.Abs(v - Math.Round(v)) < Math.Pow(10, -DoubleModPow);            
    }


    public static int[] GetSortedIndex(double[] capacities)
    {
        var indexedData = capacities.Select((value, index) => new { Value = value, Index = index }).ToList();
        return indexedData.OrderBy(x => x.Value).Select(x => x.Index).ToArray();
    }

    public static double GetHelicPreDeleteLength(int collimatorZ, uint objectFov, double preDeleteRatio)
    {
        var zoff = TableCommonConfig.ZOffset - (TableCommonConfig.ZChannelCount - collimatorZ) / 2 * TableCommonConfig.ResolutionZ;
        var k = (zoff - TableCommonConfig.ResolutionZ * collimatorZ / 2.0) / TableCommonConfig.SID;
        var preDeleteLength = ((int)Math.Abs((objectFov / 2) * k * preDeleteRatio / TableCommonConfig.DVoxelZ)) * TableCommonConfig.DVoxelZ;

        return preDeleteLength;
    }

    public static double GetHelicPostDeleteLength(BodyPart bodyPart, int collimatorZ, uint objectFov)
    {
        //if(SystemConfig.ScanDeletionLengthConfig.DeletionLengths is null)
        //{
        //    return 0;
        //}
        //var selectedItem = SystemConfig.ScanDeletionLengthConfig.DeletionLengths.FirstOrDefault<ScanDeletionLengthInfo>(x=>x.BodyPart ==  bodyPart.ToString() && x.CollimatorZ == collimatorZ && x.ObjectFOV == objectFov);
        //return selectedItem is null ? 0 : selectedItem.Length;
        return SystemConfig.GetHelicalDeletionLength(bodyPart.ToString(), objectFov, (uint)collimatorZ).Length;
    }

    /// <summary>
    /// 大锥角
    /// </summary>
    /// <param name="collimatorZ"></param>
    /// <returns></returns>
    public static double GetAxialPostDeleteLength(int collimatorZ)
    {
        //return 12000;//当前只有限束器遮挡排数一个影响因素。算法只给了242时候的值。大锥角12mm，小锥角8mm
        return SystemConfig.GetAxialDeletionLength(FocalType.Large.ToString(), (uint)collimatorZ).Length;
    }

    /// <summary>
    /// 小锥角8mm
    /// </summary>
    /// <param name="collimatorZ"></param>
    /// <returns></returns>
    public static double GetAxialPreDeleteLength(int collimatorZ)
    {
        //return 8000;
        return SystemConfig.GetAxialDeletionLength(FocalType.Small.ToString(), (uint)collimatorZ).Length;
    }

}
