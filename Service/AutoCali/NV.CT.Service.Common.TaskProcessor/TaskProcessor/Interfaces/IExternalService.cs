using NV.CT.Service.Common.TaskProcessor.Models;
using ProgressChangedEventArgs = NV.CT.Service.Common.TaskProcessor.Models.ProgressChangedEventArgs;

namespace NV.CT.Service.Common.TaskProcessor.Interfaces
{
    public interface IExternalService
    {
        string TaskId { get; set; }
        bool RequestStart();
        bool RequestStop();
        event EventHandler<StatusChangedEventArgs> StatusChanged;
        event EventHandler<ProgressChangedEventArgs> ProgressChanged;
    }
}
