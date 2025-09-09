using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.OfflineMachineEnums;
using NV.CT.FacadeProxy.Common.EventArguments.OfflineMachine;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Models;
using NV.CT.Service.Common.Models.ScanReconModels;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.Common.Wrappers;
using NV.CT.Service.HardwareTest.Attachments.DesignPatterns;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Managers;
using NV.CT.Service.HardwareTest.Models.Integrations.ImageChain;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.CT.Service.HardwareTest.UserControls.Integrations.ImageChain;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using NV.CT.Service.Models;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.ImageChain
{
    public partial class ImageChainTestingViewModel : NavigationViewModelBase
    {
        #region Fields

        private const string ScanReconParamFile = "ScanReconParameter.json";
        private readonly ILogService _logService;
        private readonly object _lock = new();

        /// <summary>
        /// 用于扫描、重建的 <seealso cref="ScanReconParamModel"/>
        /// </summary>
        private readonly ScanReconParamModel _scanReconParam;

        /// <summary>
        /// 记录采集和实时重建的状态相关信息
        /// </summary>
        private readonly ScanReconStatusModel _scanReconStatusInfo = new();

        /// <summary>
        /// 扫描重建参数TabItemIndex
        /// </summary>
        private int _reconParametersTabItemTotalCount;

        #endregion

        public ImageChainTestingViewModel(ILogService logService)
        {
            _logService = logService;
            InitializeProperties();
            InitializeProxy();
            ScanTabItem = (ScanParametersTabItem)ScanReconParametersTabItems.First();
            ScanTabItem.OnCalculateError += ScanTabItem_OnCalculateError;
            _scanReconParam = new()
            {
                Patient = ScanReconParametersFactory.CreatePatient(),
                Study = ScanReconParametersFactory.CreateStudy(),
                ScanParameter = ScanTabItem.ScanParam,
            };
        }

        #region Properties

        /// <summary>
        /// 图像控件Manager
        /// </summary>
        public ImageViewerManager ImageViewerManager { get; set; } = null!;

        /// <summary>
        /// 扫描参数
        /// </summary>
        private ScanParametersTabItem ScanTabItem { get; }

        /// <summary>
        /// 扫描重建参数TabItem集合
        /// </summary>
        public ObservableCollection<AbstractParametersTabItem> ScanReconParametersTabItems { get; set; }

        /// <summary>
        /// 被选中的TabItem Index
        /// </summary>
        [ObservableProperty]
        private int _selectedScanReconParametersTabItemIndex;

        /// <summary>
        /// 数据加载进度
        /// </summary>
        [ObservableProperty]
        private int _dataLoadProgress;

        /// <summary>
        /// 数据加载进度的显隐
        /// </summary>
        [ObservableProperty]
        private bool _dataLoadProgressVisibility;

        [ObservableProperty]
        private bool _isAddPopupBoxOpened;

        #endregion

        #region Command

        [RelayCommand]
        private async Task LoadRawDataSeriesFolderAsync()
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            var dialogResult = folderBrowserDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                var result = await LoadRawDataCommonAsync(folderBrowserDialog.SelectedPath);

                if (result)
                {
                    var getScanReconParamRes = GetScanParameter(folderBrowserDialog.SelectedPath);

                    if (!getScanReconParamRes.status)
                    {
                        PrintLoggerErrorWithDialog(ScanTabItem, getScanReconParamRes.message);
                        return;
                    }

                    var scanParam = getScanReconParamRes.data!.ScanParameter.ToModel();
                    var modifyRes = ScanTabItem.ModifyScanParam(scanParam);

                    if (!modifyRes.status)
                    {
                        PrintLoggerErrorWithDialog(ScanTabItem, modifyRes.message);
                        return;
                    }

                    ScanTabItem.ScanParam.RawDataDirectory = folderBrowserDialog.SelectedPath;
                    ScanTabItem.RawDataDirectory = folderBrowserDialog.SelectedPath;
                    ScanTabItem.ScanStatus = ScanStatus.NormalStop;
                    ScanTabItem.ProgressValue = 0;
                    _scanReconParam.Study = getScanReconParamRes.data!.Study.ToModel();
                    _scanReconParam.ScanParameter = ScanTabItem.ScanParam;
                    SetReconTabItemHasValidRawData(true);
                }
            }
        }

        [RelayCommand]
        private void LoadDicomSeriesFolder()
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            var dialogResult = folderBrowserDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                ImageViewerManager.LoadDicomSeries(folderBrowserDialog.SelectedPath);
            }
        }

        [RelayCommand]
        private void ExecuteImageCut()
        {
            var response = ImageViewerManager.CutImage();
            ScanTabItem.PrintConsoleMessage($"[{ComponentDefaults.ImageChain}] {response.Message}", response.Status ? PrintLevel.Info : PrintLevel.Error);
        }

        [RelayCommand]
        private void ExecuteImageSort()
        {
            DialogHelper.ShowDialog<ImageChainImageSortView>(1035, 750);
        }

        [RelayCommand]
        private void CloseReconParametersTabItem(AbstractParametersTabItem tabItem)
        {
            if (DialogService.Instance.ShowConfirm($"Confirm to remove [{tabItem.TabItemHeader}] ?"))
            {
                ScanReconParametersTabItems.Remove(tabItem);
            }
        }

        [RelayCommand]
        private void AddReconParametersTabItem(object testingScenarioName)
        {
            ScanReconParametersTabItems.Add(new ReconParametersTabItem($"Recon Parameters {++_reconParametersTabItemTotalCount}")
            {
                HasValidRawData = ScanTabItem.CanExecuteReconProcedure
            });
            SelectedScanReconParametersTabItemIndex = ScanReconParametersTabItems.Count - 1;
            IsAddPopupBoxOpened = false;
        }

        [RelayCommand]
        private void OpenDataAnalysisTool(AbstractParametersTabItem item)
        {
            var folder = item.GetImageDataFolder();
            var res = DataAnalysisToolUtil.OpenFolder(folder);

            if (!res.status)
            {
                PrintLoggerErrorWithDialog(item, res.message);
            }
        }

        #region Scan Control

        [RelayCommand]
        private void UseCurrentTablePosition()
        {
            ScanTabItem.ScanParam.ReconVolumeStartPosition = ((double)(DeviceSystem.Instance.Table.HorizontalPosition - SystemConfig.LaserConfig.Laser.Offset.Value)).Micron2Millimeter();
            ScanTabItem.ScanParam.TableHeight = ((double)DeviceSystem.Instance.Table.VerticalPosition).Micron2Millimeter();
        }

        /// <summary>
        /// 开始扫描Command
        /// </summary>
        [RelayCommand]
        private void StartScan()
        {
            if (ScanTabItem.ScanStatus == ScanStatus.Scanning)
            {
                return;
            }

            try
            {
                _scanReconStatusInfo.Reset();
                ScanTabItem.ScanStatus = ScanStatus.Scanning;
                ScanTabItem.ProgressValue = 0;
                AddReconSeriesParam(null);
                _scanReconParam.Update();
                var result = ScanReconWrapper.StartScan(_scanReconParam);

                if (result.Status != CommandStatus.Success)
                {
                    ScanTabItem.ScanStatus = ScanStatus.NormalStop;
                    ErrorCodesToPrintLoggerErrorWithDialog(ScanTabItem, result.ErrorCodes, "Failed to start scanning");
                    return;
                }

                PrintLoggerInfo(ScanTabItem, "Scanning has started.");
            }
            catch (Exception ex)
            {
                ScanTabItem.ScanStatus = ScanStatus.NormalStop;
                PrintLoggerErrorWithDialog(ScanTabItem, $"Something wrong when [Start Scan]: {ex.Message}", $"Failed to start scanning: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 停止扫描Command
        /// </summary>
        [RelayCommand]
        private void StopScan()
        {
            // if (ScanTabItem.ScanStatus == ScanStatus.NormalStop)
            // {
            //     return;
            // }

            try
            {
                var abortOperation = new AbortOperation(AbortCause.UserAbort, false);
                var result = AcqReconProxy.Instance.AbortScan(abortOperation);

                if (result.Status != CommandStatus.Success)
                {
                    ErrorCodesToPrintLoggerErrorWithDialog(ScanTabItem, result.ErrorCodes, "Failed to stop scanning");
                    return;
                }

                _scanReconStatusInfo.IsCancelManual = true;
                PrintLoggerInfo(ScanTabItem, "Stopping scanning.");
            }
            catch (Exception ex)
            {
                PrintLoggerErrorWithDialog(ScanTabItem, $"Something wrong when [Stop Scan]: {ex.Message}", $"Failed to stop scanning: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 载入生数据Command
        /// </summary>
        [RelayCommand]
        private async Task LoadRawDataAsync()
        {
            await LoadRawDataCommonAsync(ScanTabItem.RawDataDirectory);
        }

        #endregion

        #region Offline Recon Control

        /// <summary>
        /// 开始离线重建Command
        /// </summary>
        /// <param name="reconTabItem">被选中的重建Tab页</param>
        [RelayCommand]
        private void StartOfflineRecon(ReconParametersTabItem reconTabItem)
        {
            try
            {
                if (ScanTabItem.ScanParam.ScanOption == ScanOption.Surview) //若为定位向扫描，不能进行离线重建
                {
                    PrintLoggerErrorWithDialog(reconTabItem, "Cannot start an offline recon task base on ScanOption [Surview].");
                    return;
                }

                reconTabItem.ProgressValue = 0;
                reconTabItem.ReconParam.Update(ScanTabItem.ScanParam);
                AddReconSeriesParam(reconTabItem.ReconParam);
                var createResult = ScanReconWrapper.CreateOfflineReconTask(_scanReconParam, TaskPriority.High);

                if (createResult.Status != CommandStatus.Success)
                {
                    reconTabItem.ReconStatus = ReconStatus.NormalStop;
                    ErrorCodesToPrintLoggerErrorWithDialog(reconTabItem, createResult.ErrorCodes, "Failed to create offline recon task");
                    return;
                }

                PrintLoggerInfo(reconTabItem, $"Offline recon Task has been created, Recon ID: {reconTabItem.ReconID}, Recon SeriesUID: {reconTabItem.ReconSeriesUID}, Task ID: {createResult.TaskID}");
                reconTabItem.TaskID = createResult.TaskID;
                var startResult = OfflineMachineTaskProxy.Instance.StartTask(reconTabItem.TaskID);

                if (startResult.Status != CommandStatus.Success)
                {
                    reconTabItem.ReconStatus = ReconStatus.NormalStop;
                    ErrorCodesToPrintLoggerErrorWithDialog(reconTabItem, startResult.ErrorCodes, $"Failed to start offline recon task, Recon ID: {reconTabItem.ReconID}, Recon SeriesUID: {reconTabItem.ReconSeriesUID}");
                    return;
                }

                reconTabItem.ReconStatus = ReconStatus.Reconning;
                PrintLoggerInfo(reconTabItem, $"Offline recon Task has been started, Recon ID: {reconTabItem.ReconID}, Recon SeriesUID: {reconTabItem.ReconSeriesUID}.");
            }
            catch (Exception ex)
            {
                PrintLoggerErrorWithDialog(ScanTabItem, $"Something wrong when [CreateOfflineReconTask]: {ex.Message}", $"Failed to create offline recon task: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 停止离线重建Command
        /// </summary>
        /// <param name="reconTabItem">被选中的重建Tab页</param>
        [RelayCommand]
        private void StopOfflineRecon(ReconParametersTabItem reconTabItem)
        {
            if (reconTabItem.ReconStatus == ReconStatus.NormalStop)
            {
                return;
            }

            try
            {
                var stopResult = OfflineMachineTaskProxy.Instance.StopTask(reconTabItem.TaskID);

                if (stopResult.Status != CommandStatus.Success)
                {
                    ErrorCodesToPrintLoggerErrorWithDialog(reconTabItem, stopResult.ErrorCodes, $"Failed to stop offline recon task, Recon ID: {reconTabItem.ReconID}, Recon SeriesUID: {reconTabItem.ReconSeriesUID}");
                    return;
                }

                PrintLoggerInfo(reconTabItem, $"Stopping offline recon task, Recon ID: {reconTabItem.ReconID}, Recon SeriesUID: {reconTabItem.ReconSeriesUID}.");
            }
            catch (Exception ex)
            {
                PrintLoggerErrorWithDialog(ScanTabItem, $"Something wrong when [StopOfflineReconTask]: {ex.Message}", $"Failed to stop offline recon task, Recon ID: {reconTabItem.ReconID}, Recon SeriesUID: {reconTabItem.ReconSeriesUID}: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private void SaveReconParams(ReconParametersTabItem reconTabItem)
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            var dialogResult = folderBrowserDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                var path = Path.Combine(folderBrowserDialog.SelectedPath, "ReconParameters.json");
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                using var sw = new StreamWriter(fs);
                sw.Write(JsonUtil.Serialize(reconTabItem.ReconParam));
                sw.Flush();
            }
        }

        /// <summary>
        /// 载入重建Dicom图
        /// </summary>
        /// <param name="selectedReconTabItem">被选中的重建Tab页</param>
        [RelayCommand]
        private void LoadReconDicomSeries(ReconParametersTabItem selectedReconTabItem)
        {
            LoadDicomSeriesCommon(selectedReconTabItem);
        }

        #endregion

        #endregion

        #region Proxy Events

        private void ImageChain_Acq_RealTimeStatusChanged(object sender, RealtimeEventArgs args)
        {
            ScanTabItem.PrintConsoleMessage($"[{ComponentDefaults.ImageChain}] Realtime Status: {args.Status}", PrintLevel.Info);
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Received Realtime Status Changed: {args.Status}");

            if (ScanTabItem.ScanParam.ScanUID != args.ScanUID)
            {
                return;
            }

            OnScanRealtimeReconStatusChanged(ScanTabItem, _scanReconStatusInfo, args, null);
        }

        private void ImageChain_Acq_ReconStatusChanged(object sender, AcqReconStatusArgs args)
        {
            ScanTabItem.PrintConsoleMessage($"[{ComponentDefaults.ImageChain}] Recon Status: {args.Status}", PrintLevel.Info);
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Received AcqRecon Status Changed: {args.Status}");

            if (ScanTabItem.ScanParam.ScanUID != args.ScanUID)
            {
                return;
            }

            OnScanRealtimeReconStatusChanged(ScanTabItem, _scanReconStatusInfo, null, args);
        }

        private void ImageChain_Acq_RawDataSaved(object sender, RawImageSavedEventArgs args)
        {
            if (ScanTabItem.ScanParam.ScanUID != args.ScanUID)
            {
                return;
            }

            if (args.IsFinished)
            {
                ScanTabItem.RawDataDirectory = args.Directory;
            }

            ScanTabItem.ProgressValue = (double)args.FinishCount / args.TotalCount * 100;
        }

        private void ImageChain_OfflineRecon_TaskStatusChanged(object? sender, OfflineMachineTaskStatusChangedEventArgs args)
        {
            var reconTabItem = FindReconTabItemByTaskID(args.TaskID);

            if (reconTabItem == null)
            {
                return;
            }

            reconTabItem.PrintConsoleMessage($"[{ComponentDefaults.ImageChain}] OfflineRecon Task Status: {args.TaskStatus}", PrintLevel.Info);

            switch (args.TaskStatus)
            {
                case OfflineMachineTaskStatus.Initialize:
                case OfflineMachineTaskStatus.Created:
                case OfflineMachineTaskStatus.Waiting:
                case OfflineMachineTaskStatus.Executing:
                {
                    reconTabItem.ReconStatus = ReconStatus.Reconning;
                    break;
                }
                case OfflineMachineTaskStatus.Finished:
                {
                    OnOfflineReconComplete(reconTabItem);
                    break;
                }
                case OfflineMachineTaskStatus.Cancelled:
                {
                    OnOfflineReconCancel(reconTabItem);
                    break;
                }
                case OfflineMachineTaskStatus.Error:
                {
                    OnOfflineReconError(reconTabItem, args.ErrorCode);
                    break;
                }
            }
        }

        private void ImageChain_OfflineRecon_TaskProgressChanged(object? sender, OfflineMachineTaskProgressChangedEventArgs args)
        {
            var reconTabItem = FindReconTabItemByTaskID(args.TaskID);

            if (reconTabItem == null)
            {
                return;
            }

            reconTabItem.ProgressValue = (double)args.ProgressStep / args.TotalStep * 100;
        }

        private void ImageChain_OfflineRecon_ImageSaved(object? sender, DicomImageSavedInfoReceivedEventArgs args)
        {
            var reconTabItem = FindReconTabItemByTaskID(args.TaskID);

            if (reconTabItem == null)
            {
                return;
            }

            if (args.IsFinished)
            {
                reconTabItem.ReconImageDirectory = args.DicomImageDirectory;
                PrintLoggerInfo(reconTabItem, $"Recon image has been saved, directory: {args.DicomImageDirectory}");
            }
        }

        private void ImageChain_OfflineRecon_MachineErrorReceived(object? sender, OfflineMachineErrorInfoReceivedEventArgs args)
        {
            foreach (var scanReconTabItem in ScanReconParametersTabItems)
            {
                if (scanReconTabItem is ReconParametersTabItem { ReconStatus: ReconStatus.Reconning } reconTabItem)
                {
                    OnOfflineReconError(reconTabItem, args.ErrorCode);
                }
            }
        }

        #endregion

        #region Methods

        private void OnScanRealtimeReconStatusChanged(ScanParametersTabItem item, ScanReconStatusModel info, RealtimeEventArgs? realtimeEventArgs, AcqReconStatusArgs? acqReconStatusArgs)
        {
            try
            {
                lock (_lock)
                {
                    if (realtimeEventArgs != null)
                    {
                        _scanReconStatusInfo.RealtimeStatus = realtimeEventArgs.Status;
                        _scanReconStatusInfo.AddErrorCode(realtimeEventArgs.GetErrorCodes(), ScanReconStatusType.RealtimeStatus);
                    }
                    else if (acqReconStatusArgs != null)
                    {
                        _scanReconStatusInfo.AcqReconStatus = acqReconStatusArgs.Status;
                        _scanReconStatusInfo.AddErrorCode(acqReconStatusArgs.Errors.Codes, ScanReconStatusType.AcqReconStatus);
                    }

                    var status = info.GetScanReconStatus();
                    _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] [OnScanRealtimeReconStatusChanged] ScanReconStatus: {status}");

                    switch (status)
                    {
                        case ScanReconStatus.Inprogress:
                        {
                            item.ScanStatus = ScanStatus.Scanning;
                            return;
                        }
                        case ScanReconStatus.Cancelled:
                        {
                            info.Reset();
                            OnScanReconCancel(item);
                            return;
                        }
                        case ScanReconStatus.Finished:
                        {
                            info.Reset();
                            _ = OnScanReconComplete(item);
                            return;
                        }
                        case ScanReconStatus.Error:
                        {
                            var errorCodeTuple = info.GetErrorCode();
                            var errorCode = errorCodeTuple?.ErrorCode ?? string.Empty;
                            var isRealTimeStatusError = errorCodeTuple?.StatusType == ScanReconStatusType.RealtimeStatus;
                            info.Reset();
                            OnScanReconError(item, errorCode, isRealTimeStatusError, true);
                            return;
                        }
                        case ScanReconStatus.Emergency:
                        {
                            info.Reset();
                            OnScanEmergencyStopped(item);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnScanReconError(item, e.Message, false, false);
            }
        }

        private async Task OnScanReconComplete(ScanParametersTabItem item)
        {
            PrintLoggerInfo(item, "Scanning has completed.");
            item.ScanStatus = ScanStatus.NormalStop;
            item.ProgressValue = 100;
            SetReconTabItemHasValidRawData(true);
            _ = await LoadRawDataCommonAsync(ScanTabItem.RawDataDirectory);
        }

        private void OnScanReconCancel(ScanParametersTabItem item)
        {
            item.ScanStatus = ScanStatus.NormalStop;
            PrintLoggerInfo(item, "Scanning has stopped.");
        }

        private void OnScanReconError(ScanParametersTabItem item, string error, bool isRealTimeStatusError, bool isErrorCode)
        {
            item.ScanStatus = ScanStatus.NormalStop;
            string printMsg, dialogMsg;

            if (isErrorCode)
            {
                printMsg = $"Scanning has some error: [{error}] {error.GetErrorCodeDescription()}";
                dialogMsg = error;
            }
            else
            {
                printMsg = $"Scanning has some error: {error}";
                dialogMsg = printMsg;
            }

            PrintLoggerError(item, printMsg);

            if (isRealTimeStatusError)
            {
                // 1.与CTBox确认了，当触发实时状态为Error时，一定也会触发DeviceError事件
                // 2.根据架构师朱正广的要求，DeviceError的错误由顶层订阅并弹窗，服务模块仅处理但不准弹窗
                // 综合上述两点，此处不再进行弹窗提示
            }
            else
            {
                DispatcherWrapper.CurrentDispatcher.InvokeAsync(() => { DialogError(dialogMsg, isErrorCode, false); });
            }
        }

        private void OnScanEmergencyStopped(ScanParametersTabItem item)
        {
            item.ScanStatus = ScanStatus.NormalStop;
            PrintLoggerInfo(item, "Scanning has emergency stopped.");
            DispatcherWrapper.CurrentDispatcher.InvokeAsync(ScanReconHelper.AlertEmergencyScanStopped);
        }

        private void OnOfflineReconComplete(ReconParametersTabItem item)
        {
            item.ReconStatus = ReconStatus.NormalStop;
            item.ProgressValue = 100;
            PrintLoggerInfo(item, $"Offline recon task has completed, Recon ID: {item.ReconID}, Recon SeriesUID: {item.ReconSeriesUID}.");
            LoadDicomSeriesCommon(item);
        }

        private void OnOfflineReconCancel(ReconParametersTabItem item)
        {
            item.ReconStatus = ReconStatus.NormalStop;
            PrintLoggerInfo(item, $"Offline recon task has been cancelled, Recon ID: {item.ReconID}, Recon SeriesUID: {item.ReconSeriesUID}.");
        }

        private void OnOfflineReconError(ReconParametersTabItem item, string? errorCode)
        {
            item.ReconStatus = ReconStatus.NormalStop;
            ErrorCodeToPrintLoggerErrorWithDialog(item, errorCode, $"Offline recon task has some error, Recon ID: {item.ReconID}, Recon SeriesUID: {item.ReconSeriesUID}", true);
        }

        /// <summary>
        /// 载入生数据 Common
        /// </summary>
        private async Task<bool> LoadRawDataCommonAsync(string directory)
        {
            PrintLoggerInfo(ScanTabItem, $"Start load raw data: {directory}");
            DataLoadProgress = 0;
            DataLoadProgressVisibility = true;

            if (ImageViewerManager.RawDataPool is not null) //载入前若有数据加载，清空
            {
                RawDataReadWriteHelper.Instance.Release();
            }

            var response = await Task.Run(() => RawDataReadWriteHelper.Instance.Read(directory, RawDataReadProgressChanging));

            if (!response.status)
            {
                ErrorCodeToPrintLoggerErrorWithDialog(ScanTabItem, response.message, $"Failed to read raw data, directory: {directory}");
                return false;
            }

            //延时以完成进度显示
            await Task.Delay(100);
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Start image view load raw data.");
            ImageViewerManager.LoadRawDataList(response.data);
            DataLoadProgressVisibility = false;
            PrintLoggerInfo(ScanTabItem, "Raw data has been loaded.");
            return true;
        }

        /// <summary>
        /// 图像加载进度回调
        /// </summary>
        /// <param name="count"></param>
        /// <param name="total"></param>
        private void RawDataReadProgressChanging(int count, int total)
        {
            DataLoadProgress = Convert.ToInt32((count / (float)total) * 100);
        }

        /// <summary>
        /// 载入Dicom序列
        /// </summary>
        private void LoadDicomSeriesCommon(ReconParametersTabItem reconTabItem)
        {
            ImageViewerManager.LoadDicomSeries(reconTabItem.ReconImageDirectory);
            reconTabItem.PrintConsoleMessage($"[{ComponentDefaults.ImageChain}] {reconTabItem.TabItemHeader}'s dicom series has been loaded.", PrintLevel.Info);
        }

        private void SetReconTabItemHasValidRawData(bool valid)
        {
            foreach (var reconTabItem in ScanReconParametersTabItems.OfType<ReconParametersTabItem>())
            {
                reconTabItem.HasValidRawData = valid;
            }
        }

        private ReconParametersTabItem? FindReconTabItemByTaskID(string taskID)
        {
            foreach (var scanReconTabItem in ScanReconParametersTabItems)
            {
                if (scanReconTabItem is ReconParametersTabItem reconTabItem)
                {
                    if (taskID == reconTabItem.TaskID)
                    {
                        return reconTabItem;
                    }
                }
            }

            return null;
        }

        private void AddReconSeriesParam(ReconSeriesParamModel? model)
        {
            _scanReconParam.ReconSeriesParams.Clear();

            if (model != null)
            {
                _scanReconParam.ReconSeriesParams.Add(model);
            }
        }

        private DataResponse<ScanReconParam> GetScanParameter(string folderPath)
        {
            var scanReconParamFilePath = Path.Combine(folderPath, ScanReconParamFile);

            if (!File.Exists(scanReconParamFilePath))
            {
                return new DataResponse<ScanReconParam>(false, $"Get ScanRecon parameters failed: {ScanReconParamFile} not exist", null);
            }

            var scanReconParamStr = File.ReadAllText(scanReconParamFilePath);

            try
            {
                var scanReconParam = JsonUtil.Deserialize<ScanReconParam>(scanReconParamStr);

                if (scanReconParam == null)
                {
                    return new DataResponse<ScanReconParam>(false, $"Get ScanRecon parameters failed: the content of {ScanReconParamFile} is incorrect", null);
                }

                return new DataResponse<ScanReconParam>(true, string.Empty, scanReconParam);
            }
            catch (Exception e)
            {
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Get ScanRecon parameters failed: deserialize the content of {scanReconParamFilePath} exception. ", e);
                return new DataResponse<ScanReconParam>(false, $"Get ScanRecon parameters failed: deserialize the content of {ScanReconParamFile} exception", null);
            }
        }

        private void ScanTabItem_OnCalculateError(string msg)
        {
            DialogService.Instance.ShowError(msg);
        }

        #endregion

        #region Log

        private void PrintLoggerInfo(AbstractParametersTabItem item, string msg)
        {
            var str = $"[{ComponentDefaults.ImageChain}] {msg}";
            item.PrintConsoleMessage(str, PrintLevel.Info);
            _logService.Info(ServiceCategory.HardwareTest, str);
        }

        private void PrintLoggerError(AbstractParametersTabItem item, string msg)
        {
            var str = $"[{ComponentDefaults.ImageChain}] {msg}";
            item.PrintConsoleMessage(str, PrintLevel.Error);
            _logService.Error(ServiceCategory.HardwareTest, str);
        }

        private void PrintLoggerError(AbstractParametersTabItem item, string msg, Exception e)
        {
            var str = $"[{ComponentDefaults.ImageChain}] {msg}";
            item.PrintConsoleMessage(str, PrintLevel.Error);
            _logService.Error(ServiceCategory.HardwareTest, str, e);
        }

        private void PrintLoggerErrorWithDialog(AbstractParametersTabItem item, string msg, bool useDispatcher = false)
        {
            PrintLoggerErrorWithDialog(item, msg, msg, false, useDispatcher);
        }

        private void PrintLoggerErrorWithDialog(AbstractParametersTabItem item, string msg, string dialogMsg, bool isErrorCode = false, bool useDispatcher = false)
        {
            PrintLoggerError(item, msg);
            DialogError(dialogMsg, isErrorCode, useDispatcher);
        }

        private void PrintLoggerErrorWithDialog(AbstractParametersTabItem item, string msg, string dialogMsg, Exception e, bool isErrorCode = false, bool useDispatcher = false)
        {
            PrintLoggerError(item, msg, e);
            DialogError(dialogMsg, isErrorCode, useDispatcher);
        }

        private void ErrorCodeToPrintLoggerErrorWithDialog(AbstractParametersTabItem item, string? errorCode, string msg, bool useDispatcher = false)
        {
            var isErrorCode = !string.IsNullOrWhiteSpace(errorCode);
            var message = isErrorCode ? $"{msg}, Error Message: [{errorCode}] {errorCode.GetErrorCodeDescription()}" : msg;
            var dialogMsg = isErrorCode ? errorCode! : msg;
            PrintLoggerErrorWithDialog(item, message, dialogMsg, isErrorCode, useDispatcher);
        }

        private void ErrorCodesToPrintLoggerErrorWithDialog(AbstractParametersTabItem item, ErrorCodes errorCodes, string msg, bool useDispatcher = false)
        {
            var errorCode = errorCodes.Codes.FirstOrDefault();
            ErrorCodeToPrintLoggerErrorWithDialog(item, errorCode, msg, useDispatcher);
        }

        private void DialogError(string msg, bool isErrorCode, bool useDispatcher)
        {
            if (useDispatcher)
            {
                DispatcherWrapper.Invoke(Dialog);
            }
            else
            {
                Dialog();
            }

            void Dialog()
            {
                if (isErrorCode)
                {
                    DialogService.Instance.ShowErrorCode(msg);
                }
                else
                {
                    DialogService.Instance.ShowError(msg);
                }
            }
        }

        #endregion

        #region Initialize

        [MemberNotNull(nameof(ImageViewerManager))]
        [MemberNotNull(nameof(ScanReconParametersTabItems))]
        private void InitializeProperties()
        {
            ImageViewerManager = new();
            ScanReconParametersTabItems =
            [
                new ScanParametersTabItem("Scan Parameters"),
                new ReconParametersTabItem("Recon Parameters 1")
            ];
            _reconParametersTabItemTotalCount = ScanReconParametersTabItems.Count - 1;
        }

        private void InitializeProxy()
        {
            try
            {
                AcqReconProxy.Instance.Init(new());
                _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Proxy has been initialized.");
            }
            catch (Exception ex)
            {
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Something wrong when [InitializeProxy], [Stack]: {ex}.");
            }
        }

        private void RegisterProxyEvents()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Register AcqReconProxy & OfflineMachineTaskProxy events.");
            AcqReconProxy.Instance.RealTimeStatusChanged += ImageChain_Acq_RealTimeStatusChanged;
            AcqReconProxy.Instance.AcqReconStatusChanged += ImageChain_Acq_ReconStatusChanged;
            AcqReconProxy.Instance.RawImageSaved += ImageChain_Acq_RawDataSaved;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskStatusChanged += ImageChain_OfflineRecon_TaskStatusChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskProgressChanged += ImageChain_OfflineRecon_TaskProgressChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskDicomImageSavedInfoReceived += ImageChain_OfflineRecon_ImageSaved;
            OfflineMachineTaskProxy.Instance.OfflineMachineErrorInfoReceived += ImageChain_OfflineRecon_MachineErrorReceived;
        }

        private void UnRegisterProxyEvents()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Un-register AcqReconProxy & OfflineMachineTaskProxy events.");
            AcqReconProxy.Instance.RealTimeStatusChanged -= ImageChain_Acq_RealTimeStatusChanged;
            AcqReconProxy.Instance.AcqReconStatusChanged -= ImageChain_Acq_ReconStatusChanged;
            AcqReconProxy.Instance.RawImageSaved -= ImageChain_Acq_RawDataSaved;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskStatusChanged -= ImageChain_OfflineRecon_TaskStatusChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskProgressChanged -= ImageChain_OfflineRecon_TaskProgressChanged;
            OfflineMachineTaskProxy.Instance.OfflineMachineTaskDicomImageSavedInfoReceived -= ImageChain_OfflineRecon_ImageSaved;
            OfflineMachineTaskProxy.Instance.OfflineMachineErrorInfoReceived -= ImageChain_OfflineRecon_MachineErrorReceived;
        }

        #endregion

        #region Navigation

        public override void BeforeNavigateToCurrentPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Enter [Image Chain] testing page.");
            RegisterProxyEvents();
            ImageViewerManager.RegisterMessages();
        }

        public override void BeforeNavigateToOtherPage()
        {
            UnRegisterProxyEvents();
            ImageViewerManager.UnRegisterAllMessage();
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ImageChain}] Leave [Image Chain] testing page.");
        }

        #endregion
    }
}