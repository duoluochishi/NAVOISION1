using CommunityToolkit.Mvvm.Messaging;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.AutoCali.Logic.Messages;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class AttentionHandler : AbstractHandler
    {
        public AttentionHandler(
            ILogService logger,
            IMessagePrintService loggerUI,
            CancellationTokenSource cancellationTokenSource)
            : base(logger, loggerUI, cancellationTokenSource)
        {
        }

        public string MessengerToken { get; set; }

        protected override async Task<CommandResult> ExecuteCore<T>(T commandParameter) where T : class
        {
            if (commandParameter is not Handler)
            {
                _loggerUI.PrintLoggerError($"参数错误，期望类型{nameof(Handler)}");
                return CommandResult.DefaultFailureResult;
            }

            var handlerConfig = commandParameter as Handler;
            var parameter = handlerConfig.Parameters.FirstOrDefault((p) => p.Name == PARAMETER_NAME_ATTENTION);
            var content = parameter?.Value;
            if (parameter == null)
            {
                _loggerUI.PrintLoggerError($"[ConfigError] 没有配置参数{PARAMETER_NAME_ATTENTION}，系统自动使用默认值：{content}");
            }

            _loggerUI.PrintLoggerInfo($"[{MESSAGE_ATTENTION}] {content}");
            DialogService.Instance.ShowInfo(content, MESSAGE_ATTENTION);

            _logger.Debug(ServiceCategory.AutoCali, $"外发消息{nameof(AttentionMessage)}, 值：{content}");
            WeakReferenceMessenger.Default.Send(new AttentionMessage(content), MessengerToken);

            return CommandResult.DefaultSuccessResult;
        }

        protected override void RegisterEventCore()
        {
            throw new NotImplementedException();
        }

        protected override void UnregisterEventCore()
        {
            throw new NotImplementedException();
        }

        private static readonly string PARAMETER_NAME_ATTENTION = "Attention";
        private static readonly string MESSAGE_ATTENTION = "注意事项";
    }
}
