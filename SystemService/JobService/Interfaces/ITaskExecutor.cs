using NV.CT.JobService.Contract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.JobService.Interfaces
{
    public interface ITaskExecutor
    {
        void Execute(TaskInfo taskInfo);

        event EventHandler<TaskInfo> StatusChanged;

    }
}
