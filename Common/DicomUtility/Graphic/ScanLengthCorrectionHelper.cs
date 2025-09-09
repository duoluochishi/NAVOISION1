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

namespace NV.CT.DicomUtility.Graphic;
/// <summary>
/// 
/// </summary>
public static class ScanLengthCorrectionHelper
{
    public static int GetCorrectedSurviewScanLength(int length, int SurviewActuralSize = 20000, int minLength = 0, int maxLength = 2000000)
    {
        var validLength = GetScanLengthInRange(length, minLength, maxLength);

        return validLength;

        /*
        var correctedLength = validLength / SurviewActuralSize * SurviewActuralSize;     
        
        correctedLength = correctedLength== validLength ? correctedLength:correctedLength + SurviewActuralSize;

        correctedLength = correctedLength <= maxLength ? correctedLength:correctedLength - SurviewActuralSize ;

        return correctedLength;
        */
    }

    public static int GetCorrectedAxialScanLength(int length, int tableFeed = 20000, int collimatedSW = 42240,int preDeleteLength = 8000,int postDeleteLength=12000, int minLength = 0, int maxLength = 1500000)
    {
        var validLength = GetScanLengthInRange(length, minLength, maxLength);

        //考虑轴扫的前后删图,等效SliceWidth需要减前后删图
        var effectSW = collimatedSW - preDeleteLength - postDeleteLength;

        if (validLength < effectSW)
        {
            validLength = effectSW;
        }

        var correctedLength = (length - effectSW) / tableFeed * tableFeed + effectSW;

        correctedLength = correctedLength == validLength ? correctedLength : correctedLength + tableFeed;

        correctedLength = correctedLength <= maxLength ? correctedLength : correctedLength - tableFeed;

        return correctedLength;
    }

    /// <summary>
    /// 螺旋长度不校正，仅考虑最大最小长度
    /// </summary>
    /// <param name="length"></param>
    /// <param name="sliceThickness"></param>
    /// <returns></returns>
    public static int GetCorrectedHelicalScanLength(int length, double sliceThickness, int minLength = 0, int maxLength = 1500000)
    {
        var result = length > minLength?length: minLength;
        result = result < maxLength ? result : maxLength;  
        return result;
    }

    public static TableDirection GetTableDirection(PatientPosition pp, ImageOrders imageOrder)
    {
        switch (pp)
        {
            case PatientPosition.HFS:
            case PatientPosition.HFP:
            case PatientPosition.HFDL:
            case PatientPosition.HFDR:
                if (imageOrder is ImageOrders.HeadFoot)
                {
                    return TableDirection.In;
                }
                return TableDirection.Out;
            default:
                if (imageOrder is ImageOrders.HeadFoot)
                {
                    return TableDirection.Out;
                }
                return TableDirection.In;

        }
    }

    private static int GetScanLengthInRange(int length, int min, int max)
    {
        var lengthInRange = length;
        lengthInRange = lengthInRange >= min ? lengthInRange : min;
        lengthInRange = lengthInRange <= max ? lengthInRange : max;

        return lengthInRange;
    }

    //临时先这直接返回0
    public static int GetCorrectedNVTestBolusBaseScanLength(int length)
    {
        return 0;
    }

    //临时先直接返回0
    public static int GetCorrectedNVTestBolusScanLength(int length)
    {
        return 0;
    }
}