using NV.CT.Service.Enums;
using NV.CT.Service.Models;
using System;
using System.Collections.Generic;

namespace NV.CT.Service.Universal.PrintMessage.Abstractions
{
    public interface IMessagePrintService
    {
        event EventHandler<string>? OnConsoleMessageChanged;
        void PrintToConsole(string message, PrintLevel level = PrintLevel.Info);
        void PrintLoggerError(string message);
        void PrintLoggerInfo(string message);
        void PrintLoggerWarn(string message);       
        void PrintEnumerable(IEnumerable<LoggerMessage> messages);
        void Clear();
    }
}