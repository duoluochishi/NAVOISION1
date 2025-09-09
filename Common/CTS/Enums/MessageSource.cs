//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/11/6 16:35:59    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Enums;
/// <summary>
/// MessageSource
/// </summary>
public enum MessageSource
{
    Unknown = 0,
    JobViewer,
    ImageViewer,
    InterventionScan,
    Print,
    PatientManager,
    Examination,
    PatientBrowser,
    ProtocolManager,
    Recon,
    RGT,
    ServiceFrame,
    ImportJob,
    ExportJob,
    ArchiveJob,
    PrintJob,
    WorklistJob,    
    System,
}