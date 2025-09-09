using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NV.CT.Service.HardwareTest.Services.Universal.PrintMessage
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
            logService.Info(ServiceCategory.HardwareTest, message);
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
            logService.Warn(ServiceCategory.HardwareTest, message);
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
            logService.Error(ServiceCategory.HardwareTest, message);
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
        /// 打印Response
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void PrintResponse(GenericResponse<bool> response)
        {
            if (response.Status) 
            {
                PrintLoggerInfo(response.Message);
            }
            else 
            {
                PrintLoggerError($"{response.Message}, error code - {response.ErrorCode}");
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
