using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class OfflineReconHandler
    {
        public OfflineReconHandler(
            ILogService logService,
            IMessagePrintService messagePrintService,
            CancellationTokenSource cancellationTokenSource)
        {
            _logService = logService;
            _messagePrintService = messagePrintService;

            _cancellationTokenSource = cancellationTokenSource;
        }

        #region public interfaces

        /// <summary>
        /// Offline Reconstruction的ID
        /// </summary>
        public string ReconID { get; private set; }

        public void RegisterOfflineMachineProxy()
        {
            _logService.Debug(ServiceCategory.AutoCali, $"[{nameof(RegisterOfflineMachineProxy)}] Entered");

            UnregisterOfflineMachineProxy();


            //OldOfflineReconProxy.Instance.ConnectionStatusChanged += OnOfflineReconConnectionChanged;
            //OldOfflineReconProxy.Instance.TaskStatusChanged += OnOfflineReconTaskStatusChanged;
            //OldOfflineReconProxy.Instance.ImageSaved += OnOfflineReconImageSaved;
            //OldOfflineReconProxy.Instance.ErrorOccured += OnOfflineReconErrorOccured;
        }

        public void UnregisterOfflineMachineProxy()
        {
            _logService.Debug(ServiceCategory.AutoCali, $"[{nameof(UnregisterOfflineMachineProxy)}] Entered");

            //OldOfflineReconProxy.Instance.ConnectionStatusChanged -= OnOfflineReconConnectionChanged;
            //OldOfflineReconProxy.Instance.TaskStatusChanged -= OnOfflineReconTaskStatusChanged;
            //OldOfflineReconProxy.Instance.ImageSaved -= OnOfflineReconImageSaved;
            //OldOfflineReconProxy.Instance.ErrorOccured -= OnOfflineReconErrorOccured;
        }

        public async Task<CommandResult> StartTask(ScanReconParam scanReconParam)
        {
            //检测Offline Reconstruction服务是否在线
            CommandResult commandResult = await CheckOfflineReconConnection();

            //创建Offline Reconstruction任务
            if (commandResult?.Success() == true)
            {
                commandResult = await CreateTaskCore(scanReconParam);
            }

            //执行Offline Reconstruction任务
            if (commandResult?.Success() == true)
            {
                commandResult = await StartTaskCore();//异步等待
            }

            //不再监听异步事件
            UnregisterOfflineMachineProxy();

            return commandResult;
        }

        #endregion public interfaces

        #region private methods

        private async Task TryInitializeProxy()
        {
            if (!_initialized)
            {
                RegisterOfflineMachineProxy();

                await Task.Delay(1000);
            }

            _initialized = true;
        }

        /// <summary>
        /// 检查Offline Reconstruction连接状态
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <param name="maxRepeatTimes"></param>
        /// <returns></returns>
        private async Task<CommandResult> CheckOfflineReconConnection(int maxRepeatTimes = 5)
        {
            string method = nameof(CheckOfflineReconConnection);
            _logService.Debug(ServiceCategory.AutoCali, $"Beginning to {method}");
            _messagePrintService.PrintLoggerInfo("Checking the connection of offline recon service");

            CommandResult commandResult = new() { Sender = method, Status = CommandStatus.Success };

            int repeateTimes = 0;
            while (repeateTimes++ < maxRepeatTimes)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    commandResult.Status = CommandStatus.Failure;
                    commandResult.ErrorCodes.Codes.Add(ErrorCodeDefines.ERROR_CODE_USER_CANCELED);
                    break;
                }
                //if (OldOfflineReconProxy.Instance.Connected)
                //{
                //    _messagePrintService.PrintLoggerInfo(Constants.MSG_OFFLINERECON_CONNECTED);
                //    commandResult.Status = CommandStatus.Success;
                //    break;
                //}

                await Task.Delay(1000);
                _logService.Debug(ServiceCategory.AutoCali, $"{method}, repeat {repeateTimes} times");
            }

            if (repeateTimes >= maxRepeatTimes)
            {
                _messagePrintService.PrintLoggerInfo(Constants.MSG_OFFLINERECON_DISCONNECTED);

                commandResult.Status = CommandStatus.Failure;
                commandResult.ErrorCodes.Codes.Add(ErrorCodeDefines.ERROR_CODE_MCS_OFFLINE_RECON_DISCONNECTED);
            }

            _logService.Debug(ServiceCategory.AutoCali, $"Ended to {method}, with result '{commandResult.Status}'");
            return commandResult;
        }

        private async Task<CommandResult> CreateTaskCore(ScanReconParam scanReconParam)
        {
            await TryInitializeProxy();

            ReconID = $"{DateTime.Now.ToString("yyyyMMdd.HHmmss")}";
            scanReconParam.ReconSeriesParams[0].ReconID = ReconID;
            _logService.Info(ServiceCategory.AutoCali, $"Changed ReconID '{ReconID}' of scanReconParam.");

            string commandInfo = ToCommandInfo("CreateTask");

            //Start task
            string msg = $"{commandInfo} Requesting with ReconID '{ReconID}'.";
            _messagePrintService.PrintLoggerInfo(msg);

            //手工选择生数据的是本地路径，需要修改为网络路径
            TryResetRawDataDirectory(scanReconParam);

            CommandResult commandResult = OfflineMachineTaskProxy.Instance.CreateOfflineReconTask(scanReconParam, TaskPriority.High);
            if (!commandResult.Success())
            {
                var errorCodes = commandResult.ErrorCodes?.Codes;
                _logService.Error(ServiceCategory.AutoCali, $"Recev ErrorCodes: {JsonSerializeHelper.ToJson(errorCodes)}");

                var distinctErrorCodes = errorCodes?.Distinct()?.ToList();
                msg = $"{commandInfo} Failed to Request！ErrorCodes: {JsonSerializeHelper.ToJson(distinctErrorCodes)}";
                _messagePrintService.PrintLoggerError(msg);

                commandResult.ErrorCodes.Codes = distinctErrorCodes;
            }
            else
            {
                msg = $"{commandInfo} Request Successfully！";
                _messagePrintService.PrintLoggerInfo(msg);

                commandResult = MakeCommandResult(nameof(CreateTaskCore), CommandStatus.Success, null);
                string commandStatusInfo = (commandResult).Description;
                _messagePrintService.PrintLoggerInfo($"{commandInfo} {commandStatusInfo}，ReconID={ReconID}.");
            }

            return commandResult;
        }

        /// <summary>
        /// 主控机IP，离线重建需要使用它将本地生数据路径转换成网络路径
        /// 【TODO】重构成配置项
        /// </summary>
        private static readonly string MainCommandServerIP = "78.86.100.2";
        private void TryResetRawDataDirectory(ScanReconParam scanReconParam)
        {
            string logHead = $"[{nameof(OfflineReconHandler)}] [{nameof(TryResetRawDataDirectory)}]";
            string rawDataDirectory = scanReconParam.ScanParameter.RawDataDirectory;
            if (string.IsNullOrEmpty(rawDataDirectory))
            {
                return;
            }

            _logService.Debug(ServiceCategory.AutoCali, $"{logHead} Get ScanParameter.RawDataDirectory '{rawDataDirectory}'");

            string networkPathStart = @"\\";
            if (rawDataDirectory.StartsWith(networkPathStart))//已经是网络路径了
            {
                _logService.Debug(ServiceCategory.AutoCali, $"{logHead} rawDataDirectory.StartsWith(networkPathStart)");
                return;
            }

            var ip = MainCommandServerIP;// RuntimeConfig.MRSServices.OfflineCommandServer.IP;
            string networkRootPath = $"{networkPathStart}{ip}";
            string networkRawDataDirectory = Path.Combine(networkRootPath, rawDataDirectory.Replace(":", ""));//本地路径拼成网络路径
            scanReconParam.ScanParameter.RawDataDirectory = networkRawDataDirectory;
            _logService.Debug(ServiceCategory.AutoCali, $"{logHead} Reset ScanParameter.RawDataDirectory '{networkRawDataDirectory}'");
        }

        private string ToCommandInfo(string commandName)
        {
            return $"{Constants.MSG_MODULE_OFFLINERECON} Command '{commandName}'";
        }

        private async Task<CommandResult> StartTaskCore()
        {
            string commandInfo = ToCommandInfo("StartTask");

            //Start task
            string msg = $"{commandInfo} Requesting.";
            _messagePrintService.PrintLoggerInfo(msg);
            CommandResult commandResult = OfflineMachineTaskProxy.Instance.StartTask(ReconID);

            if (!commandResult.Success())
            {
                var errorCodes = commandResult.ErrorCodes?.Codes;
                _logService.Error(ServiceCategory.AutoCali, $"Recev ErrorCodes: {JsonSerializeHelper.ToJson(errorCodes)}");

                var distinctErrorCodes = errorCodes?.Distinct()?.ToList();
                msg = $"{commandInfo} Failed to Request！ErrorCodes: {JsonSerializeHelper.ToJson(distinctErrorCodes)}";
                _messagePrintService.PrintLoggerError(msg);

                commandResult.ErrorCodes.Codes = distinctErrorCodes;
            }
            else
            {
                msg = $"{commandInfo} Request Successfully！";
                _messagePrintService.PrintLoggerInfo(msg);


                msg = $"{commandInfo} Awaiting Asynchronous Result.";
                _messagePrintService.PrintLoggerInfo(msg);

                //await task end
                string taskName = $"OfflineRecon#{ReconID}";
                _taskToken = new TaskToken(_logService, _messagePrintService, taskName, _cancellationTokenSource);

                commandResult = await _taskToken.Take();
                msg = commandResult.Description;
                _messagePrintService.PrintLoggerInfo($"{commandInfo} {msg}.");
            }

            return commandResult;
        }

        #endregion  private methods

        #region event handler
        private void OnOfflineReconConnectionChanged(OfflineConnectionStatusArgs offlineConnectionStatus)
        {
            string method = nameof(OnOfflineReconConnectionChanged);

            if (offlineConnectionStatus.Connected)
            {
                _messagePrintService.PrintLoggerInfo(Constants.MSG_OFFLINERECON_CONNECTED);
            }
            else
            {
                _messagePrintService.PrintLoggerError(Constants.MSG_OFFLINERECON_DISCONNECTED);

                CommandResult commandResult = new()
                {
                    Sender = method,
                    Status = CommandStatus.Failure,
                    Description = Constants.MSG_OFFLINERECON_DISCONNECTED
                };
                commandResult.AddErrorCode(ErrorCodeDefines.ERROR_CODE_MCS_OFFLINE_RECON_DISCONNECTED);

                _taskToken?.Release(commandResult);
            }

            //clear
            _lastOfflineReconStatusArgs = null;
        }

        private void OnOfflineReconErrorOccured(ErrorCodes errorCodes)
        {
            string method = nameof(OnOfflineReconErrorOccured);

            string errorCodesInfo = ToErrorCodesInfo(errorCodes);
            _logService.Error(ServiceCategory.AutoCali, $"Recev ErrorCodes:{errorCodesInfo}.");

            var commandResult = MakeCommandResult(method, CommandStatus.Failure, errorCodes);
            _taskToken?.Release(commandResult);
        }

        private CommandResult MakeCommandResult(string sender, CommandStatus commandStatus, ErrorCodes errorCodes)
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

        private string ToCommandStatusInfo(CommandStatus commandStatus, ErrorCodes errorCodes)
        {
            string description = commandStatus.ToString();

            try
            {
                description = commandStatus switch
                {
                    CommandStatus.Cancelled => "Cancelled",
                    CommandStatus.Success => "Succeeded",
                    CommandStatus.Failure => $"Failed with error '{ToErrorCodesInfo(errorCodes)}'",
                };
            }
            catch (Exception ex)
            {
                _logService.Error(ServiceCategory.AutoCali, $"{nameof(ToCommandStatusInfo)} occur an exception:{ex}");
            }
            return description;
        }

        private string ToErrorCodesInfo(ErrorCodes errorCodes)
        {
            string errofInfo = string.Join(',', errorCodes.Codes);
            return errofInfo;
        }

        private void OnOfflineReconImageSaved(ImageSavedEventArgs imageSavedEventArgs)
        {
            if (imageSavedEventArgs.ReconID != ReconID)
            {
                return;
            }

            string finishedInfo = imageSavedEventArgs.IsFinished ? "DONE" : "DOING";
            int totalCount = imageSavedEventArgs.IsFinished ? imageSavedEventArgs.FinishCount : imageSavedEventArgs.TotalCount;
            _messagePrintService.PrintLoggerInfo($"OfflineRecon Progress: {imageSavedEventArgs.FinishCount} / {totalCount}, {finishedInfo}.");
            if (imageSavedEventArgs.FinishCount == 1 || imageSavedEventArgs.IsFinished)
            {
                _messagePrintService.PrintLoggerInfo($"OfflineRecon Image Saved: {imageSavedEventArgs.Directory}");
            }
        }

        private void OnOfflineReconTaskStatusChanged(OfflineReconStatusArgs offlineReconStatus)
        {
            if (offlineReconStatus.ReconID != ReconID)
            {
                return;
            }

            _messagePrintService.PrintLoggerInfo($"OfflineRecon Status Changed: {_lastOfflineReconStatusArgs?.Status} -> {offlineReconStatus.Status}.");
            _lastOfflineReconStatusArgs = offlineReconStatus;

            if (_lastOfflineReconStatusArgs?.Status > OfflineReconStatus.Reconing)
            {
                var commandStatus = ToCommandStatus(offlineReconStatus);
                var commandResult = MakeCommandResult(nameof(OnOfflineReconTaskStatusChanged), commandStatus, offlineReconStatus.Errors);
                _taskToken?.Release(commandResult);
            }
        }

        private CommandStatus ToCommandStatus(OfflineReconStatusArgs? offlineReconStatusArgs)
        {
            var commandStatus = offlineReconStatusArgs.Status switch
            {
                OfflineReconStatus.Cancelled => CommandStatus.Cancelled,
                OfflineReconStatus.Finished => CommandStatus.Success,
                OfflineReconStatus.Error => CommandStatus.Failure,
            };
            return commandStatus;
        }

        #endregion event handler

        #region private fields

        protected readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;

        private bool _initialized;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskToken _taskToken;

        private OfflineReconStatusArgs _lastOfflineReconStatusArgs;

        #endregion private fields
    }
}
