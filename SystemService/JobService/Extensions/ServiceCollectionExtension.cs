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

        // Register JobManagementService as a singleton for the interface and as the hosted service
        services.AddSingleton<IJobManagementService, JobManagementService>();
        services.AddHostedService(provider => provider.GetRequiredService<IJobManagementService>() as JobManagementService);

        services.AddHostedService<AutoFetchWorklistHostService>();

        services.AddSingleton<IOfflineConnection, OfflineConnectionService>();
        services.AddSingleton<IOfflineTaskService, OfflineTaskService>();
        services.AddSingleton<IDicomFileService, DicomFileService>();
        services.AddSingleton<IJobRequestService, JobRequestService>();
        services.AddSingleton<IJobQueueHandler, JobQueueHandler>();

        // Register all job handlers
        services.AddSingleton<IJobHandler, ArchiveJobHandler>();
        services.AddSingleton<IJobHandler, ExportJobHandler>();
        services.AddSingleton<IJobHandler, ImportJobHandler>();
        services.AddSingleton<IJobHandler, PrintJobHandler>();
        services.AddSingleton<IJobHandler, WorkListJobHandler>();

        // Register all job processors as singletons
        services.AddSingleton<ArchiveJobProcessor>();
        services.AddSingleton<ExportJobProcessor>();
        services.AddSingleton<ImportJobProcessor>();
        services.AddSingleton<PrintJobProcessor>();
        services.AddSingleton<WorklistJobProcessor>();

        services.AddSingleton<AutoFetchWorklistHostService>();

        return services;
    }
}
