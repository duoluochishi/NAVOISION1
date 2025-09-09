using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Models;
using System;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions
{
    public interface IMessagePrintService
    {
        event EventHandler<string>? OnConsoleMessageChanged;
        void PrintToConsole(string message, PrintLevel level = PrintLevel.Info);
        void PrintLoggerError(string message);
        void PrintLoggerInfo(string message);
        void PrintLoggerWarn(string message); 
        void PrintEnumerable(IEnumerable<LoggerMessage> messages);
        void PrintResponse(GenericResponse<bool> response);
        void Clear();
    }
}