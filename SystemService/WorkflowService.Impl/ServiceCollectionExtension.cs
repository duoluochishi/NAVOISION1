using Microsoft.Extensions.DependencyInjection;
using NV.CT.DicomUtility.Extensions;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.WorkflowService.Impl;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        services.DicomUtilityConfigInitialization();
        services.DicomUtilityConfigInitializationForWin();

        services.AddSingleton<DoseReport>();
        services.AddSingleton<BlackImageReport>();
        services.AddSingleton<ExamService>();
        services.AddSingleton<ReportService>();
        services.AddSingleton<IAuthorization, AuthorizationService>();
        services.AddSingleton<IReconService,ReconService>();
        services.AddSingleton<IWorkflow, WorkflowService>();
        services.AddSingleton<IViewer, ViewerService>();
        services.AddSingleton<IPrint, PrintService>();
        services.AddSingleton<IInputListener, InputListener>();

		return services;
    }
}
