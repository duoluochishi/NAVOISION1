using Microsoft.Extensions.Logging;
using NV.CT.JobViewer.ApplicationService.Contract;

namespace NV.CT.JobViewer.ApplicationService.Impl;

public class DicomFileImportTaskService : IDicomFileImportTaskService
{
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo>? AnalyzeCompleted;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo>? BeginImport;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo>? CancelImport;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo>? ImportProgress;
    public event EventHandler<object>? ImportCompleted;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo>? ImportError;

    private ILogger<DicomFileImportTaskService>? _logger;
    public DicomFileImportTaskService(ILogger<DicomFileImportTaskService> logger)
    {
        _logger = logger;
    }
   
}
