using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System.IO;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class ValidatePostOffsetHandler
    {
        private readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;
        private string _rawDataDirectory;
        private ScanParam _scanParam;

        public ValidatePostOffsetHandler(
            ILogService logger,
            IMessagePrintService loggerUI,
            string rawDataDirectory)
        {
            _logService = logger;
            _messagePrintService = loggerUI;
            _rawDataDirectory = rawDataDirectory;
        }

        public async Task<CommandResult> ExecuteAsync()
        {
            CommandResult commandResult = new();

            if (!Directory.Exists(_rawDataDirectory))
            {
                commandResult.Status = CommandStatus.Failure;

                ErrorInfo = $"投影图数据的路径不存在，路径'{_rawDataDirectory}'";
                this._messagePrintService.PrintToConsole($"{ErrorInfo}", Enums.PrintLevel.Error);
                //commandResult.AddErrorCode();
            }
            else
            {
                string postOffsetErrorInfoFile = Path.Combine(_rawDataDirectory, CONST_PostOffsetDirectoryName, CONST_PostOffsetErrorInfoFileName);
                this._logService.Info(ServiceCategory.AutoCali, $"验证PostOffset数据是否正确。通过检查是否存在错误信息文件 '{postOffsetErrorInfoFile}'");

                if (File.Exists(postOffsetErrorInfoFile))
                {
                    commandResult.Status = CommandStatus.Failure;

                    ErrorInfo = "PostOffset数据发现错误";
                    this._messagePrintService.PrintToConsole($"{ErrorInfo}", Enums.PrintLevel.Error);
                    this._logService.Error(ServiceCategory.AutoCali, $"{ErrorInfo}, 文件 '{postOffsetErrorInfoFile}'");
                }
                else
                {
                    this._logService.Info(ServiceCategory.AutoCali, $"验证PostOffset数据正确！");
                }
            }

            return commandResult;
        }

        public string ErrorInfo { get; set; }

        private static readonly string CONST_PostOffsetDirectoryName = "PostOffset";
        private static readonly string CONST_PostOffsetErrorInfoFileName = "PostOffsetError.txt";
    }
}
