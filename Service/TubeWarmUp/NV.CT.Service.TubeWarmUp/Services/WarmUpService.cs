using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Models;
using NV.CT.Service.TubeWarmUp.Services.Adapter;
using System.Threading;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Resources;

namespace NV.CT.Service.TubeWarmUp.Services
{
    public class TaskSleepArgs : EventArgs
    {
        public TaskSleepArgs(int interval)
        {
            this.RestInterval = interval;
        }

        //单位s
        public int RestInterval { get; private set; }
    }

    public class WarmUpService
    {
        private class ScanFinishHelper
        {
            public event Action<string> ScanNormalFinished;

            public event Action<(string scanUID, string errorCode)> ScanErrorFinished;

            public event Action<string> ScanEmergencyFinished;

            public string ScanUID { get; set; }

            private bool normalScanStopped;

            public bool NormalScanStopped
            {
                get { return normalScanStopped; }
                set
                {
                    if (normalScanStopped != value)
                    {
                        normalScanStopped = value;
                        RaiseScanNormalFinished();
                    }
                }
            }

            private bool errorScanStopped;

            public bool ErrorScanStopped
            {
                get { return errorScanStopped; }
                set
                {
                    if (errorScanStopped != value)
                    {
                        errorScanStopped = value;
                        RaiseScanErrorFinished();
                    }
                }
            }

            public string[] ErrorCodes { get; set; }

            private bool acqReconStatusFinished;

            public bool AcqReconStatusFinished
            {
                get { return acqReconStatusFinished; }
                set
                {
                    if (acqReconStatusFinished != value)
                    {
                        acqReconStatusFinished = value;
                        RaiseScanNormalFinished();
                        RaiseScanErrorFinished();
                        RaiseScanEmergencyrFinished();
                    }
                }
            }

            //急停后需要返回Standby,才算结束
            private bool emergencyScanStopped;

            public bool EmergencyScanStopped
            {
                get { return emergencyScanStopped; }
                set
                {
                    if (emergencyScanStopped != value)
                    {
                        emergencyScanStopped = value;
                        RaiseScanEmergencyrFinished();
                    }
                }
            }

            private bool standby;

            public bool Standby
            {
                get { return standby; }
                set
                {
                    if (standby != value)
                    {
                        standby = value;
                        RaiseScanEmergencyrFinished();
                    }
                }
            }

            private void RaiseScanNormalFinished()
            {
                if (NormalScanStopped /*&& AcqReconStatusFinished*/)
                {
                    ScanNormalFinished?.Invoke(ScanUID);
                }
            }

            private void RaiseScanErrorFinished()
            {
                if (ErrorScanStopped /*&& AcqReconStatusFinished*/)
                {
                    ScanErrorFinished?.Invoke((ScanUID, ErrorCodes.FirstOrDefault()));
                }
            }

            private void RaiseScanEmergencyrFinished()
            {
                if (EmergencyScanStopped /*&& AcqReconStatusFinished*/ && standby)
                {
                    ScanEmergencyFinished?.Invoke((ScanUID));
                }
            }

            /// <summary>
            /// 扫描完成的标志时拿到NormalScanStopped状态和AcqReconStatusFinished状态
            /// </summary>
            public bool ScanFinished => (NormalScanStopped && AcqReconStatusFinished) ||
                (EmergencyScanStopped && Standby && AcqReconStatusFinished);

            public void Clear()
            {
                ScanUID = string.Empty;
                AcqReconStatusFinished = false;
                NormalScanStopped = false;
                EmergencyScanStopped = false;
                ErrorScanStopped = false;
                Standby = false;
            }
        }

        private const int MILLISECOND = 1000;

        public event EventHandler<CycleStatusArgs> CycleStatusChanged;

        public event EventHandler<SystemStatusArgs> SystemStatusChanged;

        public event EventHandler<string[]> ErrorOccured;

        public event EventHandler<TubeHeatCapStatus> TubeHeatCapStatusChanged;

        public event EventHandler<WarmUpTask> TaskStarted;

        public event EventHandler<WarmUpTask> TaskFinished;

        public event EventHandler<(WarmUpTask, string)> TaskFailed;

        public event EventHandler WarmUpSuccess;

        public event EventHandler WarmUpFail;

        /// <summary>
        /// 预热任务休息时触发
        /// </summary>
        public event EventHandler<TaskSleepArgs> TaskSleepStarted;

        /// <summary>
        /// 预热任务结束休息时触发
        /// </summary>
        public event EventHandler<TaskSleepArgs> TaskSleepFinished;

        private readonly IWarmUpAdapter _warmUpAdapt;
        private IList<WarmUpTask> _tasks;
        private WarmUpTask _currentTask => this._currentTaskAdapt?.Task;
        private IList<WarmUpTaskAdapter> _tasksAdapter;
        private WarmUpTaskAdapter _currentTaskAdapt;
        private readonly AutoResetEvent _workStartedEvent;
        private readonly AutoResetEvent _taskFinishedEvent;
        private readonly AutoResetEvent _stopTaskEvent;
        private readonly AutoResetEvent _standbyEvent;
        private readonly AutoResetEvent _doorOpenEvent;

        /// <summary>
        /// 硬件状态
        /// </summary>
        public DeviceSystem Device { get; private set; }

        private readonly IDialogService _dialogService;
        private readonly ILogService _logService;
        private readonly IDataService _dataService;
        private readonly IConfigService _configService;

        /// <summary>
        /// 检查热熔范围时较低的阈值
        /// </summary>
        public int ThermalCapLow { get; set; }

        /// <summary>
        /// 检查热熔范围时较高的阈值
        /// </summary>
        public int ThermalCapHigh { get; set; }

        public int PreheatCount { get; set; }

        private ScanFinishHelper _scanFinishHelper;

        public WarmUpService(IWarmUpAdapter warmUpAdapt,
            IDialogService dialogService,
            ILogService logService,
            IDataService dataService,
            IConfigService configService)
        {
            _workStartedEvent = new AutoResetEvent(false);
            _taskFinishedEvent = new AutoResetEvent(false);
            _stopTaskEvent = new AutoResetEvent(false);
            _standbyEvent = new AutoResetEvent(false);
            _doorOpenEvent = new AutoResetEvent(false);
            _warmUpAdapt = warmUpAdapt;
            _dataService = dataService;
            _dialogService = dialogService;
            _logService = logService;
            _configService = configService;
            _scanFinishHelper = new ScanFinishHelper();
            _scanFinishHelper.ScanNormalFinished += OnScanNormalFinished;
            _scanFinishHelper.ScanErrorFinished += OnScanErrorFinished;
            _scanFinishHelper.ScanEmergencyFinished += OnScanEmergencyFinished;
            new Task(DoWork, TaskCreationOptions.LongRunning).Start();
        }

        private void OnScanEmergencyFinished(string scanUID)
        {
            ScanFinished(false);
        }

        private void OnScanErrorFinished((string scanUID, string errorCode) e)
        {
            ScanFinished(false, e.errorCode);
        }

        private void OnScanNormalFinished(string scanUID)
        {
            ScanFinished();
        }

        private TubeHeatCapStatus heatCapStatus;

        public TubeHeatCapStatus HeatCapStatus
        {
            get { return heatCapStatus; }
            set
            {
                if (heatCapStatus != value)
                {
                    heatCapStatus = value;
                    RaiseTubeHeatCapStatusChanged();
                }
            }
        }

        private void OnHeatCapStatusChagned()
        {
            if (heatCapStatus == TubeHeatCapStatus.Normal)
            {
                StopWarmUp();
            }
            RaiseTubeHeatCapStatusChanged();
        }

        private void RaiseTubeHeatCapStatusChanged()
        {
            TubeHeatCapStatusChanged?.Invoke(this, HeatCapStatus);
        }

        private void OnErrorOccured(object? sender, string[] errorcodes)
        {
            ErrorOccured?.Invoke(sender, errorcodes);
        }

        //扫描结束以扫描重建结束为准
        private void OnAcqReconStatusChenged(object? sender, AcqReconStatusArgs e)
        {
            _logService.Info(ServiceCategory.TubeWarmUp, $"receive acq recon status changed {e.Status}");

            if (this._currentTask == null ||
                this._currentTaskAdapt == null)
            {
                return;
            }

            if (e.ScanUID == _currentTaskAdapt.ScanUID &&
                (e.Status == AcqReconStatus.Finished || e.Status == AcqReconStatus.Cancelled))
            {
                if (_scanFinishHelper.ScanUID == e.ScanUID)
                {
                    _scanFinishHelper.AcqReconStatusFinished = true;
                }
            }
        }

        private void OnSystemStatusChanged(object? sender, SystemStatusArgs e)
        {
            _logService.Info(ServiceCategory.TubeWarmUp, $"receive system status {e.Status} scanUid:{e.ScanUID}");
            if (this._currentTask == null ||
                this._currentTaskAdapt == null)
            {
                return;
            }

            if (e.ScanUID != _currentTaskAdapt.ScanUID)
            {
                return;
            }

            if (e.Status == SystemStatus.Standby)
            {
                if (e.ScanUID == _scanFinishHelper.ScanUID)
                {
                    _scanFinishHelper.Standby = true;
                }
                _standbyEvent.Set();
            }

            if (e.Status == SystemStatus.Preparing)
            {
                this._currentTask.Status = Models.TaskStatus.Working;
                this.TaskStarted?.Invoke(this, this._currentTask);
            }
            else if (e.Status == SystemStatus.NormalScanStopped)
            {
                if (e.ScanUID == _scanFinishHelper.ScanUID)
                {
                    _scanFinishHelper.NormalScanStopped = true;
                }
            }
            else if (e.ScanUID == this._currentTaskAdapt.ScanUID &&
                e.Status == SystemStatus.ErrorScanStopped)
            {
                if (e.ScanUID == _scanFinishHelper.ScanUID)
                {
                    _scanFinishHelper.ErrorCodes = e.GetErrorCodes();
                    _scanFinishHelper.ErrorScanStopped = true;
                }
            }
            else if (e.ScanUID == this._currentTaskAdapt.ScanUID &&
                e.Status == SystemStatus.EmergencyStopped)
            {
                if (e.ScanUID == _scanFinishHelper.ScanUID)
                {
                    _scanFinishHelper.EmergencyScanStopped = true;
                }
            }
            else
            {
            }
        }

        private void ScanFinished(bool isSuccess = true, string errorCode = "")
        {
            //if (this._currentTask.CompleteTimes < this._currentTask.ScanTimes)
            //{
            //    this._currentTask.CompleteTimes++;
            //}
            //状态必须是从Working转换
            //防止用户取消预热时点击停止按钮，此时当前任务为Cancelled
            if (/*this._currentTask.CompleteTimes == this._currentTask.ScanTimes &&*/
                this._currentTask.Status == Models.TaskStatus.Working)
            {
                this._currentTask.Status = isSuccess ? Models.TaskStatus.Done : Models.TaskStatus.Error;
            }
            if (isSuccess)
            {
                this.TaskFinished?.Invoke(this, this._currentTask);
            }
            else
            {
                this.TaskFailed?.Invoke(this, (this._currentTask, errorCode));
            }
            this._taskFinishedEvent.Set();
        }

        private void OnCycleStatusChenged(object? sender, CycleStatusArgs e)
        {
            this.Device = e.Device;
            DetectHeatCap();

            this.CycleStatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 进入界面处理事件
        /// </summary>
        public void StartHandelEvent()
        {
            _warmUpAdapt.CycleStatusChenged += OnCycleStatusChenged;
            _warmUpAdapt.SystemStatusChanged += OnSystemStatusChanged;
            _warmUpAdapt.AcqReconStatusChenged += OnAcqReconStatusChenged;
            _warmUpAdapt.ErrorOccured += OnErrorOccured;
        }

        /// <summary>
        /// 卸载界面不处理事件
        /// </summary>
        public void StopHandleEvent()
        {
            _warmUpAdapt.CycleStatusChenged -= OnCycleStatusChenged;
            _warmUpAdapt.SystemStatusChanged -= OnSystemStatusChanged;
            _warmUpAdapt.AcqReconStatusChenged -= OnAcqReconStatusChenged;
            _warmUpAdapt.ErrorOccured -= OnErrorOccured;
        }

        public void OnDoorOpen()
        {
            _doorOpenEvent.Set();
        }

        private void DetectHeatCap()
        {
            try
            {
                ValidateHeatCap();
            }
            catch (Exception e)
            {
                _logService.Error(ServiceCategory.TubeWarmUp, $"DetectHeatCap error", e);
            }
        }

        public bool StartWarmUp(List<WarmUpTask> tasks)
        {
            try
            {
                _logService.Info(ServiceCategory.TubeWarmUp,
                    $"{nameof(WarmUpService)} StartWarmUp start ");
                if (!Check())
                {
                    return false;
                }
                this._tasks = tasks;
                this._tasksAdapter = new List<WarmUpTaskAdapter>(this._tasks.Count());
                for (int i = 0; i < this._tasks.Count(); i++)
                {
                    this._tasksAdapter.Add(new WarmUpTaskAdapter(this._tasks.ElementAt(i)));
                }
                this._workStartedEvent.Set();
                return true;
            }
            finally
            {
                _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} StartWarmUp end ");
            }
        }

        public void StopWarmUp()
        {
            try
            {
                _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} StopWarmUp start ");
                StopWarmupCore();
            }
            finally
            {
                _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} StopWarmUp end ");
            }
        }

        private void StopWarmupCore()
        {
            if (this._currentTask != null)
            {
                lock (_currentTask.Locker)
                {
                    if (_currentTask.Status == Models.TaskStatus.Sleep)
                    {
                        this._currentTask.Status = Models.TaskStatus.Cancelled;
                        this._stopTaskEvent.Set();
                        _logService.Info(ServiceCategory.TubeWarmUp, "WarmUpService StopWarmUp set stopTaskEvent");
                    }
                    else if (_currentTask.Status == Models.TaskStatus.Working)
                    {
                        this._currentTask.Status = Models.TaskStatus.Cancelled;
                        //停止预热后不用更新等待信号，会在实时状态中处理
                        this._warmUpAdapt.StopWarmUp();
                        _logService.Info(ServiceCategory.TubeWarmUp, "WarmUpService StopWarmUp invoke stop warmup");
                    }
                }
            }
        }

        private void DoWork()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            while (true)
            {
                _logService.Info(ServiceCategory.TubeWarmUp, "WarmUpService wait to start warmup");
                this._workStartedEvent.WaitOne();
                _logService.Info(ServiceCategory.TubeWarmUp, "WarmUpService Start warmup");
                try
                {
                    for (int i = 0; i < PreheatCount; i++)
                    {
                        //配置文件任务数量内使用对应的参数，之外的使用最后一次
                        if (i < _tasksAdapter.Count())
                        {
                            this._currentTaskAdapt = this._tasksAdapter.ElementAt(i);
                        }
                        //for (int j = 1; j <= this._currentTask.ScanTimes; j++)
                        {
                            _scanFinishHelper.Clear();
                            //休息
                            if (i != 0 /*|| j != 1*/)
                            {
                                lock (_currentTask.Locker)
                                {
                                    //cancel scan
                                    if (IsCurrentTaskCancelledOrError())
                                    {
                                        break;
                                    }
                                    _logService.Info(ServiceCategory.TubeWarmUp, $"WarmUpService sleep {this._currentTask.RestTimeInterval}");
                                    TaskSleepStarted?.Invoke(this,
                                        new TaskSleepArgs(this._currentTask.RestTimeInterval));
                                    _currentTask.Status = Models.TaskStatus.Sleep;
                                }
                                if (WaitSignal(this._currentTask.RestTimeInterval * MILLISECOND,
                                    this._stopTaskEvent, _doorOpenEvent) != null)
                                {
                                    break;
                                }
                                TaskSleepFinished?.Invoke(this,
                                    new TaskSleepArgs(this._currentTask.RestTimeInterval));
                            }

                            //判断是否是处于Standby状态，如果不是就等待
                            if (this.Device.SystemStatus != SystemStatus.Standby)
                            {
                                _standbyEvent.Reset();
                                var standby = _standbyEvent.WaitOne(_configService.GetStandbyTimeout());
                                if (!standby)
                                {
                                    _logService.Error(ServiceCategory.TubeWarmUp, $"WarmUpService currentn device system status: {Device.SystemStatus} not Standby");
                                    _currentTask.Status = Models.TaskStatus.Error;
                                    break;
                                }
                            }
                            //cancel scan
                            if (IsCurrentTaskCancelledOrError())
                            {
                                break;
                            }
                            _logService.Info(ServiceCategory.TubeWarmUp, $"WarmUpService send task param {_currentTask.Id}");
                            var res = this._warmUpAdapt.StartWarmUp(this._currentTaskAdapt,
                               this._dataService.GetScanParam());
                            if (res.Status != CommandStatus.Success)
                            {
                                _currentTask.Status = Models.TaskStatus.Error;
                                _dialogService.ShowError($"start warmup error {_currentTask.Id}, error code:{res.ErrorCodes.Codes.FirstOrDefault()}");
                                _logService.Info(ServiceCategory.TubeWarmUp, $"Start warmup error {_currentTask.Id}, error message:{res.ErrorCodes.Codes.FirstOrDefault()}");
                                break;
                            }
                            _scanFinishHelper.ScanUID = _currentTaskAdapt.ScanUID;
                            _logService.Info(ServiceCategory.TubeWarmUp, $"WarmUpService wait task finish {_currentTask.Id}");

                            _taskFinishedEvent.Reset();
                            this._taskFinishedEvent.WaitOne();
                            _logService.Info(ServiceCategory.TubeWarmUp, $"WarmUpService task finish {_currentTask.Id}");
                            //if (IsCurrentTaskCancelledOrError())
                            //{
                            //    break;
                            //}
                        }
                        //用户取消或者预热成功，需要退出
                        if (IsCurrentTaskCancelledOrError() ||
                            IsHeatCapAvailable())
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error(ServiceCategory.TubeWarmUp, $"{nameof(DoWork)} error", ex);
                }
                _logService.Info(ServiceCategory.TubeWarmUp, "WarmUpService task all finish");
                if (IsHeatCapAvailable())
                {
                    this.WarmUpSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.WarmUpFail?.Invoke(this, EventArgs.Empty);
                }
                ResetTask();
            }
        }

        private void ResetTask()
        {
            foreach (var task in this._tasks)
            {
                task.Status = Models.TaskStatus.Standby;
                task.CompleteTimes = 0;
            }
        }

        private bool Check()
        {
            if (this.Device == null)
            {
                this._dialogService.ShowInfo("Service is not ready,please warm up after checking it");
                _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} Device cannot be null, check failed");
                return false;
            }
            //连接状态ok
            if (!_warmUpAdapt.Connected)
            {
                _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} Device disconnect, check failed");
                this._dialogService.ShowInfo("Service is not ready, please warm up after checking it");
                return false;
            }

            if (!this.Device.DoorClosed)
            {
                _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} door is opened, check failed, door closed:{Device.DoorClosed}");
                this._dialogService.ShowInfo("The door is opened, please close it");
                return false;
            }

            var res = this.Device.CTBox.Status == FacadeProxy.Common.Enums.PartStatus.Normal;

            if (!res)
            {
                _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} CTBox is not ready {this.Device.CTBox.Status}, check failed");
                this._dialogService.ShowInfo("CTBox is not ready,please warm up after checking it");
                return false;
            }

            if (this.Device.SystemStatus >= SystemStatus.Preparing &&
                this.Device.SystemStatus <= SystemStatus.NormalScanStopped)
            {
                res &= false;
                _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} system is not  standby or validated {Device.SystemStatus}, check failed");
                this._dialogService.ShowInfo("One scan is doing, please warm up after it finishes.");
            }
            else if (this.Device.SystemStatus == SystemStatus.ErrorScanStopped ||
                this.Device.SystemStatus == SystemStatus.EmergencyStopped)
            {
                res &= false;
                _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpService)} system is not standby or validated {Device.SystemStatus}, check failed");
                this._dialogService.ShowInfo($"The system status is {this.Device.SystemStatus.ToString()}, please handle it.");
            }

            return res;
        }

        private bool IsCurrentTaskCancelledOrError()
        {
            return this._currentTask != null &&
                        (this._currentTask.Status == Models.TaskStatus.Cancelled ||
                        this._currentTask.Status == Models.TaskStatus.Error);
        }

        private void ValidateHeatCap()
        {
            for (int i = 0; i < this.Device.XRaySources.Count(); i++)
            {
                var tube = Device.XRaySources[i];
                if (tube.XRaySourceHeatCap < ThermalCapLow)
                {
                    HeatCapStatus = TubeHeatCapStatus.Low;
                    return;
                }
                if (tube.XRaySourceHeatCap > ThermalCapHigh)
                {
                    HeatCapStatus = TubeHeatCapStatus.High;
                    return;
                }
            }
            HeatCapStatus = TubeHeatCapStatus.Normal;
        }

        public bool IsHeatCapAvailable() => HeatCapStatus == TubeHeatCapStatus.Normal || HeatCapStatus == TubeHeatCapStatus.AboveNormal;

        public static AutoResetEvent? WaitSignal(int milliSecond, params AutoResetEvent[] waitEvents)
        {
            using (var sleepEvent = new AutoResetEvent(false))
            {
                Task.Run(() =>
                {
                    try
                    {
                        Thread.Sleep(milliSecond);
                        sleepEvent.Set();
                    }
                    catch (Exception)
                    {
                    }
                });
                var events = new WaitHandle[waitEvents.Length + 1];
                events[0] = sleepEvent;
                for (int i = 0; i < waitEvents.Length; i++)
                {
                    events[i + 1] = waitEvents[i];
                }
                var index = WaitHandle.WaitAny(events);
                return index == 0 ? null : waitEvents[index - 1];
            }
        }
    }
}