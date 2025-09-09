namespace NV.CT.Service.Common.TaskProcessor.Models
{
    public class StatusChangedEventArgs : EventArgs
    {
        public ServiceStatus NewStatus { get; }

        public StatusChangedEventArgs(ServiceStatus newStatus)
        {
            NewStatus = newStatus;
        }
    }
}
