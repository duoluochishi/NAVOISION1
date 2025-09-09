namespace NV.CT.Service.Common.TaskProcessor.Models
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public double Progress { get; }

        public ProgressChangedEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}
