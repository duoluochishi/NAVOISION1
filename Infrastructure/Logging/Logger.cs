//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/1 9:46:02           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace NV.CT.Logging
{
    public class Logger : ILogger
    {
        private readonly string _name;
        private readonly string _clientName;

        public Logger(string name)
        {
            _name = name;
            var processName = Process.GetCurrentProcess().ProcessName;
            _clientName = processName.Replace("NV.CT.", "").Replace(".exe", "");
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if ((logLevel == LogLevel.Trace || logLevel == LogLevel.Debug || logLevel == LogLevel.Information)
                && (_name.IndexOf("System.") >= 0 || _name.IndexOf("Microsoft.") >= 0 ))
            {
                return;
            }

            //StackTrace trace = new StackTrace();
            //var realMethodName = trace.GetFrame(7)?.GetMethod()?.Name;

            PipelineClient.Instance.PushMessage(JsonConvert.SerializeObject(new
            {
                CreateTime = DateTime.Now,
                Level = logLevel,
                ClientName = _clientName,
                ClassName = _name,
                //MethodName = realMethodName,
                Exception = exception,
                Message = formatter(state, exception)
            }));
        }
    }
}
