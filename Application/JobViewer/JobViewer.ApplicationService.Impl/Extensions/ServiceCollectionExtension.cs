//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/7 10:11:09           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace NV.CT.JobViewer.ApplicationService.Impl.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ToProfile));
            //services.AddHostedService<OfflineReconTaskHandler>();
            //services.AddHostedService<DicomFileImportTaskHandler>();
            return services;
        }
    }
}
