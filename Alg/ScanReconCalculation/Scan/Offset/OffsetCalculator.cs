//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/12 16:41:24    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Offset;

public static class OffsetCalculator
{
    public static int SurviewPreOffsetNum = 15;
    public static int StaggeredScanPreOffsetNum = 26;
    public static int DefaultPreOffsetNum = 15;

    public static int PostOffsetDuration = 200000;

    /// <summary>
    /// 获取PreOffset
    /// 定位像为15
    /// 错峰扫描100
    /// 其余扫描方式100
    /// </summary>
    /// <param name="scanOption"></param>
    /// <param name="scanMode"></param>
    /// <returns></returns>
    public static int GetPreOffset(ScanOption scanOption,ScanMode scanMode)
    {
        if(scanOption is ScanOption.Surview or ScanOption.DualScout)
        {
            return SurviewPreOffsetNum;
        }
        //if (scanOption is ScanOption.Axial)
        //{
        //    //todo: 临时，调试（2025/06/04），此处代码不能提交
        //    return 200;
        //}
        if(scanMode is ScanMode.StaggeredScan)
        {
            return StaggeredScanPreOffsetNum;
        }
        if (scanOption is ScanOption.NVTestBolus)
        {
            return 0;
        }

        return DefaultPreOffsetNum;
    }

    /// <summary>
    /// 获取PostOffset
    /// 定位像为0
    /// 其余扫描方式使用200ms/FrameTime
    /// </summary>
    /// <param name="scanOption"></param>
    /// <param name="frameTime">默认值单位为us。建议使用前加载系统配置PostOffsetDuration，并使用相同的单位进行调用。</param>
    /// <returns></returns>
    public static int GetPostOffset(ScanOption scanOption, int frameTime)
    {
        if(scanOption is ScanOption.Surview or ScanOption.DualScout)
        {
            return 0;
        }
        if (scanOption is ScanOption.NVTestBolus)
        {
            return 24;
        }

        return PostOffsetDuration / frameTime;
    }

}
