//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.JobService.Contract;
using NV.CT.JobService.Contract.Model;
namespace NV.CT.ClientProxy.Job;

public class DicomFileService : IDicomFileService
{
    private readonly JobClientProxy _clientProxy;

    public DicomFileService(JobClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public LoadImageInstanceCommandResult LoadImageInstances(string pathName)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDicomFileService).Namespace,
            SourceType = nameof(IDicomFileService),
            ActionName = nameof(IDicomFileService.LoadImageInstances),
            Data = pathName
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<LoadImageInstanceCommandResult>();
            return res;
        }

        return default;
    }


    public JobTaskCommandResult UpdateDICOM(UpdateDicomRequest request)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDicomFileService).Namespace,
            SourceType = nameof(IDicomFileService),
            ActionName = nameof(IDicomFileService.UpdateDICOM),
            Data = request.ToJson(),
        }) ;
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<JobTaskCommandResult>();
            return res;
        }

        return default;

    }

#pragma warning disable 67
    public event EventHandler<ImportTaskInfo>? AnalyzeCompleted;
    public event EventHandler<ImportTaskInfo>? BeginImport;
    public event EventHandler<ImportTaskInfo>? CancelImport;
    public event EventHandler<ImportTaskInfo>? ImportProgress;
    public event EventHandler<object>? ImportCompleted;
    public event EventHandler<ImportTaskInfo>? ImportError;
    public event EventHandler<List<ExportTaskInfo>>? BeginExport;
    public event EventHandler<ExportTaskInfo>? ExportProgress;
    public event EventHandler<ExportTaskInfo>? ExportCompleted;
    public event EventHandler<ExportTaskInfo>? ExportError;
    public event EventHandler<List<ImageInstanceModel>>? LoadImageInstancesCompleted;
#pragma warning restore 67

}