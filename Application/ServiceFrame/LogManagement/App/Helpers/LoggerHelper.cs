using Microsoft.Extensions.Logging;
using NV.CT.LogManagement.Models;
using NV.MPS.Exception;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NV.CT.LogManagement.Helpers
{
    public static class LoggerHelper
    {
        private const string LOG_EXTENSION = "*.log";
        private const string LOG_TYPE_TRC = "[TRC]";
        private const string LOG_TYPE_DEBUG = "[DBG]";
        private const string LOG_TYPE_INFO = "[INF]";
        private const string LOG_TYPE_WARNING = "[WRN]";
        private const string LOG_TYPE_ERROR = "[ERR]";
        private const string LOG_TYPE_FATAL = "[FTL]";

        //由于约定内容格式的限制，目前暂时仅支持MCS文件夹下以日期开头的文件名，比如"20231018_013.log"，其它文件以后再支持
        private static Regex LOG_FILE_FORMAT = new Regex("^MCS_\\d{4,}");

        public static IList<LogFileProfileModel> IdentifyLogFiles(string logDirectorPath, DateTime beginDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(logDirectorPath) || !Directory.Exists(logDirectorPath))
            {
                throw new DirectoryNotFoundException($"Not found path:{logDirectorPath}");
            }

            var identifiedLogFiles = new List<LogFileProfileModel>();
            var logFolder = new DirectoryInfo(logDirectorPath);
            var logfiles = logFolder.GetFiles(LOG_EXTENSION, SearchOption.TopDirectoryOnly)
                                    .Where(f => f.LastWriteTime >= beginDate && f.LastWriteTime < endDate).ToArray();
            foreach (FileInfo file in logfiles)
            {
                //only accept the file name that starts with number like 20230126**.log
                if (!LOG_FILE_FORMAT.IsMatch(file.Name))
                {
                    continue;
                }
                identifiedLogFiles.Add(new LogFileProfileModel()
                {
                    FileName = file.Name,
                    FileFullPath = file.FullName,
                    LastWriteTime = file.LastWriteTime
                });
            }

            return identifiedLogFiles.OrderBy(f => f.LastWriteTime).ToList();
        }

        public static IList<LogLineModel> GetLogLinesByFile(LogFileProfileModel logFileProfileModel)
        {
            var fileFullPath = logFileProfileModel.FileFullPath;
            if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath))
            {
                throw new FileNotFoundException($"Not found file:{fileFullPath}");
            }

            var lines = new List<string>();
            try
            {
                using (var fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.Default))
                {
                    while (!sr.EndOfStream)
                    {
                        lines.Add(sr.ReadLine());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new NanoException($"Failed to read file: {fileFullPath}, the exception is：{ex.Message}");
            }

            if (lines.Count == 0)
            {
                return new List<LogLineModel>();
            }

            var logLines = ConvertLinesToLogFiles(lines);
            foreach (var logLine in logLines)
            {
                logLine.LogFile = logFileProfileModel;
            }
            return logLines;
        }

        /// <summary>
        /// 目前这里暂时只支持MCS日志内容格式，以后可以引入策略设计模式来支持其它日志内容的扩展
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static List<LogLineModel> ConvertLinesToLogFiles(List<string> lines)
        {
            var logLines = new List<LogLineModel>();
            var trimmedLines = new StringBuilder();
            int lineNumber = 0;

            foreach (string line in lines)
            {
                lineNumber++;

                if (!IsNormalLogLine(line))
                {
                    //process trimmed log line like below for previous LogLineModel:
                    //   at NV.CT.LogManagement.Helpers.LoggerHelper.ConvertLineToLogFile(Int32 lineNumber, String line) in F:\mcs20.code\Application\ServiceFrame\LogManagement\App\Helpers\LoggerHelper.cs:line 117
                    trimmedLines.Append(string.Concat(System.Environment.NewLine, line));
                    continue;
                }

                //process normal log line like below:
                //example: 2024-01-26 13:31:06.7754 [TRC] (LoggingServer, NV.CT.LoggingServer.MainRunner) Start logging service...
                if (trimmedLines.Length > 0)
                {
                    ConnectTrimmedContent(logLines.LastOrDefault(), trimmedLines);
                    trimmedLines.Clear();
                }

                logLines.Add(ConvertNormalLine(lineNumber, line));
            }

            return logLines;
        }

        private static void ConnectTrimmedContent(LogLineModel? loglineModel, StringBuilder trimmedLines)
        {
            if (loglineModel == null)
            {
                return;
            }

            loglineModel.Content = string.Concat(loglineModel.Content, trimmedLines.ToString());
        }

        /// <summary>
        /// 目前这里暂时只支持MCS日志内容格式，以后可以引入策略设计模式来支持其它日志内容的扩展
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private static LogLineModel ConvertLineToLogFile(int lineNumber, string line)
        {
            //example 1: 2024-01-26 13:31:06.7754 [TRC] (LoggingServer, NV.CT.LoggingServer.MainRunner) Start logging service...
            if (IsNormalLogLine(line))
            {
                return ConvertNormalLine(lineNumber, line);
            }
            else
            {
                return ConvertRawLine(lineNumber, line);
            }
        }

        private static bool IsNormalLogLine(string line)
        {
            if (line is null || line.Length <= 25)
            {
                return false;
            }

            string datetime = line.Substring(0, 24);
            return DateTime.TryParse(datetime, out var occurredDateTime);
        }

        private static LogLineModel ConvertNormalLine(int lineNumber, string line)
        {
            //example 1: 2024-01-26 13:31:06.7754 [TRC] (LoggingServer, NV.CT.LoggingServer.MainRunner) Start logging service...
            string datetime = line.Substring(0, 24);
            string logLevel = line.Substring(26, 3);
            int positionOfBeginingModule = line.IndexOf(LogConstants.SIGN_LEFT_BRACKET);
            int positionOfEndingModule = line.IndexOf(LogConstants.SIGN_COMMA);
            string module = line.Substring(32, (positionOfEndingModule - positionOfBeginingModule - 1));
            int positionOfServiceName = line.IndexOf(LogConstants.SIGN_RIGHT_BRACKET);
            string serviceName = line.Substring(positionOfEndingModule + 2, (positionOfServiceName - positionOfEndingModule - 2));
            string content = line.Substring(positionOfServiceName + 2);

            var logLineModel = new LogLineModel();
            logLineModel.LineNumber = lineNumber;
            logLineModel.OccurredDateTime = datetime;
            logLineModel.LogLevel = logLevel;
            logLineModel.ModuleName = module;
            logLineModel.ServiceName = serviceName;
            logLineModel.Content = content;
            return logLineModel;
        }

        private static LogLineModel ConvertRawLine(int lineNumber, string line)
        {
            var logLineModel = new LogLineModel();
            logLineModel.LineNumber = lineNumber;
            logLineModel.OccurredDateTime = string.Empty;
            logLineModel.LogLevel = LogConstants.LOG_LEVEL_ERR;
            logLineModel.ModuleName = string.Empty;
            logLineModel.ServiceName = string.Empty;
            logLineModel.Content = line;
            return logLineModel;
        }

        private static LogLevel ConvertLogLevel(string logLevel)
        {
            if (string.IsNullOrEmpty(logLevel))
                return LogLevel.None;

            var logLevelType = LogLevel.None;
            switch (logLevel)
            {
                case LOG_TYPE_TRC:
                    logLevelType = LogLevel.Trace;
                    break;
                case LOG_TYPE_DEBUG:
                    logLevelType = LogLevel.Debug;
                    break;
                case LOG_TYPE_INFO:
                    logLevelType = LogLevel.Information;
                    break;
                case LOG_TYPE_WARNING:
                    logLevelType = LogLevel.Warning;
                    break;
                case LOG_TYPE_ERROR:
                    logLevelType = LogLevel.Error;
                    break;
                case LOG_TYPE_FATAL:
                    logLevelType = LogLevel.Critical;
                    break;
                default:
                    logLevelType = LogLevel.None;
                    break;
            }

            return logLevelType;

        }

    }
}
