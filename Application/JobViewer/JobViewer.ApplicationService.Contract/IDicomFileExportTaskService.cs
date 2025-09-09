using NV.CT.JobService.Contract.Model;

namespace NV.CT.JobViewer.ApplicationService.Contract;

public interface IDicomFileExportTaskService
{
    event EventHandler<List<ExportTaskInfo>> BeginExport;
    event EventHandler<ExportTaskInfo> ExportProgress;
    event EventHandler<object> ExportCompleted;
    event EventHandler<ExportTaskInfo> ExportError;
}
