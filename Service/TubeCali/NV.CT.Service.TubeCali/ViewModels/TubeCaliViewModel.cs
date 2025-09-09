using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Core.DeviceInteract.Models;
using NV.CT.FacadeProxy.Essentials.EventArguments;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeCali.Enums;
using NV.CT.Service.TubeCali.Models;
using NV.CT.Service.TubeCali.Models.Config;
using NV.CT.Service.TubeCali.Services;
using NV.CT.Service.TubeCali.Services.Interface;
using NV.CT.Service.TubeCali.Utils;
using NV.MPS.Environment;

namespace NV.CT.Service.TubeCali.ViewModels
{
    public partial class TubeCaliViewModel : ObservableObject
    {
        #region Field

        private readonly IOptionsMonitor<ExposureParamModel> _exposureParamsOptions;
        private readonly IOptionsMonitor<ThresholdModel> _thresholdOptions;
        private readonly ILogService _logService;
        private readonly IDialogService _dialogService;
        private readonly IDataStorageService _dataStorageService;
        private readonly TubeCaliService _tubeCaliService;
        private readonly EventDataParseService _eventDataParseService;
        private bool _isDoingCali;
        private bool _isDoingCheck;
        private bool _allChecked;
        private uint _tubeCaliParallelCount;
        private ExposureParamModel _resultCheckExposureParams = null!;
        private ObservableCollection<SourceComponent> _sourceComponents;
        private ObservableCollection<HistoryRecord> _histories;

        #endregion

        public TubeCaliViewModel(
                IOptionsMonitor<ExposureParamModel> exposureParamsOptions,
                IOptionsMonitor<ThresholdModel> thresholdOptions,
                ILogService logService,
                IDialogService dialogService,
                IConfigService configService,
                IDataStorageService dataStorageService,
                AddressService addressService,
                TubeCaliService tubeCaliService,
                EventDataParseService eventDataParseService)
        {
            _exposureParamsOptions = exposureParamsOptions;
            _thresholdOptions = thresholdOptions;
            _logService = logService;
            _dialogService = dialogService;
            _dataStorageService = dataStorageService;
            _tubeCaliService = tubeCaliService;
            _eventDataParseService = eventDataParseService;
            _sourceComponents = [];
            _histories = [];
            TubeCaliParallelCount = 3;
            var (tubeCount, _, tubeCountPerTubeInterface) = configService.GetTubeAndTubeInterfaceCount();

            for (var i = 0; i < tubeCount; i++)
            {
                var component = new SourceComponent
                {
                    Number = i + 1,
                    NumberOfTubeInterface = i / tubeCountPerTubeInterface + 1,
                    NumberInTubeInterface = i % tubeCountPerTubeInterface + 1,
                    Address = addressService.TubeAddressCollection.Single(x => x.Number == i + 1),
                };
                SourceComponents.Add(component);
            }
        }

        #region Property

        public bool IsDoingCali
        {
            get => _isDoingCali;
            set => SetProperty(ref _isDoingCali, value);
        }

        public bool IsDoingCheck
        {
            get => _isDoingCheck;
            set => SetProperty(ref _isDoingCheck, value);
        }

        public bool AllChecked
        {
            get => _allChecked;
            set => SetProperty(ref _allChecked, value);
        }

        /// <summary>
        /// 同时进行校准的数量
        /// </summary>
        public uint TubeCaliParallelCount
        {
            get => _tubeCaliParallelCount;
            set => SetProperty(ref _tubeCaliParallelCount, value);
        }

        public ObservableCollection<SourceComponent> SourceComponents
        {
            get => _sourceComponents;
            set => SetProperty(ref _sourceComponents, value);
        }

        public ObservableCollection<HistoryRecord> Histories
        {
            get => _histories;
            set => SetProperty(ref _histories, value);
        }

        #endregion

        #region Command

        [RelayCommand]
        private async Task StartCalibration()
        {
            if (!DeviceSystem.Instance.DoorClosed)
            {
                DialogService.Instance.ShowWarning("Please close the door!");
                return;
            }

            var checkedItems = SourceComponents.Where(p => p.IsChecked).ToList();

            if (checkedItems.Count == 0)
            {
                _dialogService.ShowWarning("Please select source components that you want to cali");
                return;
            }

            checkedItems.ForEach(i => i.Status = ComponentCaliStatus.Waiting);
            IsDoingCali = true;
            var startRes = await StartCali(checkedItems);
            IsDoingCali &= startRes;
        }

        [RelayCommand]
        private void StopCalibration()
        {
            var stopItems = SourceComponents.Where(i => i is { IsChecked: true, Status: ComponentCaliStatus.Waiting or ComponentCaliStatus.Working }).ToList();
            // var stopRes = _tubeCaliService.StopTubeCali(stopItems);
            var stopRes = _tubeCaliService.StopTubeCali(SourceComponents); //需要对所有球管关闭，以确保对应寄存器全部置位回0，且让CTBox管理射线指示灯熄灭

            if (!stopRes.status)
            {
                var errorStr = stopRes.message.GetErrorCodeDescription();
                AddConsoleLog($"Stop cali error: [{stopRes.message}] {errorStr}");
                _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Stop TubeCali Error: [{stopRes.message}] {errorStr}");
                _dialogService.ShowErrorCode(stopRes.message);
                return;
            }

            AddConsoleLog("Stop cali");
            stopItems.ForEach(i => i.Status = ComponentCaliStatus.Cancelled);
            IsDoingCali = false;
        }

        [RelayCommand]
        private void StartResultCheck()
        {
            var heatCapacityThreshold = _thresholdOptions.CurrentValue.HeatCapacityThreshold;
            var belowItems = SourceComponents.Where(i => i.ThermalCapacity <= heatCapacityThreshold).Select(i => i.Number).ToList();

            if (belowItems.Count > 0)
            {
                var str = $"XRaySource {string.Join(',', belowItems)} thermal capacity below threshold {heatCapacityThreshold}";
                AddConsoleLog(str);
                _dialogService.ShowWarning(str);
                return;
            }

            _resultCheckExposureParams = _exposureParamsOptions.CurrentValue;
            var totalFrames = MPS.Configuration.SystemConfig.SourceComponentConfig.SourceComponent.SourceCount * _resultCheckExposureParams.CycleCount;
            var proxyParams = new DataAcquisitionParams
            {
                ExposureParams =
                {
                    kVs = [_resultCheckExposureParams.KV],
                    mAs = [_resultCheckExposureParams.MA],
                    ExposureTime = _resultCheckExposureParams.ExposureTime,
                    FrameTime = _resultCheckExposureParams.FrameTime,
                    TotalFrames = totalFrames,
                    AutoDeleteNumber = 0,
                    ScanOption = ScanOption.Axial,
                    ScanMode = ScanMode.Plain,
                    ExposureMode = ExposureMode.Single,
                    ExposureTriggerMode = ExposureTriggerMode.TimeTrigger,
                    Bowtie = false,
                    CollimatorOffsetEnable = true,
                    CollimatorSwitch = true,
                    CollimatorOpenMode = 2,
                    CollimatorOpenWidth = 288,
                    TableTriggerEnable = 0,
                    FramesPerCycle = totalFrames,
                    AutoScan = true,
                    ExposureDelayTime = 10000,
                    XRaySourceIndex = (uint)XRaySourceIndex.All,
                    Focal = 0,
                    ExposureRelatedChildNodesConfig = 1,
                    SlaveDevTest = 1,
                    PostOffsetFrames = 0,
                    PrepareTimeout = 3000,
                    ExposureTimeout = 3000,
                    GantrySpeed = 0,
                    EnergySpectrumMode = EnergySpectrumMode.Single,
                },
                DetectorParams =
                {
                    CurrentDoubleRead = ReadType.DoubleRead,
                    ImageFlip = 9,
                    WriteConfig = 0,
                    CurrentImageMode = ImageMode.Normal_Data_Acquisition,
                    CurrentGain = Gain.Fix24pC,
                    CurrentBinning = ScanBinning.Bin11,
                    CurrentShutterMode = ShutterMode.Global,
                    DelayExposureTime = 10000,
                    ExposureTime = _resultCheckExposureParams.ExposureTime,
                    FrameTime = _resultCheckExposureParams.FrameTime,
                    CurrentAcquisitionMode = AcquisitionMode.HighLevelTriggering,
                    ReadDealyTime = 0,
                    HeartBeatTimeInterval = 90,
                    PrintfEnable = 0,
                    TargetTemperature = 260,
                    CurrentScatteringGain = ScatteringDetectorGain.Disable,
                    CurrentSpiTime = _resultCheckExposureParams.ExposureTime,
                    EncodeMode = DetectorEncodeMode.Origin,
                    RDelay = 0.3f,
                    TDelay = 0.3f,
                    PreOffsetEnable = 0,
                    PreOffsetAcqTotalFrame = 0,
                    PreOffsetAcqStartVaildFrame = 0,
                },
            };
            AddConsoleLog("Prepare to start exposure");
            _logService.Info(ServiceCategory.SourceComponentCali, $"[TubeCali] Start result check, exposure parameters: {JsonUtil.Serialize(proxyParams)}");
            var exposureRes = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(proxyParams);
            AddConsoleLog(exposureRes.Message);

            if (!exposureRes.Status)
            {
                _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Start result check, ConfigureDataAcquisition failed: {exposureRes.Message}]");
                _dialogService.ShowError($"Start result check failed: {exposureRes.Message}]");
                return;
            }

            exposureRes = DataAcquisitionProxy.Instance.StartDataAcquisition(proxyParams);
            IsDoingCheck = exposureRes.Status;
            AddConsoleLog(exposureRes.Message);

            if (!exposureRes.Status)
            {
                _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Start result check, StartDataAcquisition failed: {exposureRes.Message}]");
                _dialogService.ShowError($"Start result check failed: {exposureRes.Message}]");
            }
        }

        [RelayCommand]
        private void StopResultCheck()
        {
            AddConsoleLog("Prepare to stop exposure");
            _logService.Info(ServiceCategory.SourceComponentCali, "[TubeCali] Stop result check");
            var response = DataAcquisitionProxy.Instance.StopDataAcquisition();
            IsDoingCheck = !response.Status;
            AddConsoleLog(response.Message);

            if (!response.Status)
            {
                _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Stop result check failed: {response.Message}]");
                _dialogService.ShowError($"Stop result check failed: {response.Message}]");
            }
        }

        [RelayCommand]
        private void ItemCheckedChanged()
        {
            AllChecked = SourceComponents.All(p => p.IsChecked);
        }

        [RelayCommand]
        private void AllCheckedChanged()
        {
            foreach (var item in SourceComponents)
            {
                item.IsChecked = AllChecked;
            }
        }

        #endregion

        #region Navigation

        public void Loaded()
        {
            UnRegister();
            Register();
        }

        public void Unloaded()
        {
            UnRegister();
        }

        private void Register()
        {
            AcqReconProxy.Instance.DeviceErrorOccurred += OnDeviceErrorOccurred;
            AcqReconProxy.Instance.CycleStatusChanged += OnCycleStatusReceived;
            AcqReconProxy.Instance.EventDataReceived += OnEventDataReceived;
            DataAcquisitionProxy.Instance.SystemStatusChanged += OnSystemStatusChanged;
        }

        private void UnRegister()
        {
            AcqReconProxy.Instance.DeviceErrorOccurred -= OnDeviceErrorOccurred;
            AcqReconProxy.Instance.CycleStatusChanged -= OnCycleStatusReceived;
            AcqReconProxy.Instance.EventDataReceived -= OnEventDataReceived;
            DataAcquisitionProxy.Instance.SystemStatusChanged -= OnSystemStatusChanged;
        }

        #endregion

        #region Event Received

        private async void OnSystemStatusChanged(object arg1, SystemStatusArgs args)
        {
            AddConsoleLog($"System status: [{Enum.GetName(args.Status)}].");

            if (!IsDoingCheck)
            {
                return;
            }

            switch (args.Status)
            {
                case SystemStatus.NormalScanStopped:
                {
                    //① 如果是点击 StopResultCheck 按钮引发的状态变化，等待IsDoingCheck确保被置位了
                    //② 如果是正常扫描结束，等待TubeIntf上报mA变化
                    await Task.Delay(2000);

                    if (!IsDoingCheck)
                    {
                        return;
                    }

                    IsDoingCheck = false;
                    var mAThreshold = _thresholdOptions.CurrentValue.MAThreshold;
                    var aboveItems = SourceComponents.Where(i => Math.Abs(i.Current - _resultCheckExposureParams.MA) > mAThreshold).Select(i => i.Number).ToList();

                    if (aboveItems.Count > 0)
                    {
                        var str = $"Exposure completed, XRaySource {string.Join(",", aboveItems)} post_ma above threshold {mAThreshold} mA";
                        AddConsoleLog(str);
                        _logService.Warn(ServiceCategory.SourceComponentCali, $"[TubeCali] {str}");
                        _dialogService.ShowWarning(str);
                    }
                    else
                    {
                        var str = "Exposure completed, result check passed";
                        AddConsoleLog(str);
                        _logService.Info(ServiceCategory.SourceComponentCali, $"[TubeCali] {str}");
                        _dialogService.ShowInfo(str);
                    }

                    break;
                }
                case SystemStatus.EmergencyStopped:
                {
                    IsDoingCheck = false;
                    AddConsoleLog("Exposure emergency stopped");
                    _logService.Error(ServiceCategory.SourceComponentCali, "[TubeCali] Exposure emergency stopped");
                    _dialogService.ShowWarning("Emergency Stopped");
                    break;
                }
                case SystemStatus.ErrorScanStopped:
                {
                    IsDoingCheck = false;
                    var errorCode = args.GetErrorCodes().First();
                    var errorDes = errorCode.GetErrorCodeDescription();
                    AddConsoleLog($"Exposure error: [{errorCode}] {errorDes}");
                    _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Exposure error: [{errorCode}] {errorDes}");
                    _dialogService.ShowErrorCode(errorCode);
                    break;
                }
            }
        }

        private void OnDeviceErrorOccurred(object? sender, ErrorInfoEventArgs e)
        {
            foreach (var errorCode in e.ErrorCodes.Codes)
            {
                var errorStr = errorCode.GetErrorCodeDescription();
                var str = $"Received Device Error: [{errorCode}] {errorStr}";
                AddConsoleLog(str);
                _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] {str}");
            }
        }

        private void OnCycleStatusReceived(object sender, CycleStatusArgs args)
        {
            foreach (var tube in args.Device.XRaySources)
            {
                var sourceComponent = SourceComponents.FirstOrDefault(p => p.Number == tube.Number);

                if (sourceComponent != null)
                {
                    sourceComponent.OilTemperature = ((float)tube.XRaySourceOilTemp).ReduceTen();
                    sourceComponent.ThermalCapacity = tube.XRaySourceHeatCap;
                }
            }
        }

        private async void OnEventDataReceived(object? sender, EventDataEventArgs args)
        {
            if (args.EventDataInfo.Type is not (EventDataType.Unknown or EventDataType.DoseInfo))
            {
                return;
            }

            if (_eventDataParseService.ParseTubeStatus(args.EventDataInfo.Data, out var tube))
            {
                await OnTubeStatusReceived(tube);
            }
            else if (_eventDataParseService.ParseVoltageStatus(args.EventDataInfo.Data, out var tubeVoltage))
            {
                OnTubeValueReceived(tubeVoltage, "voltage", (item, value) => item.Voltage = value / 10f);
            }
            else if (_eventDataParseService.ParseCurrentStatus(args.EventDataInfo.Data, out var tubeCurrent))
            {
                OnTubeValueReceived(tubeCurrent, "current", (item, value) => item.Current = value / 10f);
            }
            else if (_eventDataParseService.ParseMsStatus(args.EventDataInfo.Data, out var tubeMs))
            {
                OnTubeValueReceived(tubeMs, "ms", (item, value) => item.ExposureTime = value / 10f);
            }
        }

        private void OnTubeValueReceived((int TubeNumber, uint Value) tubeValue, string valueType, Action<SourceComponent, uint> action)
        {
            _logService.Info(ServiceCategory.SourceComponentCali, $"[TubeCali] Receive tube {valueType}, tube number: {tubeValue.TubeNumber}, tube {valueType}: {tubeValue.Value}");
            var currentItem = SourceComponents.FirstOrDefault(p => p.Number == tubeValue.TubeNumber);

            if (currentItem != null)
            {
                action(currentItem, tubeValue.Value);
            }
        }

        private async Task OnTubeStatusReceived((int TubeNumber, ComponentCaliStatus? Status) tube)
        {
            _logService.Info(ServiceCategory.SourceComponentCali, $"[TubeCali] Receive tube status, tube number: {tube.TubeNumber}, tube status: {tube.Status?.ToString() ?? "null"}");
            var currentItem = SourceComponents.FirstOrDefault(p => p.Number == tube.TubeNumber);

            if (currentItem == null || tube.Status == null)
            {
                return;
            }

            currentItem.Status = tube.Status.Value;

            if (!DeviceSystem.Instance.DoorClosed)
            {
                foreach (var item in SourceComponents)
                {
                    if (item is { IsChecked: true, Status: ComponentCaliStatus.Waiting })
                    {
                        item.Status = ComponentCaliStatus.Cancelled;
                    }
                }

                _logService.Warn(ServiceCategory.SourceComponentCali, "[TubeCali] Stop TubeCali: the door open!");
                AddConsoleLog("Stop cali: the door open!");
                IsDoingCali = false;
                TurnOffLightAndRegisterReset();
                return;
            }

            if (tube.Status.Value == ComponentCaliStatus.Working || SourceComponents.Any(i => i is { IsChecked: true, Status: ComponentCaliStatus.Working }))
            {
                return;
            }

            var waitingItems = SourceComponents.Where(i => i is { IsChecked: true, Status: ComponentCaliStatus.Waiting }).ToList();
            var startRes = await StartCali(waitingItems);
            IsDoingCali &= startRes;

            if (!IsDoingCali)
            {
                TurnOffLightAndRegisterReset();
            }
        }

        #endregion

        private async Task<bool> StartCali(IList<SourceComponent> waitingItems)
        {
            var loopTimes = 0;

            while (true)
            {
                var startItems = waitingItems.Skip((int)TubeCaliParallelCount * loopTimes++).Take((int)TubeCaliParallelCount).ToList();

                if (startItems.Count == 0)
                {
                    return false;
                }

                var startStr = $"Start cali {string.Join(' ', startItems.Select(i => i.Number))}";
                AddConsoleLog($"{startStr} after 10s");
                await Task.Delay(10000);

                if (!IsDoingCali)
                {
                    return false;
                }

                var startRes = _tubeCaliService.StartTubeCali(startItems);

                if (!startRes.status)
                {
                    startItems.ForEach(i => i.Status = ComponentCaliStatus.Failed);
                    var errorStr = startRes.message.GetErrorCodeDescription();
                    AddConsoleLog($"{startStr} error: [{startRes.message}] {errorStr}");
                    _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Start TubeCali Error: [{startRes.message}] {errorStr}");
                    continue;
                }

                AddConsoleLog(startStr);
                return true;
            }
        }

        private void TurnOffLightAndRegisterReset()
        {
            var stopRes = _tubeCaliService.StopTubeCali(SourceComponents);

            if (!stopRes.status)
            {
                var errorCode = stopRes.message;
                var errorStr = errorCode.GetErrorCodeDescription();
                AddConsoleLog($"Turn off light error: [{errorCode}] {errorStr}");
                _logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Turn Off Light And Register Reset Error: [{errorCode}] {errorStr}");
            }
        }

        private void AddConsoleLog(string message)
        {
            var now = DateTime.Now;
            DispatcherWrapper.Invoke(() => Histories.Add(new HistoryRecord { DateTime = now, Message = message }));
            _dataStorageService.AddHistoryInfo(now, message);
        }
    }
}