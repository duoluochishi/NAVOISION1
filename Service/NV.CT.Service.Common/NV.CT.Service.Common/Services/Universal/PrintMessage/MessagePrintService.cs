using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Enums;
using NV.CT.Service.Helpers;
using NV.CT.Service.Models;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NV.CT.Service.Universal.PrintMessage
{
    public class MessagePrintService : IMessagePrintService
    {
        private readonly ILogService logService;
        private readonly StringBuilder consoleMessageBuilder;
        private readonly object messagePrinterLocker;

        public MessagePrintService(ILogService logService)
        {
            this.logService = logService;
            consoleMessageBuilder = new();
            messagePrinterLocker = new();
        }

        public ServiceCategory XServiceCategory { get; set; } = ServiceCategory.Common;

        /// <summary>
        /// 消息改变事件
        /// </summary>
        public event EventHandler<string>? OnConsoleMessageChanged;

        /// <summary>
        /// 打印到Console
        /// </summary>
        /// <param name="console"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public void PrintToConsole(string message, PrintLevel level = PrintLevel.Info)
        {
            lock (messagePrinterLocker)
            {
                /** 包装信息 **/
                string wrapperMessage = MessagePrinterHelper.MessageWrapper(message, level);
                /** 更新builder **/
                consoleMessageBuilder.AppendLine(wrapperMessage);
                /** 通知 **/
                OnConsoleMessageChanged?.Invoke(this, consoleMessageBuilder.ToString());
            }
        }

        /// <summary>
        /// 打印并记录日志 - Info
        /// </summary>
        /// <param name="message"></param>
        public void PrintLoggerInfo(string message)
        {
            /** 显示 **/
            PrintToConsole(message, PrintLevel.Info);
            /** 记录 **/
            logService.Info(XServiceCategory, message);
        }

        /// <summary>
        /// 打印并记录日志 - Warn
        /// </summary>
        /// <param name="message"></param>
        public void PrintLoggerWarn(string message)
        {
            /** 显示 **/
            PrintToConsole(message, PrintLevel.Warn);
            /** 记录 **/
            logService.Warn(XServiceCategory, message);
        }

        /// <summary>
        /// 打印并记录日志 - Error
        /// </summary>
        /// <param name="message"></param>
        public void PrintLoggerError(string message)
        {
            /** 显示 **/
            PrintToConsole(message, PrintLevel.Error);
            /** 记录 **/
            logService.Error(XServiceCategory, message);
        }

        /// <summary>
        /// 打印批量过程信息
        /// </summary>
        /// <param name="messages"></param>
        public void PrintEnumerable(IEnumerable<LoggerMessage> messages)
        {
            foreach (var item in messages)
            {
                switch (item.level)
                {
                    case PrintLevel.Info: PrintLoggerInfo(item.message); break;
                    case PrintLevel.Warn: PrintLoggerWarn(item.message); break;
                    case PrintLevel.Error: PrintLoggerError(item.message); break;
                }
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            /** 清空builder **/
            consoleMessageBuilder.Clear();
            /** 通知 **/
            OnConsoleMessageChanged?.Invoke(this, string.Empty);
        }

    }
}
