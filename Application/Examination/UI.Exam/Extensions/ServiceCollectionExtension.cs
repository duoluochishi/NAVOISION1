using NV.CT.DicomUtility.Extensions;
using NV.CT.UI.Exam.DynamicParameters.Impl;
using NV.CT.UI.Exam.DynamicParameters.Interfaces;

namespace NV.CT.UI.Exam.Extensions;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddUIExamServices(this IServiceCollection services)
	{

		#region 组件注册区域
		services.AddSingleton<DynamicParameters.Templates.Default.ScanParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.Default.ScanParameterDetailControl>();
		services.AddSingleton<DynamicParameters.Templates.Default.ReconParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.Default.ReconParameterDetailControl>();

		services.AddSingleton<DynamicParameters.Templates.AxialDefault.ScanParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.AxialDefault.ScanParameterDetailControl>();
		services.AddSingleton<DynamicParameters.Templates.AxialDefault.ReconParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.AxialDefault.ReconParameterDetailControl>();

		services.AddSingleton<DynamicParameters.Templates.InterventionDefault.ScanParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.InterventionDefault.ScanParameterDetailControl>();
		services.AddSingleton<DynamicParameters.Templates.InterventionDefault.ReconParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.InterventionDefault.ReconParameterDetailControl>();

		services.AddSingleton<DynamicParameters.Templates.Bolus.ScanParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.Bolus.ScanParameterDetailControl>();
		services.AddSingleton<DynamicParameters.Templates.Bolus.ReconParameterControl>();
		services.AddSingleton<DynamicParameters.Templates.Bolus.ReconParameterDetailControl>();

		services.AddSingleton<DynamicParameters.Templates.AdvancedReconParameterDetailControl>();
		services.AddSingleton<DynamicParameters.Templates.AdvancedScanParameterDetailControl>();

		#endregion
		services.DicomUtilityConfigInitialization();
        services.DicomUtilityConfigInitializationForWin();
        services.AddSingleton<ParameterDetailWindow>();
		services.AddSingleton<DoorAlertWindow>();
		services.AddSingleton<TimeDensityWindow>();
        services.AddSingleton<PostProcessSettingWindow>();

        services.AddSingleton<IDynamicTemplateService, DynamicTemplateService>();
		services.AddSingleton<ContentControlResolver>();

		services.AddSingleton<ScanControlsViewModel>();
		services.AddSingleton<ScanParameterViewModel>();
		services.AddSingleton<ReconParameterViewModel>();
		services.AddSingleton<ParameterDetailViewModel>();
		services.AddSingleton<ToolsViewModel>();
		services.AddSingleton<MessageTooltipViewModel>();
		services.AddSingleton<ScanReconViewModel>();
		services.AddSingleton<ProtocolSelectMainViewModel>();
		services.AddSingleton<ProtocolViewModel>();
		services.AddSingleton<ScanRangeViewModel>();
		services.AddSingleton<ProtocolPatientInfoViewModel>();

		services.AddSingleton<AutoPositioningViewModel>();
		services.AddSingleton<BodyPositionViewModel>();
		services.AddSingleton<BodyMapViewModel>();
		services.AddSingleton<TimelineViewModel>();
		services.AddSingleton<TaskListViewModel>();
		services.AddSingleton<ImageScrollViewModel>();

		services.AddSingleton<AutoPositioningWindow>();
		services.AddSingleton<ProtocolSaveAsViewModel>();
		services.AddSingleton<DoseNotificationViewModel>();
		services.AddSingleton<DoseAlertViewModel>();
		services.AddSingleton<CommonNotificationViewModel>();
		services.AddSingleton<CommonWarningViewModel>();
		services.AddSingleton<VoiceAPIConfigViewModel>();
		services.AddSingleton<DoorAlertViewModel>();
		services.AddSingleton<TimeDensityViewModel>();

		services.AddSingleton<ProtocolHostServiceExtension>();
		services.AddSingleton<EnhancedScanExtension>();
		services.AddSingleton<UIRelatedStatusServiceExtension>();
		return services;
	}
}