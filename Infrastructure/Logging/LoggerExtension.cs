//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/1 9:48:09           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace NV.CT.Logging
{
    public static class LoggerExtension
    {
        public static ILoggingBuilder AddNanoLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());

            return builder;
        }
    }
}
