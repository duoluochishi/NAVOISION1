using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public abstract class AbstractScanHandler : AbstractHandler
    {
        public AbstractScanHandler(
            ILogService logService,
            IMessagePrintService messagePrintService,
            CancellationTokenSource cancellationTokenSource)
            : base(logService, messagePrintService, cancellationTokenSource)
        {
        }

        #region public interfaces

        /// <summary>
        /// Offline Reconstruction的ID
        /// </summary>
        public string ProcessId { get; private set; }


        #endregion public interfaces

        #region private methods

        protected override async Task<CommandResult> ExecuteCore<T>(T commandParameter) where T : class
        {
            if (commandParameter is not ScanReconParam)
            {
                _loggerUI.PrintLoggerError($"参数错误，期望类型{nameof(ScanReconParam)}");
                return CommandResult.DefaultFailureResult;
            }

            _ScanReconParam = commandParameter as ScanReconParam;

            //更新有效的扫描张数
            UpdateTotalScanCount(_ScanReconParam);

            //检测设备是否可以扫描
            CommandResult commandResult = await CheckScanIsEnabledAsync();

            //创建Offline Reconstruction任务
            if (commandResult?.Success() == true)
            {
                commandResult = await Scan();
            }

            return commandResult;
        }

        protected abstract Task<CommandResult> Scan();

        protected override void RegisterEventCore()
        {
            AcqReconProxy.Instance.ScanReconConnectionChanged += OnAcqReconConnectionChanged;
            //AcqReconProxy.Instance.SystemStatusChanged += OnSystemStatusChanged;

            //AcqReconProxy.Instance.RawImageSaved += OnRawImageSaved;
            //AcqReconProxy.Instance.AcqReconStatusChanged += OnAcqReconStatusChanged;
        }

        protected override void UnregisterEventCore()
        {
            AcqReconProxy.Instance.ScanReconConnectionChanged += OnAcqReconConnectionChanged;
        }

        private void OnAcqReconConnectionChanged(object arg1, ConnectionStatusArgs connectionStatusArgs)
        {
            string method = nameof(OnAcqReconConnectionChanged);

            if (connectionStatusArgs.Connected)
            {
                _loggerUI.PrintLoggerInfo(Constants.MSG_SCAN_RECON_ENGINE_CONNECTED);
            }
            else
            {
                _loggerUI.PrintLoggerError(Constants.MSG_SCAN_RECON_ENGINE_DISCONNECTED);

                CommandResult commandResult = new()
                {
                    Sender = method,
                    Status = CommandStatus.Failure,
                    Description = Constants.MSG_SCAN_RECON_ENGINE_DISCONNECTED
                };
                commandResult.AddErrorCode(ErrorCodeDefines.ERROR_CODE_MCS_OFFLINE_RECON_DISCONNECTED);

                _taskToken?.Release(commandResult);
            }

            //clear
            //_lastOfflineReconStatusArgs = null;

            //AcqReconConnected = arg2.Connected;

            //string connectionInfo = GetModuleConnectionInfo(MSG_Module_AcqRecon, AcqReconConnected);
            //PrintConnectionInfo(connectionInfo, AcqReconConnected);

            //if (!AcqReconConnected)
            //{
            //    PrintMessage($"System Stop due to {connectionInfo}", PrintLevel.Error);
            //    StopRefreshCalcProgress();

            //    StopTask(string.Empty/* todo */, null, connectionInfo);
            //}
        }

        private void OnSystemStatusChanged(object arg1, SystemStatusArgs systemStatusArgs)
        {
            this._logger.Debug(ServiceCategory.AutoCali, $"SystemStatus: {_lastSystemStatusArgs} -> {systemStatusArgs.Status}");
            _lastSystemStatusArgs = systemStatusArgs;
        }

        internal void OnRawImageSaved(object arg1, RawImageSavedEventArgs rawImageSavedEventArgs)
        {
            if (rawImageSavedEventArgs?.ScanUID != ProcessId)
            {
                return;
            }

            //实时刷新扫描进度
            var ScannedProgress = rawImageSavedEventArgs.FinishCount;
            if (_lastRawImageSavedEventArgs.IsFinished)
            {
                _taskToken?.Release(new() { Sender = nameof(OnRawImageSaved) });
            }
            else if (ScannedProgress % 50 == 1)/* 每隔50张刷新到日志，避免太频繁 */
            {
                UpdateScanProgressInfo(_lastRawImageSavedEventArgs);
            }

            //MockAcqNormalMode_Receive(rawImageSavedEventArgs);
            _lastRawImageSavedEventArgs = rawImageSavedEventArgs;
        }

        internal void OnAcqReconStatusChanged(object arg1, AcqReconStatusArgs acqReconStatusArgs)
        {
            if (acqReconStatusArgs?.ScanUID != ProcessId)
            {
                return;
            }

            //获取后端返回的生数据路径，并更新到最后一次扫描参数上
            _ScanReconParam.ScanParameter.RawDataDirectory = acqReconStatusArgs.RawDataPath;

            this._logger.Debug(ServiceCategory.AutoCali, $"AcqReconStatus Changed: {_lastAcqReconStatusArgs?.Status} -> {_lastAcqReconStatusArgs?.Status}");
            if (_lastAcqReconStatusArgs?.Status == _lastAcqReconStatusArgs?.Status)
            {
                this._logger.Warn(ServiceCategory.AutoCali, "AcqReconStatus NOT CHANGED!");
                return;
            }

            if (acqReconStatusArgs.Status <= AcqReconStatus.Reconning)
            {
                return;
            }

            //结束了，
            if (acqReconStatusArgs.Status == AcqReconStatus.Finished)
            {
                string rawDataDirectory = acqReconStatusArgs.RawDataPath;
                this._logger.Debug(ServiceCategory.AutoCali, $"Got the rawDataDirectory:{rawDataDirectory}");
                this._loggerUI.PrintLoggerInfo($"Completed to ScanRecon");

                this._taskToken?.Release(new() { Sender = nameof(OnAcqReconStatusChanged) });
            }
            else
            {
                this._loggerUI.PrintLoggerError($"Failed to ScanRecon ");

                this._taskToken?.Release(new()
                {
                    Sender = nameof(OnAcqReconStatusChanged),
                    Status = ToCommandStatus(acqReconStatusArgs),
                    ErrorCodes = acqReconStatusArgs.Errors
                });
            }

            _lastAcqReconStatusArgs = acqReconStatusArgs;
        }

        private void UpdateScanProgressInfo(RawImageSavedEventArgs rawImageSavedEventArgs)
        {
            string msg = (rawImageSavedEventArgs.IsFinished)
                ? string.Format(Calibration_Lang.Calibration_Scan_Progress_Done_Formatter, rawImageSavedEventArgs.FinishCount, TotalScanCount)
                : string.Format(Calibration_Lang.Calibration_Scan_Progress_Doing_Formatter, rawImageSavedEventArgs.FinishCount, TotalScanCount);
            this._loggerUI.PrintLoggerInfo(msg);
        }

        private void UpdateTotalScanCount(ScanReconParam scanReconParam)
        {
            uint totalScanCount = scanReconParam.ScanParameter.TotalFrames;
            uint autoDeleteCount = scanReconParam.ScanParameter.AutoDeleteNum;
            TotalScanCount = (int)(totalScanCount - autoDeleteCount);
        }

        private CommandStatus ToCommandStatus(AcqReconStatusArgs? acqReconStatusArgs)
        {
            var commandStatus = acqReconStatusArgs.Status switch
            {
                AcqReconStatus.Cancelled => CommandStatus.Cancelled,
                AcqReconStatus.Finished => CommandStatus.Success,
                AcqReconStatus.Error => CommandStatus.Failure,
            };
            return commandStatus;
        }


        #endregion event handler

        #region private / protected

        #region 检测设备是否可扫描，轮询重试多次（默认15次）

        private static readonly int RetryCount_Max = 15;

        /// <summary>
        /// 检测设备是否可扫描，轮询重试多次（默认5次）
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<CommandResult> CheckScanIsEnabledAsync()
        {
            CommandResult commandResult = new() { Sender = nameof(CheckScanIsEnabledAsync) };

            this._loggerUI.PrintLoggerInfo($"Check whether the device is scannable ...");

            int loopCount = 1;
            while (true)
            {
                if (this._cancellationTokenSource.IsCancellationRequested)
                {
                    //用户主动取消了任务
                    this._loggerUI.PrintLoggerInfo($"User cancelled the task.");

                    commandResult.ErrorCodes = new(ErrorCodeDefines.ERROR_CODE_USER_CANCELED);

                    //this._taskToken?.Release(commandResult);
                    //taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                    break;
                }

                //当前设备实时状态curRealTimeStatus，由代理服务提供的事件OnRealTimeChanged，不断吐发消息
                if (IsScanEnabled())
                {
                    //确认设备为可扫描状态，跳出检测轮询
                    this._loggerUI.PrintLoggerInfo("The device has been detected to be scannable.");

                    break;
                }

                if (loopCount >= RetryCount_Max)
                {
                    //重试多次，依然没有等到可扫描状态，重设任务状态为被动取消
                    this._loggerUI.PrintLoggerInfo($"System passively cancelled the task because the device cannot be scannable and the retry count is exceeded [{RetryCount_Max}]!");
                    //taskViewModel.CaliTaskState = CaliTaskState.Canceled;

                    commandResult.ErrorCodes = new(ErrorCodeDefines.ERROR_CODE_DEVICE_DISCONNECTED);
                    //this._taskToken?.Release(commandResult);

                    break;
                }
                this._loggerUI.PrintLoggerInfo($"The device is not scannable, will retry for the {++loopCount}th time in 1 second.");
                await Task.Delay(1000);//间隔1000ms
            }

            return commandResult;
        }

        /// <summary>
        /// 检测设备是否可扫描，通过主动调用服务代理获取最新的实时状态。
        /// 如果使用被动接受服务代理的实时状态消息，有可能消息丢失没有收到导致始终无法闭环。
        /// </summary>
        /// <returns></returns>
        private bool IsScanEnabled()
        {
            var systemStatus = AcqReconProxy.Instance.CurrentDeviceSystem.SystemStatus;
            bool isScanEnabled = (systemStatus == SystemStatus.Standby || systemStatus == SystemStatus.Validated);

            string msg = string.Format("Device scan is {1} due to SystemStatus is {0}", systemStatus,
                (isScanEnabled ? "available" : "NOT available"));
            if (isScanEnabled)
            {
                this._logger.Debug(ServiceCategory.AutoCali, msg);
            }
            else
            {
                this._loggerUI.PrintLoggerInfo(msg);
            }

            return isScanEnabled;
        }

        /// <summary>
        /// <summary>
        /// 倒计时请求开始扫描，给用户反悔的机会，用户可随时终止
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="taskName"></param>
        /// <param name="countDownNum">倒计时个数</param>
        /// <returns></returns>
        private async Task CountDownToRequestServiceAsync(CancellationTokenSource cancellationToken, string taskName, Action cancelCallback, int countDownNum = 5)
        {
            int loopCount = countDownNum;
            while (loopCount > 0)
            {
                this._loggerUI.PrintLoggerInfo($"{taskName} will begin in {loopCount} seconds");

                if (cancellationToken.IsCancellationRequested)
                {
                    //用户主动取消了任务
                    this._loggerUI.PrintLoggerInfo($"User cancelled to {taskName}.");
                    //taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                    cancelCallback?.Invoke();
                    break;
                }

                await Task.Delay(1000);
                loopCount--;
            }
        }

        #endregion 检测设备是否可扫描，轮询重试多次（默认15次）

        /// <summary>
        /// <summary>
        /// 倒计时请求开始扫描，给用户反悔的计划，用户可随时终止
        /// </summary>
        /// <param name="taskViewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="taskName"></param>
        /// <param name="countDownNum">倒计时个数</param>
        /// <returns></returns>
        protected async Task<CommandResult> CountDownToRequestServiceAsync(string taskName, Action cancelCallback, int countDownNum = 10)
        {
            CommandResult commandResult = new() { Sender = nameof(CountDownToRequestServiceAsync) };

            int loopCount = countDownNum;
            while (loopCount > 0)
            {
                this._loggerUI.PrintLoggerInfo($"{taskName} will begin in {loopCount} seconds");

                if (this._cancellationTokenSource.IsCancellationRequested)
                {
                    //用户主动取消了任务
                    this._loggerUI.PrintLoggerWarn($"User cancelled to {taskName}.");
                    //taskViewModel.CaliTaskState = CaliTaskState.Canceled;
                    commandResult.ErrorCodes = new(ErrorCodeDefines.ERROR_CODE_USER_CANCELED);

                    cancelCallback?.Invoke();
                    break;
                }

                await Task.Delay(1000);
                loopCount--;
            }

            return commandResult;
        }

        /// <summary>
        /// 微秒 转换成 秒
        /// </summary>
        /// <param name="microSecond"></param>
        /// <returns></returns>
        protected double ConvertMicroToSecond(uint microSecond)
        {
            return microSecond / 1000.0 / 1000;
        }

        #endregion

        #region private fields

        //protected readonly ILogService _logService;
        //protected readonly IMessagePrintService _messagePrintService;

        //private bool _initialized;

        //private CancellationTokenSource _cancellationTokenSource;
        //private TaskToken _taskToken;

        protected ScanReconParam _ScanReconParam;

        //private OfflineReconStatusArgs _lastOfflineReconStatusArgs;
        private SystemStatusArgs _lastSystemStatusArgs;
        private RawImageSavedEventArgs _lastRawImageSavedEventArgs;
        private AcqReconStatusArgs _lastAcqReconStatusArgs;

        private int TotalScanCount;

        #endregion private fields
    }
}
