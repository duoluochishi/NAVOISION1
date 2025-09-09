//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.RGT.ApplicationService.Contract.Models;

public class ScanTaskModel
{
    /// <summary>
    /// 扫描任务ID
    /// </summary>
    public string TaskID { get; set; } = string.Empty;

    /// <summary>
    /// 扫描任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 检查ID
    /// </summary>
    public string StudyID { get; set; } = string.Empty;

    /// <summary>
    /// 扫描协议ID
    /// </summary>
    public string ProtocolID { get; set; } = string.Empty;

    /// <summary>
    /// 扫描类型，定位片、轴扫、螺旋
    /// </summary>
    public ScanOption ScanOption { get; set; } = ScanOption.Axial;

    /// <summary>
    /// 图形类型
    /// </summary>
    public ScanImageType ScanImageType
    {
        get
        {
            if (ScanOption == ScanOption.Axial
                || ScanOption == ScanOption.Helical)
            {
                return ScanImageType.Tomo;
            }
            else
            {
                return ScanImageType.Topo;
            }
        }
    }

    /// <summary>
    /// 扫描体位
    /// </summary>
    public PatientPosition PatientPosition { get; set; } = PatientPosition.FFS;

    /// <summary>
    /// 扫描任务状态
    /// </summary>
    public PerformStatus ScanTaskStatus { get; set; } = PerformStatus.Unperform;

    /// <summary>
    /// 是否增强
    /// </summary>
    public bool IsEnhance { get; set; } = false;

    /// <summary>
    /// 是否紧急
    /// </summary>
    public bool IsEmergency { get; set; } = false;

    /// <summary>
    /// 是否连扫的第一个，默认是第一个。
    /// </summary>
    public bool IsFirst { get; set; } = true;

    /// <summary>
    /// 是否连扫的最后一个，默认是最后一个
    /// </summary>
    public bool IsLast { get; set; } = true;

    /// <summary>
    /// 扫描协议的实时重建任务列表
    /// </summary>
    public List<ReconTaskModel> ReconTaskModels { get; set; } = new();
}