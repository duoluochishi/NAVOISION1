using NV.CT.JobService.Contract.Model;

namespace NV.CT.JobViewer.ApplicationService.Contract;

public interface IDicomFileImportTaskService
{
    //void  GetPatientInfos(string data);
    // void GetProgress(string data);
    //void GetError(string data);
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo> AnalyzeCompleted;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo> BeginImport;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo> CancelImport;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo> ImportProgress;
    public event EventHandler<object> ImportCompleted;
    public event EventHandler<JobService.Contract.Model.ImportTaskInfo> ImportError;
   
}
