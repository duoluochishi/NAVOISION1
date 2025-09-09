using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Upgrade.DAL.Implements;
using NV.CT.Service.Upgrade.DAL.Interfaces;
using NV.CT.Service.Upgrade.Repositories.Implements;
using NV.CT.Service.Upgrade.Repositories.Interfaces;
using NV.CT.Service.Upgrade.Services.Implements;
using NV.CT.Service.Upgrade.Services.Interfaces;
using NV.CT.Service.Upgrade.ViewModels;

namespace NV.CT.Service.Upgrade.Initializer
{
    internal static class IocInit
    {
        public static void Init()
        {
            var services = new ServiceCollection();
            AddCommon(services);
            AddServices(services);
            AddViewModel(services);
            Global.SetServiceProvider(services.BuildServiceProvider());
        }

        private static void AddCommon(IServiceCollection services)
        {
            services.AddSingleton<ILogService>(LogService.Instance);
            services.AddSingleton<IDialogService>(DialogService.Instance);
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigFAO, ConfigFAO>()
                    .AddSingleton<IConfigRepo, ConfigRepo>()
                    .AddSingleton<IConfigService, ConfigService>();
        }

        private static void AddViewModel(IServiceCollection services)
        {
            services.AddSingleton<UpgradeViewModel>();
        }
    }
}