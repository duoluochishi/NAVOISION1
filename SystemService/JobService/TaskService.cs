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

using Microsoft.Extensions.Logging;
using NV.CT.CTS.Models;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.JobService.Contract.Model;
using NV.CT.JobService.Interfaces;
using System.Data.Common;

namespace NV.CT.JobService
{
    public class TaskService : ITaskService
    {
        private IList<ITaskHandler> _handlers;

        private readonly ILogger<JobManagementService> _logger;

        public TaskService(ILogger<JobManagementService> logger)
        {
            _logger = logger;
            _handlers = new List<ITaskHandler>();
        }


        public void Cancel(string taskId)
        {
            throw new NotImplementedException();
        }

        public void Delete(string taskId)
        {
            throw new NotImplementedException();
        }


        public void Enqueue(TaskInfo taskInfo)
        {

            try
            {
                _logger.LogTrace($"JobManagementService EnqueueJob for jobId:{taskInfo.Id} with jobType:{taskInfo.Type.ToString()}");
                foreach (var handler in _handlers)
                {
                    if (handler.CanAccepted(taskInfo.Type))
                    {
                        handler.Enqueue(taskInfo); break;
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogWarning($"TaskSerive failed to Enqueue for jobId:{taskInfo.Id} with exception:{ex.Message}");
            }

        }

        public void Start(string taskId)
        {
            throw new NotImplementedException();
        }

        public void Stop(string taskId)
        {
            throw new NotImplementedException();
        }
    }
}
