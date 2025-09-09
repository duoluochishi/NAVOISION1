//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Recon.Extensions;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddReconAppServices(this IServiceCollection services)
	{

		AddLayout(services);

		AddViewModel(services);

		return services;
	}

	private static void AddLayout(IServiceCollection services)
	{
		services.AddSingleton<ReconControl>();
		services.AddSingleton<ScanDefaultControl>();

		//var layoutNamespace = $"{nameof(NV)}.{nameof(CT)}.{nameof(Examination)}.{nameof(App)}.{nameof(View)}.{nameof(View.Layout)}";
		//builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
		//    .PublicOnly()
		//    .Where(t => t.FullName != null &&
		//                t.FullName.StartsWith(layoutNamespace) && t.BaseType == typeof(UserControl)).SingleInstance();
	}

	private static void AddViewModel(IServiceCollection services)
	{
		services.AddSingleton<ScanMainViewModel>();
		services.AddSingleton<ScanDefaultViewModel>();
		services.AddSingleton<ReconTaskListViewModel>();
		services.AddSingleton<ReconScanControlsViewModel>();
		services.AddSingleton<ReconControlViewModel>();
		services.AddSingleton<PlanningBaseViewModel>();
		services.AddSingleton<ReconRangeViewModel>();
	}
}