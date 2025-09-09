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

public class ImportTaskInfo
{
    public string WorkflowId { get; set; }
    public string TaskName { get; set; }
    public string TaskTime { get; set; }
    //病人解析进度
    public float Progress { get; set; }
    public string ErrorMessage { get; set; }

}
