namespace NV.CT.RGT.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRgtAppServices(this IServiceCollection services)
    {

        //AddLayout(services);

        //AddViewModel(services);

        return services;
    }

    private static void AddLayout(IServiceCollection services)
    {
        //services.AddSingleton<ScanDefaultControl>();

        //var layoutNamespace = $"{nameof(NV)}.{nameof(CT)}.{nameof(RGT)}.{nameof(App)}.{nameof(Layout)}";
        //.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        //    .PublicOnly()
        //    .Where(t => t.FullName != null &&
        //                t.FullName.StartsWith(layoutNamespace) && t.BaseType == typeof(UserControl)).SingleInstance();
    }

    private static void AddViewModel(IServiceCollection services)
    {
        //services.AddSingleton<ScanMainViewModel>();
        //services.AddSingleton<ScanDefaultViewModel>();
        //services.AddSingleton<TaskListViewModel>();
        //services.AddSingleton<ScanControlsViewModel>();
    }
}