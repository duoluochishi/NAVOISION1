using NV.CT.FacadeProxy.Essentials.Logger;
using System;
using System.Text;

namespace NV.CT.Service.Common
{
    public class ProxyLogWrapper : ILogger
    {
        public void Debug(string message)
        {
            WriteAdapter(LogType.Debug, message);
        }

        public void Error(string message)
        {
            WriteAdapter(LogType.Error, message);
        }

        public void Error(string message, Exception exception)
        {
            var msg = $"{message}{System.Environment.NewLine}{ExtractStackInfo(exception)}";
            WriteAdapter(LogType.Fatal, msg);
        }
        private string ExtractStackInfo(Exception exception)
        {

            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        public void Fatal(string message)
        {
            WriteAdapter(LogType.Fatal, message);
        }

        public void Info(string message)
        {
            WriteAdapter(LogType.Info, message);
        }

        public void Warn(string message)
        {
            WriteAdapter(LogType.Warning, message);
        }

        public void Warn(string message, Exception exception)
        {
            var msg = $"{message}{System.Environment.NewLine}{ExtractStackInfo(exception)}";
            WriteAdapter(LogType.Warning, msg);
        }

        private static void WriteAdapter(LogType type, string msg)
        {
            Write(type, msg);
            WriteLogServer(type, msg);
        }
        private static void Write(LogType type, string msg)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {type} {msg}");
        }
        private static void WriteLogServer(LogType logType, string msg)
        {
            switch (logType)
            {
                case LogType.Fatal:
                    LogService.Instance.Fatal(ServiceCategory.Common, msg);
                    break;
                case LogType.Error:
                    LogService.Instance.Error(ServiceCategory.Common, msg);
                    break;
                case LogType.Warning:
                    LogService.Instance.Warn(ServiceCategory.Common, msg);
                    break;
                case LogType.Info:
                    LogService.Instance.Info(ServiceCategory.Common, msg);
                    break;
                case LogType.Debug:
                    LogService.Instance.Debug(ServiceCategory.Common, msg);
                    break;
                default:
                    break;
            }
        }
    }
}
