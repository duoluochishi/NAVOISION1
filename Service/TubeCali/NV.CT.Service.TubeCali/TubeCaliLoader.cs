using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.Service.TubeCali.Models.Config;
using NV.CT.Service.TubeCali.Services;
using NV.CT.Service.TubeCali.Services.Interface;
using NV.CT.Service.TubeCali.ViewModels;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.Environment;

namespace NV.CT.Service.TubeCali
{
    public class TubeCaliLoader : IBootLoader
    {
        public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
            var filePath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, ConstInfo.TubeCalibration, ConstInfo.AppSettingFile);
            config.AddJsonFile(filePath, true, true);
        }

        public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
        {
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ExposureParamModel>(context.Configuration.GetSection(ConstInfo.AppSetting_ExposureParam));
            services.Configure<ThresholdModel>(context.Configuration.GetSection(ConstInfo.AppSetting_Threshold));
            services.AddSingleton<TubeCaliViewModel>();
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IDataStorageService, DataStorageService>();
            services.AddSingleton<EventDataParseService>();
            services.AddSingleton<TubeCaliService>();
            services.AddSingleton<AddressService>();
        }
    }
}