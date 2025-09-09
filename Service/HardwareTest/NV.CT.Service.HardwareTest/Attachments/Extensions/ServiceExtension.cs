using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Managers;
using NV.CT.Service.HardwareTest.Attachments.Managers.Abstractions;
using NV.CT.Service.HardwareTest.Attachments.Repository;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Repositories;
using NV.CT.Service.HardwareTest.Services.Integrations.SystemEnvironment;
using NV.CT.Service.HardwareTest.Services.Integrations.SystemEnvironment.Impls;
using NV.CT.Service.HardwareTest.Services.Universal.EventData;
using NV.CT.Service.HardwareTest.Services.Universal.EventData.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.UserControls.Components.Detector;
using NV.CT.Service.HardwareTest.UserControls.Components.XRaySource;
using NV.CT.Service.HardwareTest.UserControls.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.UserControls.Integrations.DataAcquistion;
using NV.CT.Service.HardwareTest.UserControls.Integrations.ImageChain;
using NV.CT.Service.HardwareTest.UserControls.Universal;
using NV.CT.Service.HardwareTest.ViewModels.Components.Collimator;
using NV.CT.Service.HardwareTest.ViewModels.Components.Detector;
using NV.CT.Service.HardwareTest.ViewModels.Components.Table;
using NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ComponentEnablement;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ComponentStatus;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ImageChain;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.SelfCheck;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.SystemEnvironment;
using NV.CT.Service.HardwareTest.ViewModels.Navigation;
using NV.CT.Service.HardwareTest.ViewModels.Universal;
using NV.CT.Service.HardwareTest.Views.Components.Collimator;
using NV.CT.Service.HardwareTest.Views.Components.Detector;
using NV.CT.Service.HardwareTest.Views.Components.Table;
using NV.CT.Service.HardwareTest.Views.Components.XRaySource;
using NV.CT.Service.HardwareTest.Views.Integrations.ComponentEnablement;
using NV.CT.Service.HardwareTest.Views.Integrations.ComponentStatus;
using NV.CT.Service.HardwareTest.Views.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Views.Integrations.ImageChain;
using NV.CT.Service.HardwareTest.Views.Integrations.SelfCheck;
using NV.CT.Service.HardwareTest.Views.Integrations.SystemEnvironment;
using NV.CT.Service.HardwareTest.Views.Navigation;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddModelToViewModelMappings(this IServiceCollection services)
        {
            //Navigation
            services.AddSingleton<NavigationView>();
            services.AddSingleton<NavigationViewModel>();

            //XRaySource
            services.AddSingleton<XRaySourceComprehensiveTestingView>();
            services.AddSingleton<XRaySourceComprehensiveTestingViewModel>();
            services.AddSingleton<XRaySourceChartPlotView>();
            services.AddSingleton<XRaySourceChartPlotViewModel>();
            services.AddSingleton<XRaySourceKVMACoefficientsView>();
            services.AddSingleton<XRaySourceKVMACoefficientsViewModel>();
            services.AddTransient<XRaySourceAddKVMAPairView>();
            services.AddTransient<XRaySourceAddKVMAPairViewModel>();

            //Collimator
            services.AddSingleton<CollimatorCalibrationView>();
            services.AddSingleton<CollimatorCalibrationViewModel>();

            //Table
            services.AddSingleton<TableThreeAxisMotionTestingView>();
            services.AddSingleton<TableThreeAxisMotionTestingViewModel>();

            //Detertor
            services.AddSingleton<DetectorStatsView>();
            services.AddSingleton<DetectorStatsViewModel>();
            services.AddSingleton<UpdateDetectorBoardSeriesNumberView>();
            services.AddSingleton<UpdateDetectorBoardSeriesNumberViewModel>();
            services.AddSingleton<BroswerDetectBoardUpdateHistoryView>();
            services.AddSingleton<BroswerDetectBoardUpdateHistoryViewModel>();
            services.AddSingleton<SetTemperatureView>();
            services.AddSingleton<SetTemperatureViewModel>();

            //Data Acquisition
            services.AddSingleton<DataAcquisitionTestingView>();
            services.AddSingleton<DataAcquisitionTestingViewModel>();
            services.AddSingleton<DataAcquisitionImageSortView>();
            services.AddSingleton<DrawHorizontalLineView>();
            services.AddSingleton<DrawHorizontalLineViewModel>();

            //Image Chain
            services.AddSingleton<ImageChainTestingView>();
            services.AddSingleton<ImageChainTestingViewModel>();
            services.AddSingleton<ImageChainImageSortView>();

            //OnlineStatus
            services.AddSingleton<ComponentStatusTestingView>();
            services.AddSingleton<ComponentStatusTestingViewModel>();

            //Self Check
            services.AddSingleton<SelfCheckTestingView>();
            services.AddSingleton<SelfCheckTestingViewModel>();

            //SystemEnvironment
            services.AddSingleton<SystemEnvironmentView>();
            services.AddSingleton<SystemEnvironmentViewModel>();

            //ComponentEnablement
            services.AddSingleton<ComponentEnablementView>();
            services.AddSingleton<ComponentEnablementViewModel>();

            //Common
            services.AddTransient<UniversalPopUpView>();
            services.AddTransient<UniversalPopUpViewModel>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //Navigation Services
            services.AddSingleton<INavigationService, NavigationService>();
            //Log Services
            services.AddSingleton<ILogService>(LogService.Instance);
            services.AddTransient<IMessagePrintService, MessagePrintService>();
            //XRaySource Services
            services.AddSingleton<IEventDataAddressService, EventDataAddressService>();
            //Repository
            services.AddSingleton<IRepository<XRaySourceHistoryData>, XRaySourceHistoryDataRepository>();
            services.AddSingleton<IRepository<DetectorTemperatureData>, DetectorTemperatureDataRepository>();
            //XRaySource Interact Manager
            services.AddSingleton<IXRaySourceInteractManager, XRaySourceInteractManager>();
            //System Environment Set Service
            services.AddSingleton<ISystemEnvironmentSetService, SystemEnvironmentSetService>();

            return services;
        }

        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                                          .AddJsonFile("HardwareTestSettings.json", optional: false, reloadOnChange: true)
                                          .Build();

            //添加ConfigurationRoot
            services.AddSingleton(configuration);
            //添加ConfigOption
            services.Configure<HardwareTestConfigOptions>(configuration.GetSection(nameof(HardwareTestConfigOptions)));
            services.Configure<XRaySourceConfigOptions>(configuration.GetSection(nameof(XRaySourceConfigOptions)));
            services.Configure<DataAcquisitionConfigOptions>(configuration.GetSection(nameof(DataAcquisitionConfigOptions)));
            services.Configure<CollimatorConfigOptions>(configuration.GetSection(nameof(CollimatorConfigOptions)));
            services.Configure<ComponentStatusConfigOptions>(configuration.GetSection(nameof(ComponentStatusConfigOptions)));
            services.Configure<XRaySourceKVMACoefficientConfigOptions>(configuration.GetSection(nameof(XRaySourceKVMACoefficientConfigOptions)));

            return services;
        }
    }
}