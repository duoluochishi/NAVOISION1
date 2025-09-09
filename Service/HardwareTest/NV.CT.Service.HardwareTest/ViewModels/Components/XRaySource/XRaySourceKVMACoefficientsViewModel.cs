using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Essentials.EventArguments;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.FacadeProxy.Models.MotionControl.Gantry;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Addresses;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.CT.Service.HardwareTest.UserControls.Components.XRaySource;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource
{
    public partial class XRaySourceKVMACoefficientsViewModel : NavigationViewModelBase
    {
        #region Field

        private readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;

        /// <summary>
        /// 12点钟方向，球管编号和机架角度基准值配置文件地址
        /// </summary>
        private readonly string _sourceAndGantryBaseFilePath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, ComponentDefaults.HardwareTest, XRaySourceKVMACoefficientDefaults.SourceAndGantryBaseFileName);

        /// <summary>
        /// 点击开始按钮后，当前执行到了哪个步骤
        /// </summary>
        private XRaySourceKVMACoefficientStartStep _startStep = XRaySourceKVMACoefficientStartStep.Stop;

        #endregion

        public XRaySourceKVMACoefficientsViewModel(
                ILogService logService,
                IMessagePrintService messagePrintService,
                IOptions<XRaySourceKVMACoefficientConfigOptions> coefficientOptions)
        {
            _logService = logService;
            _messagePrintService = messagePrintService;
            BaseParam = GetSourceAndGantryBaseParam();
            ScanParam = new()
            {
                ExposureTime = coefficientOptions.Value.ExposureTime,
                FrameTime = coefficientOptions.Value.FrameTime,
                Focus = coefficientOptions.Value.Focus,
                FramesCount = coefficientOptions.Value.FramesCount,
            };
            Items = new(SystemConfig.VoltageCurrentCoefficientConfig.Coefficients.Select(i => new CoefficientModel()
            {
                Voltage = (uint)i.KV,
                Current = (uint)i.MA,
                Sources = new(i.Sources
                               .Select(x => new SourceCoefficientModel((uint)x.Id, Math.Round(x.KVFactor / 10000d, 4), Math.Round(x.MAFactor / 10000d, 4)))
                               .OrderBy(x => x.Id))
            }));
            SelectedItem = Items.FirstOrDefault();
            SelectedSourceItem = _selectedItem?.Sources.FirstOrDefault();
        }

        #region Property

        /// <summary>
        /// 是否点击了开始按钮，处于机架运动或曝光中
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopCommand))]
        private bool _isStarting;

        /// <summary>
        /// 是否处于忙碌状态中，例如正在机架运动或曝光中、正在保存中
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool _isBusy;

        [ObservableProperty]
        private string _consoleMessage = string.Empty;

        [ObservableProperty]
        private SourceAndGantryBaseModel _baseParam;

        [ObservableProperty]
        private CoefficientScanParamModel _scanParam;

        [ObservableProperty]
        private ObservableCollection<CoefficientModel> _items;

        [ObservableProperty]
        private CoefficientModel? _selectedItem;

        [ObservableProperty]
        private SourceCoefficientModel? _selectedSourceItem;

        #endregion

        #region Command

        [RelayCommand]
        private void ClearConsoleMessage()
        {
            _messagePrintService.Clear();
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void Add()
        {
            DialogHelper.ShowDialog<XRaySourceAddKVMAPairView>(300, 200);
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void Delete(CoefficientModel deleteItem)
        {
            var str = string.Format(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_DeleteConfirm, deleteItem.Voltage, deleteItem.Current);

            if (!DialogService.Instance.ShowConfirm(str))
            {
                return;
            }

            var isSelected = SelectedItem == deleteItem;
            Items.Remove(deleteItem);

            if (isSelected)
            {
                SelectedItem = Items.FirstOrDefault();
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void Start()
        {
            if (SelectedItem == null || SelectedSourceItem == null)
            {
                return;
            }

            if (!DeviceSystem.Instance.DoorClosed)
            {
                _messagePrintService.PrintToConsole($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Door is not closed, please close the door!");
                DialogService.Instance.ShowWarning(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_DoorNotClose);
                return;
            }

            StartGantryMove();
        }

        [RelayCommand(CanExecute = nameof(IsStarting))]
        private void Stop()
        {
            switch (_startStep)
            {
                case XRaySourceKVMACoefficientStartStep.GantryMove:
                {
                    StopGantryMove();
                    break;
                }
                case XRaySourceKVMACoefficientStartStep.Exposure:
                {
                    StopExposure();
                    break;
                }
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task Save()
        {
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Prepare to save.");
            SystemConfig.VoltageCurrentCoefficientConfig.Coefficients =
            [
                ..Items.Select(i => new CategoryCoefficientInfo()
                {
                    KV = (int)i.Voltage,
                    MA = (int)i.Current,
                    Sources =
                    [
                        ..i.Sources.Select(x => new SourceCoefficientInfo()
                        {
                            Id = (int)x.Id,
                            KVFactor = (int)(x.KVFactor * 10000),
                            MAFactor = (int)(x.MAFactor * 10000),
                        })
                    ],
                })
            ];
            _logService.Info(ServiceCategory.HardwareTest, $"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Save Data: {JsonUtil.Serialize(SystemConfig.VoltageCurrentCoefficientConfig.Coefficients)}].");
            SaveSourceAndGantryBaseParam();

            if (!SystemConfig.SaveVoltageCurrentCoefficientConfig())
            {
                _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Save failed: config save failed.");
                DialogService.Instance.ShowError(HardwareTest_Lang.Hardware_SaveConfigFailed);
                return;
            }

            IsBusy = true;
            var res = await XRaySourceInteractProxy.Instance.UploadVoltageCurrentConfigAsync();
            IsBusy = false;

            if (res.Status)
            {
                _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Save completed.");
            }
            else
            {
                var description = res.Code.GetErrorCodeDescription();
                _messagePrintService.PrintLoggerError($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] [{res.Code}] Save failed: {description}.");
                DialogService.Instance.ShowErrorCode(res.Code);
            }
        }

        private bool IsNotBusy()
        {
            return !IsBusy;
        }

        #endregion

        #region Gantry Move And Exposure

        private void StartGantryMove()
        {
            //下发机架转动命令，监控机架到位状态
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Prepare to start gantry move, target Source {SelectedSourceItem!.Id}.");
            var gantryParams = GetGantryParams(SelectedSourceItem!.Id);
            _logService.Info(ServiceCategory.HardwareTest, $"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Gantry Move Parameters: {JsonUtil.Serialize(gantryParams)}].");
            var gantryRes = MotionControlProxy.Instance.StartMoveGantry(gantryParams);
            _messagePrintService.PrintResponse(gantryRes);

            if (!gantryRes.Status)
            {
                Reset();
                DialogService.Instance.ShowError(string.Format(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_StartExposure_Failed, gantryRes.Message));
                return;
            }

            _startStep = XRaySourceKVMACoefficientStartStep.GantryMove;
            IsStarting = true;
            IsBusy = true;
        }

        private void StartExposure()
        {
            //下发曝光命令，监控DataAcquisitionProxy的各类状态
            var exposureDelayTime = 10000;
            var exposureParam = new DataAcquisitionParameters()
            {
                ExposureParameters = new()
                {
                    KVs = [SelectedItem!.Voltage, 0, 0, 0, 0, 0, 0, 0],
                    MAs = [SelectedItem.Current, 0, 0, 0, 0, 0, 0, 0],
                    ExposureTime = ScanParam.ExposureTime,
                    FrameTime = ScanParam.FrameTime,
                    TotalFrames = ScanParam.FramesCount,
                    Focus = ScanParam.Focus,
                    XRaySourceIndex = (XRaySourceIndex)SelectedSourceItem!.Id,
                    AutoDeleteNumber = 0,
                    ScanOption = ScanOption.Surview,
                    ScanMode = ScanMode.Plain,
                    ExposureMode = ExposureMode.Single,
                    ExposureTriggerMode = ExposureTriggerMode.TimeTrigger,
                    Bowtie = CommonSwitch.Disable,
                    CollimatorOffsetEnable = CommonSwitch.Enable,
                    CollimatorSwitch = CommonSwitch.Enable,
                    CollimatorOpenType = CollimatorValidOpenType.NearCenter_288,
                    TableTriggerEnable = CommonSwitch.Disable,
                    FramesPerCycle = 1,
                    ExposureDelayTime = exposureDelayTime,
                    ExposureRelatedChildNodesConfig = 1,
                    SlaveDevTest = 1,
                    PostOffsetFrames = 0,
                    PrepareTimeout = 3000,
                    ExposureTimeout = 3000,
                    GantryVelocity = 0,
                },
                DetectorParameters = new()
                {
                    CurrentDoubleRead = ReadType.DoubleRead,
                    ImageFlip = 9,
                    WriteConfig = 0,
                    CurrentImageMode = ImageMode.Normal_Data_Acquisition,
                    CurrentGain = Gain.Fix24pC,
                    CurrentBinning = ScanBinning.Bin11,
                    CurrentShutterMode = ShutterMode.Global,
                    ExposureTime = ScanParam.ExposureTime,
                    DelayExposureTime = exposureDelayTime,
                    FrameTime = ScanParam.FrameTime,
                    CurrentAcquisitionMode = AcquisitionMode.HighLevelTriggering,
                    ReadDealyTime = 0,
                    HeartBeatTimeInterval = 90,
                    PrintfEnable = CommonSwitch.Disable,
                    TargetTemperature = 260,
                    CurrentScatteringGain = ScatteringDetectorGain.Disable,
                    CurrentSpiTime = ScanParam.ExposureTime,
                    EncodeMode = DetectorEncodeMode.Origin,
                    RDelay = 0.3f,
                    TDelay = 0.3f,
                    PreOffsetEnable = CommonSwitch.Disable,
                    PreOffsetAcqTotalFrame = 0,
                    PreOffsetAcqStartVaildFrame = 0,
                }
            };
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Prepare to start exposure.");
            _logService.Info(ServiceCategory.HardwareTest, $"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Exposure Parameters: {JsonUtil.Serialize(exposureParam)}.");
            var exposureParamProxy = exposureParam.ToProxyParam();
            var exposureRes = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(exposureParamProxy);
            _messagePrintService.PrintResponse(exposureRes);

            if (!exposureRes.Status)
            {
                Reset();
                DialogService.Instance.ShowError(string.Format(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_StartExposure_Failed, exposureRes.Message));
                return;
            }

            exposureRes = DataAcquisitionProxy.Instance.StartDataAcquisition(exposureParamProxy);
            _messagePrintService.PrintResponse(exposureRes);

            if (!exposureRes.Status)
            {
                Reset();
                DialogService.Instance.ShowError(string.Format(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_StartExposure_Failed, exposureRes.Message));
                return;
            }

            _startStep = XRaySourceKVMACoefficientStartStep.Exposure;
        }

        private void StopGantryMove()
        {
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Prepare to stop gantry move.");
            var response = MotionControlProxy.Instance.StopMoveGantry();
            _messagePrintService.PrintResponse(response);

            if (!response.Status)
            {
                DialogService.Instance.ShowError(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_StopExposure_Failed);
                return;
            }

            Reset();
        }

        private void StopExposure()
        {
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Prepare to stop exposure.");
            var response = DataAcquisitionProxy.Instance.StopDataAcquisition();
            _messagePrintService.PrintResponse(response);

            if (!response.Status)
            {
                DialogService.Instance.ShowError(HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_StopExposure_Failed);
                return;
            }

            Reset();
        }

        private void MotionControlProxy_EventDataReceived(object sender, EventDataEventArgs args)
        {
            if (!IsStarting || _startStep != XRaySourceKVMACoefficientStartStep.GantryMove)
            {
                return;
            }

            var keyValuePair = EventDataHelper.ParseEventData(args.EventDataInfo.Data);

            if (keyValuePair.address == GantryRegisterAddresses.SystemStaus)
            {
                _logService.Info(ServiceCategory.HardwareTest, $"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Received gantry event data, address:{keyValuePair.address:X}, value:{keyValuePair.content:X}.");

                if (GantryControlHelper.IsGantryInPlace(keyValuePair.content))
                {
                    _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Gantry move in place.");
                    StartExposure(); //机架到位后，开始曝光
                }
            }
        }

        private void DataAcquisitionProxy_SystemStatusChanged(object sender, SystemStatusArgs args)
        {
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] System status: [{Enum.GetName(args.Status)}].");

            if (!IsStarting || _startStep != XRaySourceKVMACoefficientStartStep.Exposure)
            {
                return;
            }

            switch (args.Status)
            {
                case SystemStatus.NormalScanStopped:
                {
                    Reset();
                    _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Exposure completed.");
                    break;
                }
                case SystemStatus.EmergencyStopped:
                {
                    Reset();
                    _messagePrintService.PrintLoggerError($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Exposure emergency stopped.");
                    DialogService.Instance.ShowWarning("Emergency Stopped");
                    break;
                }
                case SystemStatus.ErrorScanStopped:
                {
                    Reset();
                    var errorCode = args.GetErrorCodes().FirstOrDefault();
                    _messagePrintService.PrintLoggerError($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] [{errorCode}] Exposure ErrorScanStopped: {errorCode.GetErrorCodeDescription()}.");

                    if (errorCode == null)
                    {
                        DialogService.Instance.ShowErrorCode("Exposure ErrorScanStopped");
                    }
                    else
                    {
                        DialogService.Instance.ShowErrorCode(errorCode);
                    }

                    break;
                }
            }
        }

        private void DataAcquisitionProxy_RealTimeStatusChanged(object sender, RealtimeEventArgs args)
        {
            _messagePrintService.PrintLoggerInfo($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Realtime status: [{Enum.GetName(args.Status)}].");
        }

        #endregion

        #region MessageReceived And MessagePrint

        private void OnReceivedAddKVMAPair(object sender, AddKVMAPairMessage message)
        {
            if (Items.Any(i => i.Voltage == message.Item.Voltage && i.Current == message.Item.Current))
            {
                message.Reply((false, HardwareTest_Lang.Hardware_XRaySource_KVMACoefficients_Add_AlreadyExist));
                return;
            }

            Items.Add(message.Item);
            SelectedItem = message.Item;
            message.Reply((true, string.Empty));
        }

        private void MessagePrintService_OnConsoleMessageChanged(object? sender, string message)
        {
            ConsoleMessage = message;
        }

        #endregion

        #region Registration

        private void Register()
        {
            _messagePrintService.OnConsoleMessageChanged += MessagePrintService_OnConsoleMessageChanged;
            MotionControlProxy.Instance.EventDataReceived += MotionControlProxy_EventDataReceived;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged += DataAcquisitionProxy_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.SystemStatusChanged += DataAcquisitionProxy_SystemStatusChanged;
            WeakReferenceMessenger.Default.Register<AddKVMAPairMessage>(this, OnReceivedAddKVMAPair);
        }

        private void UnRegister()
        {
            _messagePrintService.OnConsoleMessageChanged -= MessagePrintService_OnConsoleMessageChanged;
            MotionControlProxy.Instance.EventDataReceived -= MotionControlProxy_EventDataReceived;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged -= DataAcquisitionProxy_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.SystemStatusChanged -= DataAcquisitionProxy_SystemStatusChanged;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        #endregion

        #region Navigation

        public override void BeforeNavigateToCurrentPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Enter [{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] page.");
            Register();
        }

        public override void BeforeNavigateToOtherPage()
        {
            UnRegister();
            _logService.Info(ServiceCategory.HardwareTest, $"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Leave [{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] page.");
        }

        #endregion

        #region Method

        private SourceAndGantryBaseModel GetSourceAndGantryBaseParam()
        {
            SourceAndGantryBaseModel? model = null;

            if (!File.Exists(_sourceAndGantryBaseFilePath))
            {
                var dir = Path.GetDirectoryName(_sourceAndGantryBaseFilePath)!;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            else
            {
                var str = File.ReadAllText(_sourceAndGantryBaseFilePath);
                model = JsonUtil.Deserialize<SourceAndGantryBaseModel>(str);
            }

            model ??= new()
            {
                SourceId = 8,
                GantryAngle = 74,
            };
            model.ResetChanged();
            return model;
        }

        private void SaveSourceAndGantryBaseParam()
        {
            if (BaseParam.IsChanged)
            {
                var str = JsonUtil.Serialize(BaseParam);
                File.WriteAllText(_sourceAndGantryBaseFilePath, str);
            }
        }

        private GantryParams GetGantryParams(uint sourceId)
        {
            var currentAngle = ((double)DeviceSystem.Instance.Gantry.Position).ReduceHundred();
            var intervalAngle = 360d / SystemConfig.SourceComponentConfig.SourceComponent.SourceCount;
            var calcAngle = ((int)sourceId - (int)BaseParam.SourceId) * intervalAngle + BaseParam.GantryAngle;
            var angle = calcAngle switch
            {
                var calc when calc < SystemConfig.GantryConfig.Gantry.Angle.Min.ReduceHundred() => calcAngle + 360,
                var calc when calc > SystemConfig.GantryConfig.Gantry.Angle.Max.ReduceHundred() => calcAngle - 360,
                _ => calcAngle,
            };
            _messagePrintService.PrintToConsole($"[{XRaySourceKVMACoefficientDefaults.KVMACoefficients}] Gantry current position: {currentAngle}, move to {angle}.");
            return new GantryParams()
            {
                Velocity = 200,
                MoveMode = GantryMoveMode.PositionMode,
                MoveDirection = angle >= currentAngle ? GantryDirection.Clockwise : GantryDirection.CounterClockwise,
                TargetPosition = (uint)angle.ExpandHundred(),
            };
        }

        partial void OnSelectedItemChanged(CoefficientModel? value)
        {
            SelectedSourceItem = value?.Sources.FirstOrDefault();
        }

        private void Reset()
        {
            DispatcherWrapper.Invoke(() =>
            {
                IsStarting = false;
                IsBusy = false;
                _startStep = XRaySourceKVMACoefficientStartStep.Stop;
            });
        }

        #endregion
    }
}