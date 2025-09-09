using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Models;
using NV.CT.Service.TubeWarmUp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.ViewModels
{
    public class MiniWarmUpViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly WarmUpService _warmUpService;
        private readonly IDialogService _dialogService;
        private readonly ILogService _logService;

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

        private bool warmUpDoing;

        public bool WarmUpDoing
        {
            get { return warmUpDoing; }
            set { SetProperty(ref warmUpDoing, value); }
        }

        public MiniWarmUpViewModel(IDataService dataService, IDialogService dialogService, ILogService logService,
            WarmUpTaskViewModel warmUpTaskViewModel, WarmUpProgressViewModel warmUpProgressViewModel, WarmUpService warmUpService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _logService = logService;
            ProgressViewModel = warmUpProgressViewModel;
            WarmUpTaskViewModel = warmUpTaskViewModel;
            _warmUpService = warmUpService;
        }

        private void OnTaskSleep(object? sender, TaskSleepArgs e)
        {
            LogInfo($"task sleep {e.RestInterval}");
        }

        private void OnWarmUpSuccess(object? sender, EventArgs e)
        {
            try
            {
                this.ProgressViewModel.StopTime();
                this.ProgressViewModel.Progress = 100;
                this._dialogService.ShowInfo("Preheat success");
                LogInfo("warm up success");
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
                this._dialogService.ShowError("Preheat fail");
                LogInfo("warm up fail");
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
            LogInfo($"task finished {e.Id}");
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
            LogInfo($"task failed error code {e.errorCode}");
        }

        private void OnTaskStarted(object? sender, WarmUpTask e)
        {
            LogInfo($"task started {e.Id}");
        }

        private void LogInfo(string message)
        {
            _logService.Info(ServiceCategory.TubeWarmUp, message);
        }

        private void OnErrorOccured(object? sender, string[] errorCodes)
        {
            try
            {
                _dialogService.ShowErrorCode(errorCodes.FirstOrDefault(), Array.Empty<string>());
            }
            catch (Exception ex)
            {
                _logService.Error(ServiceCategory.TubeWarmUp, $"OnReconError", ex);
            }
        }

        private void OnCycleStatusChanged(object? sender, CycleStatusArgs e)
        {
        }

        private void OnTubeHeatCapStatusChanged(object? sender, TubeHeatCapStatus e)
        {
            progressViewModel.HeatCapStatus = e;
        }

        public void Loaded()
        {
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
        }

        public void Unloaded()
        {
            _warmUpService.CycleStatusChanged -= OnCycleStatusChanged;
            _warmUpService.ErrorOccured -= OnErrorOccured;
            _warmUpService.TaskStarted -= OnTaskStarted;
            _warmUpService.TaskFailed -= OnTaskFailed;
            _warmUpService.TaskFinished -= OnTaskFinished;
            _warmUpService.WarmUpSuccess -= OnWarmUpSuccess;
            _warmUpService.WarmUpFail -= OnWarmUpFail;
            _warmUpService.TaskSleepStarted -= OnTaskSleep;
            progressViewModel.HeatCapStatus = _warmUpService.HeatCapStatus;
            _warmUpService.TubeHeatCapStatusChanged -= OnTubeHeatCapStatusChanged;
        }

        private DelegateCommand? loadedCommand;

        public DelegateCommand? LoadedCommand => loadedCommand == null ?
            loadedCommand = new DelegateCommand(OnLoaded) : loadedCommand;

        private void OnLoaded()
        {
            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(MiniWarmUpViewModel)} Loaded start");

            WarmUpTaskViewModel.Load();
            Loaded();

            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(MiniWarmUpViewModel)} Loaded end");
        }

        private DelegateCommand? unloadedCommand;

        public DelegateCommand? UnloadedCommand => unloadedCommand == null ?
            unloadedCommand = new DelegateCommand(OnUnloaded) : unloadedCommand;

        private void OnUnloaded()
        {
            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(MiniWarmUpViewModel)} Unloaded start");

            Unloaded();

            _logService.Info(ServiceCategory.TubeWarmUp, $"{nameof(MiniWarmUpViewModel)} Unloaded end");
        }

        private DelegateCommand? startCommand;

        public DelegateCommand? StartCommand => startCommand == null ?
            startCommand = new DelegateCommand(OnStart) : startCommand;

        private void OnStart()
        {
            try
            {
                LogService.Instance.Info(ServiceCategory.TubeWarmUp, $"WarmUpViewModel OnStart start");
                if (this._warmUpService.StartWarmUp(this.WarmUpTaskViewModel.Tasks.ToList()))
                {
                    this.WarmUpDoing = true;
                    CalculatePara();
                    CalculateProgress();
                    this.ProgressViewModel.StartTimer();
                }
                else
                {
                    LogInfo("start warm up fail");
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
                LogInfo($"WarmUpViewModel OnStop start");
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
                LogInfo($"WarmUpViewModel OnStop end");
            }
        }

        #region warmup progress

        private int _totalTask;
        private float _everyTaskProgress;

        public void CalculatePara()
        {
            this._totalTask = this.WarmUpTaskViewModel.Tasks.Sum(p => p.ScanTimes);
            this._everyTaskProgress = 100 / this._totalTask;
        }

        public void CalculateProgress()
        {
            var complateCount = this.WarmUpTaskViewModel.Tasks.Sum(p => p.CompleteTimes);
            this.ProgressViewModel.Progress = (int)(this._everyTaskProgress * complateCount);
        }

        private void ResetProgress()
        {
            this.ProgressViewModel.Progress = 0;
        }

        #endregion warmup progress
    }
}