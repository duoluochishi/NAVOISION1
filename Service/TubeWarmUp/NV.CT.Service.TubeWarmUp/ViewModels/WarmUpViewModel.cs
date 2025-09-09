using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.TubeWarmUp.DependencyInject;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Models;
using NV.CT.Service.TubeWarmUp.Services;
using NV.CT.Service.TubeWarmUp.Services.Adapter;
using NV.CT.Service.TubeWarmUp.Utilities;

namespace NV.CT.Service.TubeWarmUp.ViewModels
{
    public class WarmUpViewModel : ViewModelBase
    {
        private const int _preheatCount = 10;
        private readonly IDataService _dataService;
        private readonly WarmUpService _warmUpService;
        private readonly IDialogService _dialogService;
        private readonly ILogService _logService;
        private readonly IConfigService _configService;

        private WarmUpTaskViewModel warmUpTaskViewModel;

        public WarmUpTaskViewModel WarmUpTaskViewModel
        {
            get { return warmUpTaskViewModel; }
            set { SetProperty(ref warmUpTaskViewModel, value); }
        }

        private WarmUpProgressViewModel progressViewModel;

        public WarmUpProgressViewModel ProgressViewModel
        {
            get { return progressViewModel; }
            set { SetProperty(ref progressViewModel, value); }
        }

        private int thermalCapValue;

        public int ThermalCapValue
        {
            get { return thermalCapValue; }
            set
            {
                if (value >= _warmUpService.ThermalCapHigh)
                {
                    _dialogService.ShowInfo($"Warmup goal should less than {_warmUpService.ThermalCapHigh}");
                    return;
                }
                else if (value <= 0)
                {
                    _dialogService.ShowInfo($"Warmup goal should more than 0");
                    return;
                }
                if (SetProperty(ref thermalCapValue, value))
                {
                    _warmUpService.ThermalCapLow = thermalCapValue;
                }
            }
        }

        private int _warmupMaxCount;
        private int preheatCount;

        //预热最大次数
        public int PreheatCount
        {
            get { return preheatCount; }
            set
            {
                if (value <= 0)
                {
                    _dialogService.ShowInfo($"Warmup count should more than 0");
                    return;
                }
                else if (value >= _warmupMaxCount)
                {
                    _dialogService.ShowInfo($"Warmup count should less than {_warmupMaxCount}");
                    return;
                }
                if (SetProperty(ref preheatCount, value))
                {
                    _warmUpService.PreheatCount = preheatCount;
                }
            }
        }

        //预热完成的数量
        private int _completeCount;

        public WarmUpViewModel(IDataService dataService, IDialogService dialogService, ILogService logService, IConfigService configService,
            WarmUpTaskViewModel warmUpTaskViewModel, WarmUpProgressViewModel warmUpProgressViewModel, WarmUpService warmUpService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _logService = logService;
            WarmUpTaskViewModel = warmUpTaskViewModel;
            ProgressViewModel = warmUpProgressViewModel;
            _warmUpService = warmUpService;
            _configService = configService;

            Devices = new DeviceParts();
            Histories = new ObservableCollection<WarmUpHistory>();
            WarmUpTaskViewModel.WarmUpTaskChanged += (object? sender, EventArgs e) =>
            {
                PreheatCount = Math.Max(this.WarmUpTaskViewModel.Tasks.Count(), _preheatCount);
            };
        }

        private void OnTubeHeatCapStatusChanged(object? sender, TubeHeatCapStatus e)
        {
            progressViewModel.HeatCapStatus = e;
        }

        private void OnErrorOccured(object? sender, string[] errorCodes)
        {
            try
            {
                _dialogService.ShowMessage("Error", $"ErrorCode, {string.Join(' ', errorCodes)}");
            }
            catch (Exception ex)
            {
                _logService.Error(ServiceCategory.TubeWarmUp, $"OnReconError", ex);
            }
        }

        private void OnWarmUpTaskChanged(object? sender, EventArgs e)
        {
            CalculateTotalTime();
        }

        private void OnTaskSleep(object? sender, TaskSleepArgs e)
        {
            AddHistory($"task sleep {e.RestInterval} s");
        }

        private void OnWarmUpSuccess(object? sender, EventArgs e)
        {
            try
            {
                this.ProgressViewModel.StopTime();
                this.ProgressViewModel.Progress = 100;
                this._dialogService.ShowInfo(Warmup_Lang.Warmup_WarmupSuccess);
                AddHistory("warm up success");
                this.WarmUpDoing = false;
                ResetProgress();
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.TubeWarmUp, "OnAllTaskFinished error", ex);
            }
        }

        private void OnWarmUpFail(object? sender, EventArgs e)
        {
            try
            {
                this.ProgressViewModel.StopTime();
                this.ProgressViewModel.Progress = 100;
                this._dialogService.ShowInfo(Warmup_Lang.Warmup_WarmupFail);
                AddHistory("warm up fail");
                this.WarmUpDoing = false;
                ResetProgress();
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.TubeWarmUp, "OnWarmUpFail error", ex);
            }
        }

        private void OnTaskFinished(object? sender, WarmUpTask e)
        {
            AddHistory($"task finished");
            _completeCount++;
            try
            {
                CalculateProgress();
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.TubeWarmUp, "OnTaskFinished error", ex);
            }
        }

        private void OnTaskFailed(object? sender, (WarmUpTask task, string errorCode) e)
        {
            AddHistory($"task failed, error code {e.errorCode}");
        }

        private void OnTaskStarted(object? sender, WarmUpTask e)
        {
            AddHistory($"task started {e.KV} kV {e.MA / 1000} mA");
        }

        private void OnCycleStatusChanged(object? sender, CycleStatusArgs e)
        {
            #region 更新球管信息

            try
            {
                for (int i = 0; i < this.Devices.Tubes.Length; i++)
                {
                    var tube = this.Devices.Tubes[i];
                    tube.HeatCap = e.Device.XRaySources[i].XRaySourceHeatCap;
                    tube.OilTemp = e.Device.XRaySources[i].XRaySourceOilTemp / (float)10;
                }
            }
            catch (Exception)
            {
            }

            #endregion 更新球管信息
        }

        public void HandleEvent()
        {
            WarmUpTaskViewModel.WarmUpTaskChanged += OnWarmUpTaskChanged;
            _warmUpService.CycleStatusChanged += OnCycleStatusChanged;
            _warmUpService.ErrorOccured += OnErrorOccured;
            _warmUpService.TaskStarted += OnTaskStarted;
            _warmUpService.TaskFailed += OnTaskFailed;
            _warmUpService.TaskFinished += OnTaskFinished;
            _warmUpService.WarmUpSuccess += OnWarmUpSuccess;
            _warmUpService.WarmUpFail += OnWarmUpFail;
            _warmUpService.TaskSleepStarted += OnTaskSleep;
            progressViewModel.HeatCapStatus = _warmUpService.HeatCapStatus;
            _warmUpService.TubeHeatCapStatusChanged += OnTubeHeatCapStatusChanged;
            DeviceSystem.Instance.DoorClosedStatusChanged += OnDoorClosedStatusChanged;

            _warmUpService.StartHandelEvent();
        }

        private void OnDoorClosedStatusChanged(object? sender, bool e)
        {
            _logService.Warn(ServiceCategory.TubeWarmUp, $"{nameof(OnDoorClosedStatusChanged)} door closed:{e}");
            if (e == false)
            {
                _warmUpService.OnDoorOpen();
            }
        }

        public void StopHandleEvent()
        {
            WarmUpTaskViewModel.WarmUpTaskChanged -= OnWarmUpTaskChanged;
            _warmUpService.CycleStatusChanged -= OnCycleStatusChanged;
            _warmUpService.ErrorOccured -= OnErrorOccured;
            _warmUpService.TaskStarted -= OnTaskStarted;
            _warmUpService.TaskFailed -= OnTaskFailed;
            _warmUpService.TaskFinished -= OnTaskFinished;
            _warmUpService.WarmUpSuccess -= OnWarmUpSuccess;
            _warmUpService.WarmUpFail -= OnWarmUpFail;
            _warmUpService.TaskSleepStarted -= OnTaskSleep;
            _warmUpService.TubeHeatCapStatusChanged -= OnTubeHeatCapStatusChanged;
            DeviceSystem.Instance.DoorClosedStatusChanged -= OnDoorClosedStatusChanged;
            _warmUpService.StopHandleEvent();
        }

        #region 硬件信息

        private DeviceParts devices;

        public DeviceParts Devices
        {
            get { return devices; }
            set { SetProperty(ref devices, value); }
        }

        #endregion 硬件信息

        private bool warmUpDoing;

        public bool WarmUpDoing
        {
            get { return warmUpDoing; }
            set { SetProperty(ref warmUpDoing, value); }
        }

        private ObservableCollection<WarmUpHistory> histories;

        public ObservableCollection<WarmUpHistory> Histories
        {
            get { return histories; }
            set { SetProperty(ref histories, value); }
        }

        private DelegateCommand? exitCommand;

        public DelegateCommand? ExitCommand => exitCommand == null ?
            exitCommand = new DelegateCommand(OnExit) : exitCommand;

        private void OnExit()
        {
        }

        private DelegateCommand? startCommand;

        public DelegateCommand? StartCommand => startCommand == null ?
            startCommand = new DelegateCommand(OnStart) : startCommand;

        private void OnStart()
        {
            try
            {
                if (_warmUpService.IsHeatCapAvailable())
                {
                    _dialogService.ShowInfo("Current thermal capacity is ok, don't need preheat");
                    return;
                }

                LogService.Instance.Info(ServiceCategory.TubeWarmUp, $"WarmUpViewModel OnStart start");
                AddHistory("start warm up");
                if (this._warmUpService.StartWarmUp(this.WarmUpTaskViewModel.Tasks.ToList()))
                {
                    this.WarmUpDoing = true;
                    CalculatePara();
                    CalculateProgress();
                    this.ProgressViewModel.StartTimer();
                }
                else
                {
                    AddHistory("start warm up fail");
                }
            }
            catch (Exception e)
            {
                LogService.Instance.Error(ServiceCategory.TubeWarmUp, $"OnStart error", e);
            }
            finally
            {
                LogService.Instance.Info(ServiceCategory.TubeWarmUp, $"WarmUpViewModel OnStart end");
            }
        }

        private DelegateCommand? stopCommand;

        public DelegateCommand? StopCommand => stopCommand == null ?
            stopCommand = new DelegateCommand(OnStop) : stopCommand;

        private void OnStop()
        {
            try
            {
                LogService.Instance.Info(ServiceCategory.TubeWarmUp, $"WarmUpViewModel OnStop start");
                AddHistory("stop warm up");
                this._warmUpService.StopWarmUp();
                this.ProgressViewModel.StopTime();
                this.WarmUpDoing = false;
            }
            catch (Exception e)
            {
                LogService.Instance.Error(ServiceCategory.TubeWarmUp, $"OnStop error", e);
            }
            finally
            {
                LogService.Instance.Info(ServiceCategory.TubeWarmUp, $"WarmUpViewModel OnStop end");
            }
        }

        private DelegateCommand? loadedCommand;

        public DelegateCommand? LoadedCommand => loadedCommand == null ?
            loadedCommand = new DelegateCommand(OnLoaded) : loadedCommand;

        private void OnLoaded()
        {
            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpViewModel)} Loaded start");
            InitData();
            WarmUpTaskViewModel.Load();
            HandleEvent();

            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpViewModel)} Loaded end");
        }

        private bool _inited;

        private void InitData()
        {
            if (_inited)
            {
                return;
            }
            _inited = true;

            InitWarmupHistories();
            InitPreheatRange();
            _warmupMaxCount = _configService.GetConfig().MaxWarmupCount;
        }

        private void InitWarmupHistories()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            Histories!.Clear();
            Task.Run(() =>
            {
                var his = _dataService.GetWarmUpHistories();
                dispatcher.InvokeAsync(() =>
                {
                    int i = his.Count() - 1;
                    while (i >= 0)
                    {
                        Histories.Add(his.ElementAt(i));
                        i--;
                    }
                });
            });
        }

        private void InitPreheatRange()
        {
            var preheatRange = _configService.GetTubeHeatCaps();
            _warmUpService.ThermalCapLow = preheatRange.min;
            _warmUpService.ThermalCapHigh = preheatRange.max;
            ThermalCapValue = preheatRange.min;
        }

        private DelegateCommand? unloadedCommand;

        public DelegateCommand? UnloadedCommand => unloadedCommand == null ?
            unloadedCommand = new DelegateCommand(OnUnloaded) : unloadedCommand;

        private void OnUnloaded()
        {
            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpViewModel)} Unloaded start");

            StopHandleEvent();

            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(WarmUpViewModel)} Unloaded end");
        }

        private void AddHistory(string messge)
        {
            try
            {
                var history = new WarmUpHistory()
                {
                    DateTime = DateTime.Now,
                    Message = messge
                };
                var his = this._dataService.AddWarmUpHistory(history);
                DispatchUtility.Invoke(() =>
                {
                    this.Histories.Insert(0, his);
                });
            }
            catch (Exception e)
            {
                LogService.Instance.Error(ServiceCategory.TubeWarmUp, $"add history error", e);
            }
        }

        #region calculate progress

        private int _totalTask;
        private float _everyTaskProgress;
        private const int _dectorWidth = 472;//单位0.1mm

        private void CalculateTotalTime()
        {
            var para = this._dataService.GetScanParam();
            var frameTime = para.FrameTime / 1000;//ms
            int cycle = 1;
            //轴扫圈数算法
            if (para.ScanLength > _dectorWidth)
            {
                cycle = (int)Math.Ceiling(((int)para.ScanLength - _dectorWidth) / (double)(int)para.TableFeed) + 1;
            }
            int scanTime = (int)(frameTime * para.FramesPerCycle * cycle) / 1000;
            scanTime += 2;//2s准备时间

            int totalTime = 0;//s
            for (int i = 0; i < this.WarmUpTaskViewModel.Tasks.Count(); i++)
            {
                var task = this.WarmUpTaskViewModel.Tasks[i];
                if (i == 0)
                {
                    totalTime += task.RestTimeInterval * (task.ScanTimes - 1);
                }
                else
                {
                    totalTime += task.RestTimeInterval * task.ScanTimes;
                }
                totalTime += scanTime * task.ScanTimes;
            }
            this.ProgressViewModel.TotalTime = TimeSpan.FromSeconds(totalTime);
        }

        public void CalculatePara()
        {
            this._totalTask = this.WarmUpTaskViewModel.Tasks.Sum(p => p.ScanTimes);
            this._everyTaskProgress = 100 / this._totalTask;
        }

        public void CalculateProgress()
        {
            this.ProgressViewModel.Progress = (int)(_completeCount * 100 / PreheatCount);
        }

        private void ResetProgress()
        {
            this.ProgressViewModel.Progress = 0;
            _completeCount = 0;
        }

        #endregion calculate progress
    }
}