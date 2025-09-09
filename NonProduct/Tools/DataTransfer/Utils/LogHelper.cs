using Microsoft.Extensions.Logging;
using NV.CT.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.Utils
{
    public static class LogHelper<T>
    {
        private static readonly ConcurrentDictionary<string, Logger<T>> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public static ILogger<T> CreateLogger(string category)
        {
            return _loggers.GetOrAdd(category, name => new Logger<T>(name));
        }
    }
}
