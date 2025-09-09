using Autofac;
using Microsoft.Extensions.Hosting;
using NV.CT.JobViewer.ApplicationService.Contract;

namespace NV.CT.JobViewer.ApplicationService.Impl
{
    public class ApplicationServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OfflineReconTaskService>().As<IOfflineReconTaskService>().SingleInstance();
            builder.RegisterType<DicomFileImportTaskService>().As<IDicomFileImportTaskService>().SingleInstance();
            builder.RegisterType<DicomFileExportTaskService>().As<IDicomFileExportTaskService>().SingleInstance();
        }
    }
}
