//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/6 17:27:07     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Server.Services;

namespace NV.CT.ConfigService.Impl;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddConfigServices(this IServiceCollection services)
    {       
        services.AddSingleton<IErrorCodeService, ErrorCodeService>();
        //services.AddSingleton<IHardwareService, HardwareService>();
        services.AddSingleton<IImageAnnotationService, ImageAnnotationService>();         
        services.AddSingleton<IPrintProtocolConfigService, PrintProtocolConfigService>();
        services.AddSingleton<IFilmSettingsConfigService, FilmSettingsConfigService>();

        services.AddSingleton<IPatientConfigService, PatientConfigService>();     
        services.AddSingleton<IStudyListColumnsConfigService, StudyListColumnsConfigService>();
        services.AddSingleton<IFilterConfigService, FilterConfigService>();
        return services;
    }
}