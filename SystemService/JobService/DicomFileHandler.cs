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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NV.CT.JobService.Contract;

namespace NV.CT.JobService;

public class DicomFileHandler : IHostedService
{
    private readonly ILogger<DicomFileHandler> _logger;

    private readonly IDicomFileService _dicomFileService;

    //IImportEvents importEvents, EventSubscriber eventSubscriber,
    public DicomFileHandler(ILogger<DicomFileHandler> logger, IDicomFileService dicomFileService)
    {
        _logger = logger;
        _dicomFileService = dicomFileService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_dicomFileService is not null)
        {
            //_dicomFileService.AnalyzeCompleted += _dicomFileService_AnalyzeCompleted;
            //_dicomFileService.ImportCompleted += _dicomFileService_ImportCompleted;
            //_dicomFileService.ImportError += _dicomFileService_ImportError;
            //_dicomFileService.ImportProgress += _dicomFileService_ImportProgress; ;
        }

        return Task.CompletedTask;
    }

    private void _dicomFileService_ImportProgress(object? sender, Contract.Model.ImportTaskInfo e)
    {
        throw new NotImplementedException();
    }

    private void _dicomFileService_ImportError(object? sender, Contract.Model.ImportTaskInfo e)
    {
        throw new NotImplementedException();
    }

    private void _dicomFileService_ImportCompleted(object? sender, object e)
    {
        throw new NotImplementedException();
    }

    private void _dicomFileService_AnalyzeCompleted(object? sender, Contract.Model.ImportTaskInfo e)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_dicomFileService is not null)
        {
            //_dicomFileService.AnalyzeCompleted -= _dicomFileService_AnalyzeCompleted;
            //_dicomFileService.ImportCompleted -= _dicomFileService_ImportCompleted;
            //_dicomFileService.ImportError -= _dicomFileService_ImportError;
            //_dicomFileService.ImportProgress -= _dicomFileService_ImportProgress;
        }
        return Task.CompletedTask;
    }

    //private void _importEvents_ImportProgress(object? sender, string e)
    //{
    //    //_dicomFileService.RaiseImportProgressEvent(sender?.ToString(), e);
    //}

    //private void _importEvents_ImportError(object? sender, string e)
    //{
    //    //_dicomFileService.RaiseImportErrorEvent(sender?.ToString(), e);

    //}

    //private void _importEvents_ImportCompleted(object? sender, string e)
    //{

    //}

    //private void _importEvents_AnalyzeCompleted(object? sender, string e)
    //{
    //    //_dicomFileService.RaiseAnalyzeCompletedEvent(sender?.ToString(), e);
    //}
}