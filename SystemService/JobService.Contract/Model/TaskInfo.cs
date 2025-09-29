//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/9/29 09:21:20    V2.0.4       陈鑫
// </summary>
//-----------------------------------------------------------------------


using NV.CT.CTS.Enums;
using TaskStatus = NV.CT.CTS.Enums.TaskStatus;

namespace NV.CT.JobService.Contract.Model
{
    public class TaskInfo
    {
        public string Id { get; set; }
        public TaskType Type { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        //具体任务类实例
        public object Parameters { get; set; }
    }
}
