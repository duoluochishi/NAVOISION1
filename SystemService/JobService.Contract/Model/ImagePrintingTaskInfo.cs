//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.JobService.Contract.Model;

public class ImagePrintTaskInfo
{
    public string WorkflowId { get; set; } = string.Empty;
    public string StudyId { get; set; } = string.Empty;
    public string SeriesId { get; set; } = string.Empty;
    public string ImageId { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;
    public int NumberOfCopies { get; set; } = 1;
    public string PageSize { get; set; } = string.Empty;

    //starts from 1 to show
    public int ImageDisplayNumber { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string AECaller { get; set; } = string.Empty;
    public string AETitle { get; set; } = string.Empty;
    public int Progress {  get; set; }
    public int TotalCount { get; set; }
    public bool IsPartlyCompleted { get; set; } = false;
    public DateTime TaskTime { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

}
