using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic
{
    public class TaskToken
    {
        public TaskToken(
            ILogService logService,
            IMessagePrintService messagePrintService,
            string taskName,
            CancellationTokenSource cancellationTokenSource)
        {

            this._logService = logService;
            this._messagePrintService = messagePrintService;

            FillTaskName(taskName);

            X_CancellationTokenSource = cancellationTokenSource;

            _autoResetEvent = new AutoResetEvent(false);
        }

        public string TaskName { get; private set; }

        public CancellationTokenSource X_CancellationTokenSource { get; private set; }

        public CommandResult CommandResult { get; private set; }

        /// <summary>
        /// 异步阻塞流程,等待外部事件信号是否阻塞
        /// </summary>
        /// <returns></returns>
        public Task<CommandResult> Take()
        {
            string method = $"[{nameof(TaskToken)}] [{nameof(Take)}]";

            _logService.Info(ServiceCategory.AutoCali, $"Task '{TaskName}' take the token, Keep waiting until it is released.");

            //异步监听是否被用户取消
            _ = MonitorUserCanceledAsync();

            //阻塞流程，直到外界有信号释放阻塞
            _autoResetEvent?.WaitOne();

            _logService.Info(ServiceCategory.AutoCali, $"{method} Task '{TaskName}' get the signal to release.");

            return Task.FromResult(CommandResult);
        }

        /// <summary>
        /// 释放被阻塞的流程
        /// </summary>
        /// <param name="commandResult"></param>
        public void Release(CommandResult commandResult)
        {
            CommandResult = commandResult;

            string errorCodesInfo = string.Join(",", commandResult.ErrorCodes.Codes);
            string errorInfo = (!commandResult.Success()) ? $"ErrorCodes:{errorCodesInfo}" : string.Empty;
            string msg = $"Task '{TaskName}' released the token by the sender '{commandResult.Sender}'" +
                $", with status '{commandResult.Status}', {errorInfo}.";
            _logService.Error(ServiceCategory.AutoCali, msg);

            _autoResetEvent?.Set();
        }

        /// <summary>
        /// 定时刷新用户是否取消
        /// </summary>
        private Task MonitorUserCanceledAsync()
        {
            _ = Task.Factory.StartNew(() =>
            {
                string method = nameof(MonitorUserCanceledAsync);
                var timer = new AutoResetEvent(false);
                var timeSpan = TimeSpan.FromSeconds(1);
                while (true)
                {
                    //_logService.Debug(ServiceCategory.AutoCali, $"[{method}] timer.WaitOne({timeSpan})");
                    timer.WaitOne(timeSpan);

                    if (X_CancellationTokenSource.IsCancellationRequested)
                    {
                        _logService.Debug(ServiceCategory.AutoCali, $"[{method}] Recev the signal 'User Cancelled', so break the loop for waiting user cancelled.");
                        break;
                    }
                }

                CommandResult commandResult = new ()
                {
                    Sender = method,
                    Status = CommandStatus.Cancelled,
                    Description = Constants.MSG_USER_CANCELED
                };
                Release(commandResult);
            });

            return Task.CompletedTask;
        }

        private void FillTaskName(string taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                taskName = DateTime.Now.ToString("yyyyMMdd.HHmmss.fff");
            }
            TaskName = taskName;
        }

        protected readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;

        private AutoResetEvent _autoResetEvent;
    }
}
