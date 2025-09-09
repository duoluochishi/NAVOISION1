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
using Microsoft.Extensions.DependencyInjection;

using NV.CT.DatabaseService.Contract;

namespace NV.CT.DatabaseService.Impl;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
	{
		services.AddSingleton<IReconTaskService, ReconTaskService>();
		services.AddSingleton<IScanTaskService, ScanTaskService>();
		services.AddSingleton<ISeriesService, SeriesService>();
		services.AddSingleton<IStudyService, StudyService>();
		services.AddSingleton<IJobTaskService, JobTaskService>();
		services.AddSingleton<IPatientService, PatientService>();
		services.AddSingleton<IDoseCheckService, DoseCheckService>();
		services.AddSingleton<IVoiceService, VoiceService>();
		services.AddSingleton<IRoleService, RoleService>();
		services.AddSingleton<IUserService, UserService>();
		services.AddSingleton<ILoginHistoryService, LoginHistoryService>();
		services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IRawDataService, RawDataService>();
        services.AddSingleton<IPrintConfigManager, PrintConfigManager>();
        return services;
	}
}