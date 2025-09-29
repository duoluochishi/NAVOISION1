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
using NV.CT.CTS.Models;
using NV.CT.JobService.Contract.Model;

namespace NV.CT.JobService.Contract
{
    public interface ITaskService
    {
        void Enqueue(TaskInfo taskInfo);
        void Start(string taskId);
        void Stop(string taskId);
        void Cancel(string taskId);
        void Delete(string taskId);
    }
}
