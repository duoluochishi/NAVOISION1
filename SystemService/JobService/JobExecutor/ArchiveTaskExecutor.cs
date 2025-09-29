using NV.CT.JobService.Contract.Model;
using NV.CT.JobService.Interfaces;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.JobService.JobExecutor
{
    public class ArchiveTaskExecutor : ITaskExecutor
    {
        public event EventHandler<TaskInfo> StatusChanged;

        public void Execute(TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }
    }
}
