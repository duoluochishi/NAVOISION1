using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.CT.DicomUtility.Extensions;
using NV.CT.JobService.Contract;
using NV.CT.JobService.Interfaces;
using NV.CT.JobService.JobHandlers;
using NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;
using NV.MPS.Communication;

namespace NV.CT.JobService.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddJobServices(this IServiceCollection services)
    {
        services.DicomUtilityConfigInitialization();
        services.DicomUtilityConfigInitializationForWin();
        services.AddAutoMapper(typeof(ToProfile));
        services.AddMRSMapper();

        services.AddCommunicationServerServices();
        services.AddCommunicationClientServices();

        services.AddHostedService<OfflineTaskHandler>();
        services.AddHostedService<DicomFileHandler>();
        services.AddHostedService<PrintJobProcessor>();
        services.AddHostedService<ArchiveJobProcessor>();
        services.AddHostedService<ImportJobProcessor>();
        services.AddHostedService<ExportJobProcessor>();
        services.AddHostedService<WorklistJobProcessor>();
        services.AddHostedService<AutoFetchWorklistHostService>();

        services.AddSingleton<IOfflineConnection, OfflineConnectionService>();
        services.AddSingleton<IOfflineTaskService, OfflineTaskService>();
        services.AddSingleton<IDicomFileService, DicomFileService>();
        services.AddSingleton<IJobRequestService, JobRequestService>();
        services.AddSingleton<IJobManagementService, JobManagementService>();
        services.AddSingleton<IJobQueueHandler, JobQueueHandler>();


        services.AddSingleton<ArchiveJobHandler>();
        services.AddSingleton<ExportJobHandler>();
        services.AddSingleton<ImportJobHandler>();
        services.AddSingleton<PrintJobHandler>();
        services.AddSingleton<WorkListJobHandler>();

        services.AddSingleton<ArchiveJobProcessor>();
        services.AddSingleton<ExportJobProcessor>();
        services.AddSingleton<ImportJobProcessor>();
        services.AddSingleton<PrintJobProcessor>();
        services.AddSingleton<WorklistJobProcessor>();

        services.AddSingleton<AutoFetchWorklistHostService>();

        return services;
    }
}
