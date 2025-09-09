//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/1 9:47:16           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace NV.CT.Logging
{
    [ProviderAlias("NanoLogger")]
    public sealed class LoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Logger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public LoggerProvider()
        {
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new Logger(name));

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
