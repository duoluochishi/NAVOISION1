using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.Logging;
using NV.CT.Service.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NV.CT.Service.Common
{
    public enum ServiceCategory
    {
        Common,
        TubeWarmUp,
        AutoCali,
        UpgradeFirmware,
        QualityTest,
        HardwareTest,
        DMSTest,
        SourceComponentCali,
        ComponentHistory,
        TubeHistory,
    }

    public class LogService : ILogService
    {
        private const string NAMESPACEPREFIX = "NV.CT.Service.";
        private readonly Dictionary<ServiceCategory, string> _categoryNames;

        private static readonly Lazy<LogService> _instance =
            new Lazy<LogService>(() => new LogService());

        public static LogService Instance => _instance.Value;
        private ILogger<LogService> _logger;

        private LogService()
        {
            _logger = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<ILogger<LogService>>();
            _categoryNames = new Dictionary<ServiceCategory, string>();
            foreach (var category in Enum.GetValues<ServiceCategory>())
            {
                _categoryNames.Add(category, $"{NAMESPACEPREFIX}{category}");
            }
        }

        public void Debug(ServiceCategory serviceCategory,
            string message)
        {
            Write(serviceCategory, LogType.Debug, message);
            WriteLogServer(serviceCategory, LogType.Debug, message);
        }

        public void Info(ServiceCategory serviceCategory,
            string message)
        {
            Write(serviceCategory, LogType.Info, message);
            WriteLogServer(serviceCategory, LogType.Info, message);
        }

        public void Warn(ServiceCategory serviceCategory,
            string message)
        {
            Write(serviceCategory, LogType.Warning, message);
            WriteLogServer(serviceCategory, LogType.Warning, message);
        }

        public void Error(ServiceCategory serviceCategory,
            string message)
        {
            Error(serviceCategory, message, null);
        }

        public void Error(ServiceCategory serviceCategory,
            string message, Exception e)
        {
            Write(serviceCategory, LogType.Error, $"{message}{ExtractStackInfo(e)}");
            WriteLogServer(serviceCategory, LogType.Error, $"{message}{ExtractStackInfo(e)}");
        }

        public void Fatal(ServiceCategory serviceCategory,
            string message)
        {
            Write(serviceCategory, LogType.Fatal, message);
            WriteLogServer(serviceCategory, LogType.Fatal, message);
        }

        private string ExtractStackInfo(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }
            return stringBuilder.ToString();
        }

        private string GetCategoryName(ServiceCategory serviceCategory)
        {
            string categoryName = string.Empty;
            if (_categoryNames.TryGetValue(serviceCategory, out var category))
            {
                categoryName = category;
            }
            else
            {
                categoryName = serviceCategory.ToString();
            }
            return categoryName;
        }

        private void Write(ServiceCategory serviceCategory,
            LogType type, string msg)
        {
            var categoryName = GetCategoryName(serviceCategory);
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {categoryName} {type} {msg}");
        }

        private void WriteLogServer(ServiceCategory serviceCategory,
            LogType logType, string msg)
        {
            var categoryName = GetCategoryName(serviceCategory);
            string message = $"{categoryName}, {msg}";
            switch (logType)
            {
                case LogType.Debug:
                    //NV.CT.Logging.LogManager.Instance.Debug(categoryName, msg);
                    _logger.LogDebug(message);
                    break;

                case LogType.Info:
                    //NV.CT.Logging.LogManager.Instance.Information(categoryName, msg);
                    _logger.LogInformation(message);
                    break;

                case LogType.Warning:
                    //NV.CT.Logging.LogManager.Instance.Warning(categoryName, msg);
                    _logger.LogWarning(message);
                    break;

                case LogType.Error:
                    //NV.CT.Logging.LogManager.Instance.Error(categoryName, msg);
                    _logger.LogError(message);
                    break;

                case LogType.Fatal:
                    //NV.CT.Logging.LogManager.Instance.Fatal(categoryName, msg);
                    _logger.LogCritical(message);
                    break;

                default:
                    break;
            }
        }
    }

    public enum LogType
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal,
    }

    /// <summary>
    /// 日志工具
    /// </summary>
    public class LogWrapper
    {
        public LogWrapper(ServiceCategory serviceCategory)
        {
            _serviceCategory = serviceCategory;
        }

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

        private void WriteAdapter(LogType type, string msg)
        {
            Write(type, msg);
            WriteLogServer(type, msg);
        }

        private void Write(LogType type, string msg)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {type} {msg}");
        }

        private void WriteLogServer(LogType logType, string msg)
        {
            switch (logType)
            {
                case LogType.Fatal:
                    LogService.Instance.Fatal(_serviceCategory, msg);
                    break;

                case LogType.Error:
                    LogService.Instance.Error(_serviceCategory, msg);
                    break;

                case LogType.Warning:
                    LogService.Instance.Warn(_serviceCategory, msg);
                    break;

                case LogType.Info:
                    LogService.Instance.Info(_serviceCategory, msg);
                    break;

                case LogType.Debug:
                    LogService.Instance.Debug(_serviceCategory, msg);
                    break;

                default:
                    break;
            }
        }

        #region Fields

        private ServiceCategory _serviceCategory;

        #endregion Fields
    }
}