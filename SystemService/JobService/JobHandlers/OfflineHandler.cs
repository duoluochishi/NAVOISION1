using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.JobService.Contract.Model;
using NV.CT.JobService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.JobService.JobHandlers
{
    public class OfflineHandler : ITaskHandler
    {

        private Queue<TaskInfo> _queue;

        public bool CanAccepted(TaskType taskType)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Start(string taskId)
        {
            //var executor = new OfflineExecutor();
            //executor.StatusChanged += TaskStatusChanged;
            //try
            //{
            //    executor.Execute((OfflineTaskInfo)taskInfo.Parameters);
            //}
            //catch
            //{
            //    _Busy = false;
            //}
        }

        public void Stop(string taskId)
        {
            throw new NotImplementedException();
        }

        bool ITaskHandler.CanAccepted(TaskType taskType)
        {
            throw new NotImplementedException();
        }

        void ITaskHandler.Cancel(string taskId)
        {
            throw new NotImplementedException();
        }

        void ITaskHandler.Delete(string taskId)
        {
            throw new NotImplementedException();
        }

        void ITaskHandler.Enqueue(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        void ITaskHandler.Start(string taskId)
        {
            throw new NotImplementedException();
        }

        void ITaskHandler.Stop(string taskId)
        {
            throw new NotImplementedException();
        }
    }
}
