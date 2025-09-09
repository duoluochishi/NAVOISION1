using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public abstract class AbstractHandler
    {
        public AbstractHandler(
            ILogService logger,
            IMessagePrintService loggerUI,
            CancellationTokenSource cancellationTokenSource)
        {
            _logger = logger;
            _loggerUI = loggerUI;

            _cancellationTokenSource = cancellationTokenSource;
        }

        #region public interfaces

        public Handler HandlerConfig { get; set; }

        /// <summary>
        /// Offline Reconstruction的ID
        /// </summary>
        public string ReconID { get; private set; }

        public void RegisterEvent()
        {
            _logger.Debug(ServiceCategory.AutoCali, $"[{nameof(RegisterEvent)}] Entered");

            UnregisterEvent();

            RegisterEventCore();
        }

        public void UnregisterEvent()
        {
            _logger.Debug(ServiceCategory.AutoCali, $"[{nameof(UnregisterEvent)}] Entered");

            UnregisterEventCore();
        }

        //public async Task<CommandResult> Execute(object commandParameter)
        public async Task<CommandResult> Execute<T>(T commandParameter) where T : class
        {
            //检测Offline Reconstruction服务是否在线
            CommandResult commandResult = await ExecuteCore(commandParameter);

            //不再监听异步事件
            UnregisterEventCore();

            return commandResult;
        }


        #endregion public interfaces

        #region private methods

        private async Task TryInitializeProxy()
        {
            if (!_initialized)
            {
                RegisterEvent();

                await Task.Delay(1000);
            }

            _initialized = true;
        }

        protected abstract void RegisterEventCore();

        protected abstract void UnregisterEventCore();

        //protected abstract Task<CommandResult> ExecuteCore(object commandParameter);
        protected abstract Task<CommandResult> ExecuteCore<T>(T commandParameter) where T : class;

        #endregion  private methods

        private CommandResult MakeAsyncCommandResult(string sender, CommandStatus commandStatus, ErrorCodes errorCodes)
        {
            CommandResult commandResult = new()
            {
                Sender = sender,
                Status = commandStatus,
                ErrorCodes = errorCodes
            };
            commandResult.Description = ToCommandStatusInfo(commandStatus, errorCodes);

            return commandResult;
        }

        protected string ToCommandStatusInfo(CommandStatus commandStatus, ErrorCodes errorCodes)
        {
            string description = commandStatus.ToString();

            try
            {
                description = commandStatus switch
                {
                    CommandStatus.Cancelled => "Canceled",
                    CommandStatus.Success => "Succeeded",
                    CommandStatus.Failure => $"Failed with error '{ToErrorCodesInfo(errorCodes)}'",
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ServiceCategory.AutoCali, $"{nameof(ToCommandStatusInfo)} occur an exception:{ex}");
            }
            return description;
        }

        protected string ToErrorCodesInfo(ErrorCodes errorCodes)
        {
            string errofInfo = string.Join(',', errorCodes.Codes);
            return errofInfo;
        }

        #region private fields

        protected readonly ILogService _logger;
        protected readonly IMessagePrintService _loggerUI;

        private bool _initialized;

        protected CancellationTokenSource _cancellationTokenSource;
        protected TaskToken _taskToken;

        #endregion private fields
    }
}
