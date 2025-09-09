//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.InterventionalScan.ViewModel;
using NV.CT.InterventionScan.Layout;
using NV.CT.InterventionScan.View;
using NV.CT.InterventionScan.ViewModel;

namespace NV.CT.InterventionScan.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInterventionScanAppServices(this IServiceCollection services)
    {
        AddLayout(services);
        AddViewModel(services);
        return services;
    }

    private static void AddLayout(IServiceCollection services)
    {
        services.AddSingleton<LayoutControl>();
        services.AddSingleton<CustomWWWLWindow>();
    }

    private static void AddViewModel(IServiceCollection services)
    {
        services.AddSingleton<LayoutViewModel>();
        services.AddSingleton<InterventionScanViewModel>();
        services.AddSingleton<DicomImageViewModel>();
    }
}