//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.JobViewer.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ToApplicationProfile));
            return services;
        }
    }
}
