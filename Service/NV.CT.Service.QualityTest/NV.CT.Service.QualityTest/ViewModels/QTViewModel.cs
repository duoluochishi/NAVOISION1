using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.OfflineMachineEnums;
using NV.CT.FacadeProxy.Common.EventArguments.OfflineMachine;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Models;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Common.Wrappers;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Extension;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Services;
using NV.CT.Service.QualityTest.Utilities;
using NV.CT.Service.Universal.PrintMessage.Abstractions;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.QualityTest.ViewModels
{
    public partial class QTViewModel : ObservableObject
    {
        #region Field

        private readonly IMessagePrintService _messagePrintService;
        private readonly IDialogService _dialog;
        private readonly IDataStorageService _dataStorageService;
        private readonly ITableService _tableService;
        private ItemModel? _selectedItem;

        /// <summary>
        /// 配置项
        /// </summary>
        private readonly ConfigModel _config;

        /// <summary>
        /// 摆膜的参数Item
        /// </summary>
        private readonly ItemEntryModel? _integrationPhantomItem;

        /// <summary>
        /// 记录采集和实时重建的状态相关信息
        /// </summary>
        private readonly ScanReconStatusModel _scanReconStatusInfo;

        /// <summary>
        /// 当前正在采集重建的Item
        /// </summary>
        private ItemEntryModel? _curScanReconItem;

        /// <summary>
        /// 等待采集的队列
        /// </summary>
        private readonly Queue<ItemEntryModel> _scanQueue;

        /// <summary>
        /// 离线重建成功的项目，供导出报告使用
        /// </summary>
        private readonly List<ItemModel> _offlineReconSucItems;

        /// <summary>
        /// 所有测试条目的集合，方便筛选
        /// </summary>
        private readonly List<ItemEntryModel> _allParamItems;

        #endregion

        public QTViewModel(
                IMessagePrintService messagePrintService,
                IDialogService dialogService,
                IDataStorageService dataStorageService,
                ITableService tableService,
                ConfigModel config,
                ItemModel[] items)
        {
            _scanReconStatusInfo = new();
            _scanQueue = new Queue<ItemEntryModel>();
            _offlineReconSucItems = new List<ItemModel>();
            _messagePrintService = messagePrintService;
            _dialog = dialogService;
            _dataStorageService = dataStorageService;
            _tableService = tableService;
            _config = config;
            Items = new ObservableCollection<ItemModel>(items);
            _allParamItems = items.Where(i => i.Entries is { Count: > 0 }).SelectMany(i => i.Entries!).ToList();
            _integrationPhantomItem = Items.FirstOrDefault(i => i.IsPhantom)?.Entries?.FirstOrDefault();
        }

        #region Property

        [ObservableProperty]
        private string? _runLog;

        /// <summary>
        /// 是否正在采集重建中
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanButtonStartScan))]
        [NotifyPropertyChangedFor(nameof(CanButtonStopScan))]
        // [NotifyCanExecuteChangedFor(nameof(ButtonStartScanCommand))]
        // [NotifyCanExecuteChangedFor(nameof(ButtonStopScanCommand))]
        private bool _isScanning;

        /// <summary>
        /// 是否正在离线重建中
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanButtonStopOfflineRecon))]
        // [NotifyCanExecuteChangedFor(nameof(ButtonStopOfflineReconCommand))]
        private bool _isInOfflineRecon;

        public ItemModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    Global.CurrentQTType = value?.QTType ?? QTType.None;
                    // ButtonStartScanCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(CanButtonStartScan));
                }
            }
        }

        public ObservableCollection<ItemModel> Items { get; }

        #endregion

        #region Event

        public Action? InvalidateRequerySuggested { get; set; }
        public Func<ItemEntryModel, ResultModel>? BeforeScan { get; set; }
        public Func<ItemEntryModel, ResultModel> SetScanAndReconParam { get; set; } = null!;
        public Func<ItemEntryModel, Task<ResultModel>> AfterRecon { get; set; } = null!;

        #endregion

        #region Command

        public void OnLoaded()
        {
            SelectedItem ??= Items.FirstOrDefault();
            UnRegisterServerEvent();
            RegisterServerEvent();
        }

        public void OnUnLoaded()
        {
            UnRegisterServerEvent();
        }

        public string OnMCSClosing()
        {
            if (IsScanning || IsInOfflineRecon)
            {
                return Quality_Lang.Quality_Close_InScanOrRecon;
            }

            return string.Empty;
        }

        // [RelayCommand(CanExecute = nameof(OnCanButtonStartScan))]
        [RelayCommand]
        private async Task OnButtonStartScan()
        {
            if (SelectedItem?.Entries == null || SelectedItem.Entries.Count == 0 || SelectedItem.Entries.All(i => !i.IsChecked))
            {
                return;
            }

            if (!SelectedItem.IsPhantom && _offlineReconSucItems.Contains(SelectedItem) && SelectedItem.Entries.Where(i => !i.IsChecked).All(i => !i.IsOfflineReconSucceed))
            {
                _offlineReconSucItems.Remove(SelectedItem);
            }

            foreach (var item in SelectedItem.Entries)
            {
                if (!item.IsChecked)
                {
                    continue;
                }

                item.Status = StatusType.WaitScanRecon;
                _scanQueue.Enqueue(item);
            }

            if (SelectedItem.IsPhantom)
            {
                _tableService.SaveCurrentTablePosition();
            }

            _scanReconStatusInfo.Reset();
            SetInScan(true);
            await ScanRecon();
        }

        public bool CanButtonStartScan => OnCanButtonStartScan();

        private bool OnCanButtonStartScan()
        {
            return SelectedItem != null
                && !IsScanning
                && (SelectedItem.IsPhantom
                 || _config.SkipPhantomValidate
                 || _integrationPhantomItem?.Result == true);
        }

        // [RelayCommand(CanExecute = nameof(IsScanning))]
        [RelayCommand]
        private void OnButtonStopScan()
        {
            foreach (var item in _scanQueue)
            {
                item.Status = StatusType.Cancel;
            }

            _scanQueue.Clear();

            if (_curScanReconItem == null)
            {
                ScanQueueEnd();
                return;
            }

            var delRawData = _dialog.ShowConfirm(Quality_Lang.Quality_DeleteRawData);

            if (_curScanReconItem.Status is StatusType.None or StatusType.WaitScanRecon)
            {
                _curScanReconItem.Status = StatusType.Cancel;
                ScanQueueEnd();
                return;
            }

            if (_curScanReconItem!.Status != StatusType.ScanRecon)
            {
                _dialog.ShowInfo(Quality_Lang.Quality_DeleteRawData_ScanComplete);
                // ScanQueueEnd();
                return;
            }

            var res = AcqReconProxy.Instance.AbortScan(new AbortOperation(AbortCause.UserAbort, delRawData));

            if (res.Status != CommandStatus.Success)
            {
                _curScanReconItem.Status = StatusType.Error;
                var code = res.ErrorCodes.Codes.FirstOrDefault();
                _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_Scan_StopFail, code.GetErrorCodeDescription()));
                ScanQueueEnd();
                _dialog.ShowErrorCode(code!);
                return;
            }

            _scanReconStatusInfo.IsCancelManual = true;
        }

        public bool CanButtonStopScan => IsScanning;

        // [RelayCommand(CanExecute = nameof(IsInOfflineRecon))]
        [RelayCommand]
        private void OnButtonStopOfflineRecon()
        {
            foreach (var item in _allParamItems)
            {
                if (item.Status is not (StatusType.WaitOfflineRecon or StatusType.OfflineRecon))
                {
                    continue;
                }

                var res = OfflineMachineTaskProxy.Instance.StopTask(item.OfflineReconTaskID);

                if (res.Status != CommandStatus.Success)
                {
                    var code = res.ErrorCodes.Codes.FirstOrDefault();
                    _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_OfflineRecon_StopFail, ItemInfo(item), code.GetErrorCodeDescription()));
                }
            }
        }

        public bool CanButtonStopOfflineRecon => IsInOfflineRecon;

        public void GenerateReport()
        {
            var reportHeadInfo = Global.ServiceProvider.GetRequiredService<ReportHeadInfoModel>();
            ReportUtility.CreateReport(Global.ReportFilePath, _offlineReconSucItems, reportHeadInfo, _config.ReportAllowType);
        }

        #endregion

        #region Proxy Event

        private async void OnAcqReconRealTimeChanged(object sender, RealtimeEventArgs arg)
        {
            if (_curScanReconItem == null || arg.ScanUID != _curScanReconItem.ScanUID)
            {
                return;
            }

            _scanReconStatusInfo.RealtimeStatus = arg.Status;
            _scanReconStatusInfo.AddErrorCode(arg.GetErrorCodes(), ScanReconStatusType.RealtimeStatus);
            await OnScanRealtimeReconStatusChanged(_curScanReconItem, _scanReconStatusInfo);
        }

        private async void OnAcqReconReconStatusChanged(object sender, AcqReconStatusArgs arg)
        {
            if (_curScanReconItem == null || arg.ScanUID != _curScanReconItem.ScanUID)
            {
                return;
            }

            _scanReconStatusInfo.AcqReconStatus = arg.Status;
            _scanReconStatusInfo.AddErrorCode(arg.Errors.Codes, ScanReconStatusType.AcqReconStatus);
            await OnScanRealtimeReconStatusChanged(_curScanReconItem, _scanReconStatusInfo);
        }

        private void OnAcqReconRawImageSaved(object sender, RawImageSavedEventArgs arg)
        {
            if (_curScanReconItem == null || arg.ScanUID != _curScanReconItem.ScanUID)
            {
                return;
            }

            _curScanReconItem.Progress = (double)arg.FinishCount / arg.TotalCount;
        }

        private async void OnOfflineMachineTaskStatusChanged(object? sender, OfflineMachineTaskStatusChangedEventArgs arg)
        {
            var item = _allParamItems.FirstOrDefault(i => i.OfflineReconTaskID == arg.TaskID);

            if (item == null)
            {
                return;
            }

            switch (arg.TaskStatus)
            {
                case OfflineMachineTaskStatus.Initialize:
                case OfflineMachineTaskStatus.Created:
                case OfflineMachineTaskStatus.Waiting:
                {
                    if (item.Status != StatusType.WaitOfflineRecon)
                    {
                        item.Status = StatusType.WaitOfflineRecon;
                        item.Progress = 0;
                    }

                    break;
                }
                case OfflineMachineTaskStatus.Executing:
                {
                    if (item.Status != StatusType.OfflineRecon)
                    {
                        item.Status = StatusType.OfflineRecon;
                        _messagePrintService.PrintLoggerInfo(string.Format(Quality_Lang.Quality_OfflineRecon_Begin, ItemInfo(item)));
                    }

                    break;
                }
                case OfflineMachineTaskStatus.Finished:
                {
                    await OnOfflineReconComplete(item);
                    break;
                }
                case OfflineMachineTaskStatus.Cancelled:
                {
                    OnOfflineReconCancel(item);
                    break;
                }
                case OfflineMachineTaskStatus.Error:
                {
                    OnOfflineReconError(item, arg.ErrorCode);
                    break;
                }
            }
        }

        private void OnOfflineMachineTaskProgressChanged(object? sender, OfflineMachineTaskProgressChangedEventArgs arg)
        {
            var item = _allParamItems.FirstOrDefault(i => i.OfflineReconTaskID == arg.TaskID);

            if (item == null)
            {
                return;
            }

            item.Progress = (double)arg.ProgressStep / arg.TotalStep * 100;
        }

        private void OnOfflineMachineTaskDicomImageSavedInfoReceived(object? sender, DicomImageSavedInfoReceivedEventArgs arg)
        {
            var item = _allParamItems.FirstOrDefault(i => i.OfflineReconTaskID == arg.TaskID);

            if (item == null)
            {
                return;
            }

            if (arg.IsFinished)
            {
                item.OfflineReconImageFolder = arg.DicomImageDirectory;
            }
        }

        #endregion

        #region ScanRecon/OfflineRecon

        private async Task ScanRecon()
        {
            if (!_scanQueue.TryDequeue(out var item))
            {
                ScanQueueEnd();
                return;
            }

            if (item.Status is StatusType.ScanRecon or StatusType.WaitOfflineRecon or StatusType.OfflineRecon)
            {
                _messagePrintService.PrintLoggerError(Quality_Lang.Quality_Scan_Fail_PreviousRunning);
                await ScanRecon();
                return;
            }

            _curScanReconItem = item;
            _scanReconStatusInfo.Reset();
            _messagePrintService.PrintLoggerInfo(string.Format(Quality_Lang.Quality_Scan_Begin, ItemInfo(item)));

            if (BeforeScan != null)
            {
                var res = BeforeScan(item);

                if (!res.Success)
                {
                    Error(string.Format(Quality_Lang.Quality_Scan_BeforeFail, res.Message));
                    await ScanRecon();
                    return;
                }
            }

            var setRes = SetScanAndReconParam(item);

            if (!setRes.Success)
            {
                Error(Quality_Lang.Quality_Scan_CreateParamFail);
                await ScanRecon();
                return;
            }

            if (AcqReconProxy.Instance.CurrentDeviceSystem.SystemStatus != SystemStatus.Standby)
            {
                await Task.Delay(3000);
            }

            LogService.Instance.Info(ServiceCategory.QualityTest, $"StartScan Param:{JsonSerializeHelper.ToJson(item.ScanReconParamDto)}");
            var result = ScanReconWrapper.StartScan(item.ScanReconParamDto);

            if (result.Status != CommandStatus.Success)
            {
                var code = result.ErrorCodes.Codes.FirstOrDefault();
                Error(string.Format(Quality_Lang.Quality_Scan_StartFail, code.GetErrorCodeDescription()));
                await ScanRecon();
            }

            // TODO:正式代码需删除
            // item.OfflineReconImageFolder = @"F:\T\Dicom\1.2.840.1.59.0.8569.2410221335514000.141";
            // await OnOfflineReconComplete(item);
            // await ScanRecon();

            void Error(string msg)
            {
                item.Status = StatusType.Error;
                _messagePrintService.PrintLoggerError(msg);
            }
        }

        private async Task OnScanRealtimeReconStatusChanged(ItemEntryModel item, ScanReconStatusModel info)
        {
            try
            {
                var status = info.GetScanReconStatus();

                switch (status)
                {
                    case ScanReconStatus.Inprogress:
                    {
                        item.Status = StatusType.ScanRecon;
                        return;
                    }
                    case ScanReconStatus.Cancelled:
                    {
                        await OnScanReconCancel(item);
                        return;
                    }
                    case ScanReconStatus.Finished:
                    {
                        await OnScanReconComplete(item);
                        return;
                    }
                    case ScanReconStatus.Error:
                    {
                        var errorCodeTuple = info.GetErrorCode();
                        var errorCode = errorCodeTuple?.ErrorCode ?? string.Empty;
                        await OnScanReconError(item, errorCode, true);
                        return;
                    }
                    case ScanReconStatus.Emergency:
                    {
                        OnScanEmergencyStopped(item);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                await OnScanReconError(item, e.Message, false);
            }
        }

        private async Task OnScanReconComplete(ItemEntryModel item)
        {
            _messagePrintService.PrintLoggerInfo(Quality_Lang.Quality_Scan_Complete);
            var createRes = ScanReconWrapper.CreateOfflineReconTask(item.OfflineReconParamDto, TaskPriority.High);

            if (createRes.Status == CommandStatus.Success)
            {
                item.OfflineReconTaskID = createRes.TaskID;
                var startRes = OfflineMachineTaskProxy.Instance.StartTask(createRes.TaskID);

                if (startRes.Status == CommandStatus.Success)
                {
                    SetInOfflineRecon(true);
                }
                else
                {
                    item.Status = StatusType.Error;
                    var code = startRes.ErrorCodes.Codes.FirstOrDefault();
                    _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_OfflineRecon_StartFail, ItemInfo(item), code.GetErrorCodeDescription()));
                }
            }
            else
            {
                item.Status = StatusType.Error;
                var code = createRes.ErrorCodes.Codes.FirstOrDefault();
                _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_OfflineRecon_CreateFail, ItemInfo(item), code.GetErrorCodeDescription()));
            }

            await ScanRecon();
        }

        private async Task OnScanReconCancel(ItemEntryModel item)
        {
            _scanReconStatusInfo.IsCancelManual = false;
            item.Status = StatusType.Cancel;
            _messagePrintService.PrintLoggerInfo(Quality_Lang.Quality_Scan_Cancel);
            await ScanRecon();
        }

        private async Task OnScanReconError(ItemEntryModel item, string error, bool isErrorCode)
        {
            item.Status = StatusType.Error;
            var errorMsg = isErrorCode ? error.GetErrorCodeDescription() : error;
            _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_Scan_Error, errorMsg));
            await ScanRecon();
        }

        private void OnScanEmergencyStopped(ItemEntryModel item)
        {
            _scanReconStatusInfo.IsCancelManual = false;
            item.Status = StatusType.Cancel;

            foreach (var i in _scanQueue)
            {
                i.Status = StatusType.Cancel;
            }

            _scanQueue.Clear();
            ScanQueueEnd();
            ScanReconHelper.AlertEmergencyScanStopped();
        }

        private async Task OnOfflineReconComplete(ItemEntryModel item)
        {
            if (string.IsNullOrWhiteSpace(item.OfflineReconImageFolder) || !Directory.Exists(item.OfflineReconImageFolder) || Directory.GetFiles(item.OfflineReconImageFolder).Length == 0)
            {
                item.Status = StatusType.Error;
                _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_OfflineRecon_FolderNotExist, ItemInfo(item), item.OfflineReconImageFolder));
                return;
            }

            item.Status = StatusType.Complete;
            item.IsOfflineReconSucceed = true;
            SetExitOfflineRecon();
            _messagePrintService.PrintLoggerInfo(string.Format(Quality_Lang.Quality_OfflineRecon_Complete, ItemInfo(item)));

            if (!_offlineReconSucItems.Contains(item.Parent))
            {
                _offlineReconSucItems.Add(item.Parent);
            }

            _ = await AfterRecon(item);
        }

        private void OnOfflineReconCancel(ItemEntryModel item)
        {
            item.Status = StatusType.Cancel;
            SetExitOfflineRecon();
            _messagePrintService.PrintLoggerInfo(string.Format(Quality_Lang.Quality_OfflineRecon_Cancel, ItemInfo(item)));
        }

        private void OnOfflineReconError(ItemEntryModel item, string errorCode)
        {
            item.Status = StatusType.Error;
            SetExitOfflineRecon();
            _messagePrintService.PrintLoggerError(string.Format(Quality_Lang.Quality_OfflineRecon_Error, ItemInfo(item), errorCode.GetErrorCodeDescription()));
        }

        private void ScanQueueEnd()
        {
            _curScanReconItem = null;
            _scanReconStatusInfo.Reset();
            SetInScan(false);
            _dataStorageService.GenerateHistory(Items);
            InvalidateRequerySuggested?.Invoke();
        }

        private void SetInScan(bool value)
        {
            if (IsScanning == value)
            {
                return;
            }

            IsScanning = value;
            SetServiceToken();
        }

        private void SetInOfflineRecon(bool value)
        {
            if (IsInOfflineRecon == value)
            {
                return;
            }

            IsInOfflineRecon = value;
            SetServiceToken();
        }

        private void SetExitOfflineRecon()
        {
            if (_allParamItems.All(i => i.Status is not (StatusType.WaitOfflineRecon or StatusType.OfflineRecon)))
            {
                SetInOfflineRecon(false);
            }
        }

        private void SetServiceToken()
        {
            if (IsScanning || IsInOfflineRecon)
            {
                ServiceToken.Take(Global.ServiceAppName);
            }
            else
            {
                ServiceToken.Release(Global.ServiceAppName);
            }
        }

        #endregion

        #region Init

        private void RegisterServerEvent()
        {
            _messagePrintService.OnConsoleMessageChanged += OnConsoleMessageChanged;
            AcqReconProxy.Instance.RealTimeStatusChanged += OnAcqReconRealTimeChanged;
            AcqReconProxy.Instance.AcqReconStatusChanged += OnAcqReconReconStatusChanged;
            AcqReconProxy.Instance.RawImageSaved += OnAcqReconRawImageSaved;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskStatusChanged += OnOfflineMachineTaskStatusChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskProgressChanged += OnOfflineMachineTaskProgressChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskDicomImageSavedInfoReceived += OnOfflineMachineTaskDicomImageSavedInfoReceived;
        }

        private void UnRegisterServerEvent()
        {
            _messagePrintService.OnConsoleMessageChanged -= OnConsoleMessageChanged;
            AcqReconProxy.Instance.RealTimeStatusChanged -= OnAcqReconRealTimeChanged;
            AcqReconProxy.Instance.AcqReconStatusChanged -= OnAcqReconReconStatusChanged;
            AcqReconProxy.Instance.RawImageSaved -= OnAcqReconRawImageSaved;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskStatusChanged -= OnOfflineMachineTaskStatusChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskProgressChanged -= OnOfflineMachineTaskProgressChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskDicomImageSavedInfoReceived -= OnOfflineMachineTaskDicomImageSavedInfoReceived;
        }

        #endregion

        #region Method

        private void OnConsoleMessageChanged(object? sender, string e)
        {
            RunLog = e;
        }

        private string ItemInfo(ItemEntryModel item)
        {
            return $"{item.Parent.Name} {Quality_Lang.Quality_Number} {item.ID}";
        }

        #endregion
    }
}