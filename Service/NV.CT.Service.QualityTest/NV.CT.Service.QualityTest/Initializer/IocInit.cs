using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.Phantoms;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.QualityTest.Services.Impls;
using NV.CT.Service.QualityTest.ViewModels;
using NV.CT.Service.QualityTest.Views;
using NV.CT.Service.QualityTest.Views.QTUC;
using NV.CT.Service.Universal.PrintMessage;
using NV.CT.Service.Universal.PrintMessage.Abstractions;

namespace NV.CT.Service.QualityTest.Initializer
{
    internal static class IocInit
    {
        public static void Init()
        {
            var services = new ServiceCollection()
                          .AddConfiguration()
                          .AddCommon()
                          .AddServices()
                          .AddModels()
                          .AddViewModels()
                          .AddViews();
            Global.SetServiceProvider(services.BuildServiceProvider());
        }

        private static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                               .SetBasePath(Global.ConfigPath)
                               .AddJsonFile(Global.AppSettingFile, true, false)
                               .Build();
            return services.AddSingleton(configuration.GetSection(Global.ConfigNode).Get<ConfigModel>())
                           .AddSingleton(configuration.GetSection(Global.PhantomNode).Get<IntegrationPhantomModel>())
                           .AddSingleton(configuration.GetSection(Global.ParamDisplayNode).Get<ParamDisplayItemModel[]>())
                           .AddSingleton(_ =>
                            {
                                var items = configuration.GetSection(Global.ItemsNode).Get<ItemModel[]>();
                                ItemParamInit.Init(items);
                                return items;
                            });
        }

        private static IServiceCollection AddCommon(this IServiceCollection services)
        {
            return services.AddSingleton<ILogService>(LogService.Instance)
                           .AddSingleton<IDialogService>(DialogService.Instance)
                           .AddSingleton<IMessagePrintService>(service => new MessagePrintService(service.GetRequiredService<ILogService>())
                            {
                                XServiceCategory = ServiceCategory.QualityTest,
                            })
                           .AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services.AddSingleton<IDataStorageService, DataStorageService>()
                           .AddSingleton<ITableService, TableService>()
                           .AddSingleton<IIntegrationPhantomService, IntegrationPhantomService>();
        }

        private static IServiceCollection AddModels(this IServiceCollection services)
        {
            return services.AddSingleton<ReportHeadInfoModel>();
        }

        private static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            return services.AddTransient<QTViewModel>();
        }

        private static IServiceCollection AddViews(this IServiceCollection services)
        {
            return services.AddTransient<ReportHeadInfoWindow>()
                           .AddTransient<IntegrationPhantomUC>()
                           .AddTransient<SliceThicknessAxialUC>()
                           .AddTransient<SliceThicknessHelicalUC>()
                           .AddTransient<HomogeneityUC>()
                           .AddTransient<CTOfWaterUC>()
                           .AddTransient<NoiseOfWaterUC>()
                           .AddTransient<ContrastScaleUC>()
                           .AddTransient<HighContrastResolutionXYUC>()
                           .AddTransient<HighContrastResolutionZUC>();
        }
    }
}