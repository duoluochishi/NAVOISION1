using Microsoft.Extensions.Logging;
using NV.CT.JobService.Contract.Model;
using NV.CT.JobViewer.ApplicationService.Contract;

namespace NV.CT.JobViewer.ApplicationService.Impl
{
    public class DicomFileExportTaskService : IDicomFileExportTaskService
    {
        public event EventHandler<List<ExportTaskInfo>> BeginExport;
        public event EventHandler<ExportTaskInfo> ExportProgress;
        public event EventHandler<object> ExportCompleted;
        public event EventHandler<ExportTaskInfo> ExportError;
        private ILogger<DicomFileExportTaskService> _logger;
        public DicomFileExportTaskService(ILogger<DicomFileExportTaskService> logger)
        {
            _logger = logger;
        }

    }
}
