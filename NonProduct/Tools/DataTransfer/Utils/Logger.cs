using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.Utils
{
    public class Logger<T> : ILogger<T>
    {
        Logger _log;

        public Logger(string name)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");
            _log = new LoggerConfiguration().WriteTo.File(logPath, rollingInterval: RollingInterval.Day).CreateLogger();
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            System.Diagnostics.Debug.WriteLine($"{logLevel}: {formatter(state, exception)}");
            if (logLevel == LogLevel.Error)
                _log?.Error($"{formatter(state, exception)}");
            else if (logLevel == LogLevel.Information)
                _log?.Information($"{formatter(state, exception)}");
            else if (logLevel == LogLevel.Debug)
                _log?.Debug($"{formatter(state, exception)}");
            else if (logLevel == LogLevel.Trace)
                _log?.Verbose($"{formatter(state, exception)}");
            else
                _log?.Warning($"{formatter(state, exception)}");
        }
    }
}
