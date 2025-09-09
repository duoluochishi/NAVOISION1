//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/17 15:42:21     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.CTS.Models;

//
// 摘要:
//     真实剂量信息
public class RealDoseInfo
{
    /// <summary>
    /// EV (113769, DCM, "Irradiation Event UID")
    /// 描述：扫描UID
    /// 来源：采集重建参数下发
    /// </summary>
    public string ScanUID { get; set; } = string.Empty;

    /// <summary>
    /// EV (113824, DCM, "Exposure Time")
    /// 描述：曝光总时间
    /// 来源：每张采集图像所用曝光时间之和，采集卡提供每张图像曝光时间
    /// 单位：ms
    /// </summary>
    public float TotalExposureTime { get; set; }

    /// <summary>
    /// DTID 10014 “Scanning Length”
    /// 描述：扫描长度
    /// 来源：根据采集卡图像携带的床码信息计算
    /// 单位：mm
    /// </summary>
    public float ScanLength { get; set; }

    /// <summary>
    /// EV (113826, DCM, "Nominal Single Collimation Width")
    /// 描述：单准直器宽度
    /// 单位：mm
    /// </summary>
    public float SingleCollimationWidth { get; set; }

    /// <summary>
    /// 总准直器宽度
    /// 单位：mm
    /// </summary>
    public float TotalCollimationWidth { get; set; }

    /// <summary>
    ///EV(113823, DCM, "Number of X-Ray Sources")
    ///描述：射线源数量
    ///来源：根据CT型号确定
    ///单位：个
    /// </summary>
    public int SourceCount { get; set; }

    /// <summary>
    ///EV(113832, DCM, "Identification of the X-Ray Source")
    ///描述：射线源标识
    ///来源：统计采集图像的射线源号
    /// </summary>
    public int[]? SourceNum { get; set; }

    //以下不是定位扫时是必须的，定位扫时不是必须的。
    /// <summary>
    ///EV(113830, DCM, "Mean CTDIvol")
    ///描述：容积CT剂量指数,单位 mGy
    ///来源：算法提供CTDI公式
    /// </summary>
    public float CTDIvol { get; set; }

    /// <summary>
    /// 电流毫安积(mAs)
    /// </summary>
    public float mAs { get; set; }

    /// <summary>
    /// EV(113835, DCM, "Phantom Type")
    /// CT剂量模体类型
    /// </summary>
    public PhantomType PhantomType { get; set; }

    /// <summary>
    ///EV(113838, DCM, "DLP")
    ///描述：DLP,单位 mGycm
    ///来源：CTDI* 扫描长度
    /// </summary>
    public float DLP { get; set; }

    public float ExposureLength { get; set; }

    /// <summary>
    ///描述：球管剂量
    /// </summary>
    public List<TubeDoseInfo>? TubeDoses { get; set; }
}
