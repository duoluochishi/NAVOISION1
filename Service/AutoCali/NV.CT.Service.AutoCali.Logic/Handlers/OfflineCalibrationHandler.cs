using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.OfflineMachineEnums;
using NV.CT.FacadeProxy.Common.EventArguments.OfflineMachine;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.MPS.Environment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class OfflineCalibrationHandler
    {
        public OfflineCalibrationHandler(
            ILogService logService,
            IMessagePrintService messagePrintService,
            CancellationTokenSource cancellationTokenSource)
        {
            _logService = logService;
            _messagePrintService = messagePrintService;

            _cancellationTokenSource = cancellationTokenSource;

            _stopwatch = new Stopwatch();
        }

        #region public interfaces

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskID { get; private set; }

        public void RegisterOfflineCalibrationProxy()
        {
            _logService.Debug(ServiceCategory.AutoCali, $"[{nameof(RegisterOfflineCalibrationProxy)}] Entered");

            UnregisterOfflineCalibrationProxy();

            OfflineCalibrationTaskProxy.Instance.OfflineMachineConnectionStatusChanged += OnServiceConnectionStatusChanged;
            OfflineCalibrationTaskProxy.Instance.TaskStatusChanged += OnTaskStatusChanged;
            OfflineCalibrationTaskProxy.Instance.TaskProgressChanged += OnTaskProgressChanged;
        }

        public void UnregisterOfflineCalibrationProxy()
        {
            _logService.Debug(ServiceCategory.AutoCali, $"[{nameof(UnregisterOfflineCalibrationProxy)}] Entered");

            OfflineCalibrationTaskProxy.Instance.OfflineMachineConnectionStatusChanged -= OnServiceConnectionStatusChanged;
            OfflineCalibrationTaskProxy.Instance.TaskStatusChanged -= OnTaskStatusChanged;
            OfflineCalibrationTaskProxy.Instance.TaskProgressChanged -= OnTaskProgressChanged;
        }

        public async Task<CommandResult> ExecuteTask(List<ScanReconParam> scanReconParamList)
        {
            //检测Offline Reconstruction服务是否在线
            //CommandResult commandResult = await CheckIsOfflineMachineConnected();
            CommandResult commandResult = null;// await CheckIsOfflineMachineConnected();

            _stopwatch.Reset();
            _stopwatch.Start();

            //创建Offline Reconstruction任务
            //if (commandResult?.Success() == true)
            {
                commandResult = await CreateTaskCore(scanReconParamList);
            }

            //执行Offline Reconstruction任务
            if (commandResult?.Success() == true)
            {
                commandResult = await StartTaskCore(TaskID);//异步等待
            }

            _stopwatch.Stop();
            //不再监听异步事件
            UnregisterOfflineCalibrationProxy();

            return commandResult;
        }

        private static readonly string Msg_Begin_Request_To_Stop = "Request to stop";
        private static readonly string Msg_Begin_Failed_To_Stop = "Request to stop";
        private static readonly string Msg_Begin_Successfully_To_Stop = "Request to stop";

        protected async Task<CommandResult> StopTask()
        {
            //检测Offline Reconstruction服务是否在线
            //CommandResult commandResult = await CheckIsOfflineMachineConnected();
            CommandResult commandResult = CommandResult.DefaultFailureResult;// await CheckIsOfflineMachineConnected();

            string msg = $"Request to stop calibration";
            _messagePrintService.PrintLoggerInfo(msg);

            string requestMethod = nameof(OfflineCalibrationTaskProxy.StopTask);
            _logService.Debug(ServiceCategory.AutoCali, $"Request {requestMethod}, [Input] TaskID:'{TaskID}'");

            var taskExecuteResult = OfflineCalibrationTaskProxy.Instance.StopTask(TaskID);

            if (taskExecuteResult.Success())
            {
                msg = $"Successfully stopped calibration.";
                _messagePrintService.PrintLoggerInfo(msg);
            }
            else
            {
                var errorCodes = taskExecuteResult.ErrorCodes?.Codes;
                _logService.Error(ServiceCategory.AutoCali, $"Recev ErrorCodes: {JsonSerializeHelper.ToJson(errorCodes)}");

                var distinctErrorCodes = errorCodes?.Distinct()?.ToList();
                msg = $"Failed to stop calibration. ErrorCodes: {JsonSerializeHelper.ToJson(distinctErrorCodes)}";
                _messagePrintService.PrintLoggerError(msg);

                commandResult.ErrorCodes.Codes = distinctErrorCodes;
            }

            //不再监听异步事件
            UnregisterOfflineCalibrationProxy();

            return commandResult;
        }

        #endregion public interfaces

        #region Fields

        protected readonly Stopwatch _stopwatch;

        #endregion

        #region private methods

        private async Task TryInitializeProxy()
        {
            if (!_initialized)
            {
                RegisterOfflineCalibrationProxy();

                await Task.Delay(1000);//延迟等待1s，避免服务未注册上
            }

            _initialized = true;
        }

        /// <summary>
        /// 检查Offline Reconstruction连接状态
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <param name="maxRepeatTimes"></param>
        /// <returns></returns>
        private async Task<CommandResult> CheckIsOfflineMachineConnected(int maxRepeatTimes = 5)
        {
            await TryInitializeProxy();
            string method = nameof(CheckIsOfflineMachineConnected);
            _logService.Debug(ServiceCategory.AutoCali, $"Beginning to {method}");
            _messagePrintService.PrintLoggerInfo("Checking the connection of offline machine service");

            CommandResult commandResult = new() { Sender = method, Status = CommandStatus.Failure };

            int repeateTimes = 0;
            while (repeateTimes++ < maxRepeatTimes)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    commandResult.Status = CommandStatus.Failure;
                    commandResult.ErrorCodes.Codes.Add(ErrorCodeDefines.ERROR_CODE_USER_CANCELED);
                    break;
                }
                if (OfflineCalibrationTaskProxy.Instance.IsOfflineMachineConnected)
                {
                    _messagePrintService.PrintLoggerInfo(Constants.MSG_OFFLINE_MACHINE_CONNECTED);
                    commandResult.Status = CommandStatus.Success;
                    break;
                }

                await Task.Delay(1000);
                _logService.Debug(ServiceCategory.AutoCali, $"{method}, repeat {repeateTimes} times");
            }

            if (repeateTimes >= maxRepeatTimes)
            {
                _messagePrintService.PrintLoggerInfo(Constants.MSG_OFFLINE_MACHINE_DISCONNECTED);

                commandResult.Status = CommandStatus.Failure;
                commandResult.ErrorCodes.Codes.Add(ErrorCodeDefines.ERROR_CODE_MCS_OFFLINE_MACHINE_DISCONNECTED);
            }

            _logService.Debug(ServiceCategory.AutoCali, $"Ended to {method}, with result '{commandResult.Status}'");
            return commandResult;
        }

        private async Task<CommandResult> CreateTaskCore(List<ScanReconParam> scanReconParamList)
        {
            await TryInitializeProxy();

            string commandInfo = ToCommandInfo("CreateTask");

            //Start task
            string msg = $"Request {commandInfo}...";
            _messagePrintService.PrintLoggerInfo(msg);

            //手工选择生数据的是本地路径，需要修改为网络路径
            foreach (var scanReconParam in scanReconParamList)
            {
                TryResetRawDataDirectory(scanReconParam);
            }

            CommandResult commandResult = CommandResult.DefaultFailureResult;
            try
            {
                string requestMethod = nameof(OfflineCalibrationTaskProxy.CreateTask);
                _logService.Debug(ServiceCategory.AutoCali, $"Request {requestMethod} ...");

                var taskExecuteResult = OfflineCalibrationTaskProxy.Instance.CreateTask(scanReconParamList, TaskPriority.High);
                TaskID = taskExecuteResult?.TaskID;

                if (!taskExecuteResult.Success())
                {
                    var errorCodes = taskExecuteResult.ErrorCodes?.Codes;
                    _logService.Error(ServiceCategory.AutoCali, $"Recev the task execute result, ErrorCodes: {JsonSerializeHelper.ToJson(errorCodes)}");

                    var distinctErrorCodes = errorCodes?.Distinct()?.ToList();
                    msg = $"{commandInfo} Failed to Request！ErrorCodes: {JsonSerializeHelper.ToJson(distinctErrorCodes)}";
                    _messagePrintService.PrintLoggerError(msg);

                    commandResult.ErrorCodes.Codes = distinctErrorCodes;
                }
                else
                {
                    msg = $"{commandInfo} Request {requestMethod} Successfully, [Output] TaskID:'{TaskID}'";
                    _messagePrintService.PrintLoggerInfo(msg);

                    commandResult = MakeCommandResult(nameof(CreateTaskCore), CommandStatus.Success, null);
                    string commandStatusInfo = commandResult.Description;
                    _messagePrintService.PrintLoggerInfo($"{commandInfo} {commandStatusInfo}，TaskID={TaskID}.");
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ServiceCategory.AutoCali, $"There is unkown error, detail:{ex}");
            }

            return commandResult;
        }

        /// <summary>
        /// 主控机IP，离线重建需要使用它将本地生数据路径转换成网络路径
        /// </summary>
        private string ServiceDataServerIP = null;// RuntimeConfig.MCSServices.IP;// "78.86.100.2" or 127.0.0.1（单机系统）
        private void TryResetRawDataDirectory(ScanReconParam scanReconParam)
        {
            string logHead = $"[{nameof(OfflineCalibrationHandler)}] [{nameof(TryResetRawDataDirectory)}]";
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

            if (string.IsNullOrEmpty(ServiceDataServerIP))
            {
                ServiceDataServerIP = RuntimeConfig.MasterMachineIP;// "78.86.100.2" or 127.0.0.1（单机系统）
                _logService.Debug(ServiceCategory.AutoCali, $"{logHead} 获取生数据所在主机的IP:'{ServiceDataServerIP}'，来自通用配置'MCSServices.IP'");

                if (string.IsNullOrEmpty(ServiceDataServerIP))
                {
                    string serviceDataPath = RuntimeConfig.OfflineRecon.ServiceData.Path;
                    _logService.Debug(ServiceCategory.AutoCali, $"{logHead} 获取生数据所在主机的IP为空，继续适配从离线配置获取，节点'OfflineRecon.ServiceData'：'{serviceDataPath}'");

                    ServiceDataServerIP = UncPathParser.GetIPFromUncPath(serviceDataPath);
                    _logService.Debug(ServiceCategory.AutoCali, $"{logHead} 成功获取生数据所在主机的IP：'{ServiceDataServerIP}'");
                }
            }
            string networkRootPath = $"{networkPathStart}{ServiceDataServerIP}";
            string networkRawDataDirectory = Path.Combine(networkRootPath, rawDataDirectory.Replace(":", ""));//本地路径拼成网络路径
            scanReconParam.ScanParameter.RawDataDirectory = networkRawDataDirectory;
            _logService.Debug(ServiceCategory.AutoCali, $"{logHead} Reset ScanParameter.RawDataDirectory '{networkRawDataDirectory}'");
        }

        private string ToCommandInfo(string commandName)
        {
            return $"{Constants.MSG_MODULE_OFFLINE_MACHINE} Command '{commandName}'";
        }

        private async Task<CommandResult> StartTaskCore(string taskID)
        {
            string commandInfo = ToCommandInfo("StartTask");
            string msg = $"Request {commandInfo} with taskID '{taskID}' ....";
            _messagePrintService.PrintLoggerInfo(msg);
            CommandResult commandResult = CommandResult.DefaultFailureResult;

            string requestMethod = nameof(OfflineCalibrationTaskProxy.StartTask);
            _logService.Debug(ServiceCategory.AutoCali, $"Request {requestMethod}, [Input] TaskID:'{taskID}'");

            var taskExecuteResult = OfflineCalibrationTaskProxy.Instance.StartTask(taskID);

            if (!taskExecuteResult.Success())
            {
                var errorCodes = taskExecuteResult.ErrorCodes?.Codes;
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
                string taskName = $"{requestMethod}#{TaskID}";
                _taskToken = new TaskToken(_logService, _messagePrintService, taskName, _cancellationTokenSource);

                //阻塞，直到获得释放信号（成功或者失败）
                commandResult = await _taskToken.Take();

                //调用停止服务逻辑
                if (commandResult.Status == CommandStatus.Cancelled)
                {
                    await StopTaskCore(this.TaskID);
                }

                msg = commandResult.Description;
                _messagePrintService.PrintLoggerInfo($"{commandInfo} {msg}.");
            }

            return commandResult;
        }

        private async Task<CommandResult> StopTaskCore(string taskID)
        {
            string commandInfo = ToCommandInfo("Stop Task");
            string msg = $"Request {commandInfo} '{taskID}' ....";
            _messagePrintService.PrintLoggerInfo(msg);
            CommandResult commandResult = CommandResult.DefaultFailureResult;

            string requestMethod = nameof(OfflineCalibrationTaskProxy.StopTask);
            _logService.Debug(ServiceCategory.AutoCali, $"Request {requestMethod}, [Input] TaskID:'{taskID}'");

            var taskExecuteResult = OfflineCalibrationTaskProxy.Instance.StopTask(taskID);

            if (!taskExecuteResult.Success())
            {
                var errorCodes = taskExecuteResult.ErrorCodes?.Codes;
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


                //msg = $"{commandInfo} Awaiting Asynchronous Result.";
                //_messagePrintService.PrintLoggerInfo(msg);

                //await task end
                //string taskName = $"{requestMethod}#{TaskID}";
                //_taskToken = new TaskToken(_logService, _messagePrintService, taskName, _cancellationTokenSource);

                //AsyncCommandResult asyncCommandResult = await _taskToken.Take();
                //commandResult = asyncCommandResult;
                //msg = asyncCommandResult.Description;
                //_messagePrintService.PrintLoggerInfo($"{commandInfo} {msg}.");
            }

            return commandResult;
        }

        #endregion  private methods

        #region event handler

        private void OnServiceConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
        {
            string method = nameof(OnServiceConnectionStatusChanged);

            if (e.IsConnected)
            {
                _messagePrintService.PrintLoggerInfo(Constants.MSG_OFFLINE_MACHINE_CONNECTED);
            }
            else
            {
                _messagePrintService.PrintLoggerError(Constants.MSG_OFFLINE_MACHINE_DISCONNECTED);

                CommandResult commandResult = new()
                {
                    Sender = method,
                    Status = CommandStatus.Failure,
                    Description = Constants.MSG_OFFLINE_MACHINE_DISCONNECTED
                };
                commandResult.AddErrorCode(ErrorCodeDefines.ERROR_CODE_MCS_OFFLINE_RECON_DISCONNECTED);

                _taskToken?.Release(commandResult);
            }

            ////clear
            //_lastOfflineReconStatusArgs = null;
        }

        private bool CheckIsCurrentTaskID(string taskID)
        {
            return taskID != TaskID;
        }

        private bool CheckIsFinished(OfflineMachineTaskStatus taskStatus)
        {
            _isFinished = (taskStatus > OfflineMachineTaskStatus.Executing);
            return _isFinished;
        }

        private void OnTaskStatusChanged(object? sender, OfflineMachineTaskStatusChangedEventArgs e)
        {
            string method = nameof(OnTaskStatusChanged);
            if (CheckIsCurrentTaskID(e.TaskID))
            {
                return;
            }

            _messagePrintService.PrintLoggerInfo($"{ServiceName} Status: {_lastTaskStatusChangedEventArgs?.TaskStatus} -> {e.TaskStatus}, {GetElapsedTime()}.");
            _lastTaskStatusChangedEventArgs = e;

            if (CheckIsFinished(e.TaskStatus))
            {
                var commandStatus = ToCommandStatus(e.TaskStatus);
                var commandResult = MakeCommandResult(method, commandStatus, ToErrorCodes(e.ErrorCode));
                _taskToken?.Release(commandResult);
            }
        }

        private ErrorCodes ToErrorCodes(string errorCodeKey)
        {
            return new ErrorCodes() { Codes = new() { errorCodeKey } };
        }
        private CommandStatus ToCommandStatus(OfflineMachineTaskStatus taskStatus)
        {
            var commandStatus = taskStatus switch
            {
                OfflineMachineTaskStatus.Cancelled => CommandStatus.Cancelled,
                OfflineMachineTaskStatus.Finished => CommandStatus.Success,
                OfflineMachineTaskStatus.Error => CommandStatus.Failure,
            };
            return commandStatus;
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

        private void OnTaskProgressChanged(object? sender, OfflineMachineTaskProgressChangedEventArgs e)
        {
            if (e.TaskID != TaskID)
            {
                return;
            }

            int finishCount = e.ProgressStep;
            int totalCount = e.TotalStep;

            double calPercentage = finishCount * 100.0 / totalCount;//转换成百分比
            double percentage = calPercentage;
            if (finishCount >= totalCount)//进度超100%，如果没有完成，优化为无限逼近100%
            {
                if (!_isFinished)
                {
                    percentage = finishCount * 100.0 / (finishCount + 1);//无限逼近100%
                }
                else
                {
                    percentage = 100;
                }
            }

            _logService.Info(ServiceCategory.AutoCali, $"收到进度消息：{finishCount} / {totalCount}，优化后显示百分比：{percentage:F2}%");
            _messagePrintService.PrintLoggerInfo($"Calibration Progress: {percentage:F2}%, {GetElapsedTime()}.");
        }

        private string GetElapsedTime()
        {
            var elapsedTime = _stopwatch.Elapsed;
            return $"Elapsed: {elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";
        }

        #endregion event handler

        #region private fields

        private bool _isFinished;

        protected readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;

        private bool _initialized;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskToken _taskToken;

        private OfflineMachineTaskStatusChangedEventArgs _lastTaskStatusChangedEventArgs;

        private static readonly string ServiceName = "Offline Calibration";

        #endregion private fields

    }
}
