//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using NV.CT.Job.ClientProxy;
//using NV.CT.Job.ClientProxy.GRPCClients;
//using NV.CT.Job.ClientProxy.Services;
//using NV.CT.Job.Common;
//using NV.CT.JobViewer.ApplicationService.Contract;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace NV.CT.JobViewer.ApplicationService.Impl
//{
//    public class DicomFileImportTaskHandler : IHostedService
//    {
//        private readonly ILogger<DicomFileImportTaskHandler> _logger;
//        private readonly IImportEvents  _importEvents;
//        private readonly EventSubscriber _eventSubscriber;
//        private readonly IDicomFileImportTaskService  _dicomFileImportTaskService;
//        public DicomFileImportTaskHandler(ILogger<DicomFileImportTaskHandler> logger, IImportEvents importEvents, EventSubscriber eventSubscriber, IDicomFileImportTaskService dicomFileImportTaskService)
//        {
//            _logger = logger;
//            _importEvents = importEvents;
//            _eventSubscriber = eventSubscriber;
//            _dicomFileImportTaskService = dicomFileImportTaskService;
//        }

//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            if (_importEvents is not null)
//            {
//                _importEvents.AnalyzeCompleted += _importEvents_AnalyzeCompleted;
//                _importEvents.ImportCompleted += _importEvents_ImportCompleted;
//                _importEvents.ImportError += _importEvents_ImportError;
//                _importEvents.ImportProgress += _importEvents_ImportProgress;
//            }
//            if (_eventSubscriber is not null)
//            {
//                Task.Run(async () =>
//                {
//                    await _eventSubscriber.Subscribe();
//                });
//            }
//            return Task.CompletedTask;
//        }

//        private void _importEvents_ImportProgress(object? sender, string e)
//        {
//            _dicomFileImportTaskService.RaiseImportProgressEvent(sender?.ToString(), e);
//        }

//        public Task StopAsync(CancellationToken cancellationToken)
//        {
//            if (_importEvents is not null)
//            {
//                _importEvents.AnalyzeCompleted -= _importEvents_AnalyzeCompleted;
//                _importEvents.ImportCompleted -= _importEvents_ImportCompleted;
//                _importEvents.ImportError -= _importEvents_ImportError;
//                _importEvents.ImportProgress -= _importEvents_ImportProgress;
//            }
//            if (_eventSubscriber is not null)
//            {
//                _eventSubscriber.Unsubscribe();
//            }
//            return Task.CompletedTask;
//        }
//        private void _importEvents_ImportError(object? sender, string e)
//        {
//            _dicomFileImportTaskService.RaiseImportErrorEvent(sender?.ToString(), e);

//        }

//        private void _importEvents_ImportCompleted(object? sender, string e)
//        {

//        }

//        private void _importEvents_AnalyzeCompleted(object? sender, string e)
//        {
//            _dicomFileImportTaskService.RaiseAnalyzeCompletedEvent(sender?.ToString(),e);
//        }


//    }
//}
