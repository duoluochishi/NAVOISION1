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
using NV.CT.AppService.Contract;
using NV.CT.ClientProxy.Application;
using NV.CT.ClientProxy.ConfigService;
using NV.CT.ClientProxy.DataService;
using NV.CT.ClientProxy.Job;
using NV.CT.ClientProxy.JobService;
using NV.CT.ClientProxy.SyncService;
using NV.CT.ClientProxy.Workflow;
using NV.MPS.Communication;
using NV.CT.ConfigService.Contract;
using NV.CT.DatabaseService.Contract;
using NV.CT.JobService.Contract;
using NV.CT.MessageService.Contract;
using NV.CT.ProtocolService.Contract;
using NV.CT.SyncService.Contract;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.CT.SystemInterface.MCSRuntime.Impl;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ClientProxy;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddCommunicationClientServices(this IServiceCollection services)
	{
		services.AddSingleton<BusClient>();

        services.AddSingleton<MCSServiceClientProxy>();

        //Job
        services.AddSingleton<JobClientProxy>();

		//Application
		services.AddSingleton<IApplicationCommunicationService, ApplicationCommunicationService>();
		services.AddSingleton<IScreenManagement, ScreenManagementService>();
		services.AddSingleton<AppService.Contract.IShutdownService, Application.ShutdownService>();
		services.AddSingleton<AppService.Contract.ISelfCheckService, Application.SelfCheckService>();

		//Workflow
		services.AddSingleton<IWorkflow, Workflow.Workflow>();
		services.AddSingleton<IReconService, Recon>();
		services.AddSingleton<IInputListener, InputListener>();
		services.AddSingleton<IViewer, Workflow.Viewer>();
		services.AddSingleton<IProtocolOperation, ProtocolOperationService>();
		services.AddSingleton<IPatientService, Patient>();
		services.AddSingleton<IStudyService, Study>();
		services.AddSingleton<IScanTaskService, ScanTask>();
		services.AddSingleton<IReconTaskService, ReconTask>();
		services.AddSingleton<ISeriesService, Series>();
		services.AddSingleton<IJobTaskService, JobTask>();
		services.AddSingleton<IAuthorization, Authorization>();
		services.AddSingleton<IPrintConfigManager, PrintConfigManager>();

		//Job
		services.AddSingleton<IOfflineConnection, OfflineConnection>();
		services.AddSingleton<IOfflineTaskService, OfflineTaskService>();
		services.AddSingleton<IDicomFileService, DicomFileService>();
		services.AddSingleton<IJobRequestService, JobRequestService>();
		
		services.AddSingleton<IMessageService, NV.CT.ClientProxy.MessageService.MessageService>();
		services.AddSingleton<IPrint, Workflow.PrintService>();
		services.AddSingleton<IRawDataService, RawData>();

		//DB
		services.AddSingleton<IRoleService, RoleClientProxy>();
		services.AddSingleton<IUserService, UserClientProxy>();
		services.AddSingleton<ILoginHistoryService, LoginHistoryService>();
		services.AddSingleton<IPermissionService, PermissionClientProxy>();

		services.AddSingleton<ISpecialDiskService, SpecialDiskService>();
		services.AddSingleton<IDeviceService, DeviceService>();
		services.AddSingleton<ICDROMService, CDROMService>();
		services.AddSingleton<ILogicalDiskService, LogicalDiskService>();
		services.AddSingleton<IUSBService, USBService>();
		services.AddSingleton<ISpecialDiskService, SpecialDiskService>();

		services.AddSingleton<IImageAnnotationService, ImageAnnotationService>();
		services.AddSingleton<IPatientConfigService, PatientConfigService>();
		services.AddSingleton<IStudyListColumnsConfigService, StudyListColumnsConfigService>();
		services.AddSingleton<IFilterConfigService, FilterConfigService>();

		//services.AddSingleton<IHardwareService, HardwareService>();
		services.AddSingleton<IDoseCheckService, DoseCheck>();
		services.AddSingleton<IVoiceService, VoiceService>();
		//配置相关
		services.AddSingleton<IErrorCodeService, ErrorCodeService>();
		services.AddSingleton<IPrintProtocolConfigService, PrintProtocolConfigService>();
		services.AddSingleton<IFilmSettingsConfigService, FilmSettingsConfigService>();
		//UISync
		services.AddSingleton<SyncServiceClientProxy>();
		services.AddSingleton<IScreenSync, ScreenSyncService>();
		services.AddSingleton<IDataSync, DataSyncService>();
		services.AddSingleton<PrintConfigClientProxy>();
		services.AddSingleton<IPrintConfigService, PrintConfigService>();
		return services;
	}
}