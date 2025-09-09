using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.Collimator;
using NV.CT.FacadeProxy.Core.DeviceInteract.Models;
using NV.CT.FacadeProxy.Essentials.EventArguments;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Managers;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Attachments.Repository;
using NV.CT.Service.HardwareTest.Models.Components.Collimator;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Models.Components.Gantry;
using NV.CT.Service.HardwareTest.Models.Components.Table;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.EventData.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.CT.Service.HardwareTest.UserControls.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.UserControls.Integrations.DataAcquistion;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using RawDataHelperWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.DataAcquisition
{
    public partial class DataAcquisitionTestingViewModel : NavigationViewModelBase, IModuleDirectory
    {
        private readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;
        private readonly IEventDataAddressService _eventDataAddressService;
        private readonly DataAcquisitionConfigOptions _dataAcquisitionConfig;
        private readonly HardwareTestConfigOptions _hardwareTestConfig;
        private readonly IRepository<DetectorTemperatureData> detectorTemperatureDataRepository;
       
        public DataAcquisitionTestingViewModel(ILogService logService,
            IMessagePrintService messagePrintService,
            IEventDataAddressService eventDataAddressService,    
            IOptions<DataAcquisitionConfigOptions> dataAcquisitionOptions,
            IOptions<HardwareTestConfigOptions> hardwareTestOptions,
            IRepository<DetectorTemperatureData> detectorTemperatureDataRepository)
        {
            this._logService = logService;
            this._messagePrintService = messagePrintService;
            this._eventDataAddressService = eventDataAddressService;             
            this._dataAcquisitionConfig = dataAcquisitionOptions.Value;
            this._hardwareTestConfig = hardwareTestOptions.Value;
            this.detectorTemperatureDataRepository = detectorTemperatureDataRepository;

            InitializeProperties();
            InitializeModuleDirectory();

            WeakReferenceMessenger.Default.Register<DrawHorizontalLineMessage>(this, DrawHorizontalLine);
        }

        #region Initialize

        private void InitializeProperties()
        {
            ImageViewerManager = new();

            XDataAcquisitionParameters = new();
            XMotionControlParameters = new();
             
            XTableSource = new();
            XGantrySource = new();
            AcquisitionCardSources = new()
            {
                new AcquisitionCardSource { SerialNumber = 1 },
                new AcquisitionCardSource { SerialNumber = 2 }
            };
            XRaySources = new(
                Enumerable.Range(1, (int)_dataAcquisitionConfig.XRaySourceCount)
                .Select(i => new XRayOriginSource { Name = $"XRay Source - {i.ToString("00")}", Index = (uint)i })
            );
            CollimatorSources = new(
                Enumerable.Range(1, (int)_dataAcquisitionConfig.CollimatorSourceCount)
                .Select(i => new CollimatorSource { Name = $"{i.ToString("00")}" })
            );
            DetectorSources = new(
                Enumerable.Range(1, (int)_dataAcquisitionConfig.DetectorSourceCount)
                .Select(i => new DetectorSource { Name = $"Detector - {i.ToString("00")}" })
            );

            RelatedComponents = new() 
            {
                new RelatedComponent() { Name = "IFBox", BitOffset = 0, IsChecked = true, IsEnabled = false },
                new RelatedComponent() { Name = "PDU", BitOffset = 1 },
                new RelatedComponent() { Name = "Gantry", BitOffset = 2 },
                new RelatedComponent() { Name = "Table", BitOffset = 3 },
                new RelatedComponent() { Name = "TubeInterface1", BitOffset = 4 },
                new RelatedComponent() { Name = "TubeInterface2", BitOffset = 5 },
                new RelatedComponent() { Name = "TubeInterface3", BitOffset = 6 },
                new RelatedComponent() { Name = "TubeInterface4", BitOffset = 7 },
                new RelatedComponent() { Name = "TubeInterface5", BitOffset = 8 },
                new RelatedComponent() { Name = "TubeInterface6", BitOffset = 9 },
                new RelatedComponent() { Name = "AuxBoard", BitOffset = 10 },
                new RelatedComponent() { Name = "ExtBoard", BitOffset = 11 },
                new RelatedComponent() { Name = "ControlBox", BitOffset = 12 },
                new RelatedComponent() { Name = "WorkStation", BitOffset = 13, IsChecked = true, IsEnabled = false}
            };

        }

        public void InitializeModuleDirectory()
        {
            if (!Directory.Exists(ModuleDataDirectoryPath))
            {
                Directory.CreateDirectory(ModuleDataDirectoryPath);
            }    
        }

        #endregion

        #region Fields

        /// <summary>
        /// 子模块数据目录地址 
        /// </summary>
        public string ModuleDataDirectoryPath => Path.Combine(_hardwareTestConfig.DataDirectoryPath, ComponentDefaults.DataAcquisition);

        /// <summary>
        /// 灰度记录目录地址 
        /// </summary>
        public string GrayLevelRecordDirectoryPath => Path.Combine(ModuleDataDirectoryPath, "GrayLevelRecord");

        #endregion

        #region Properties

        #region Left Acquisition Parameters

        //采集参数集合 
        public DataAcquisitionParameters XDataAcquisitionParameters { get; set; } = null!;
        //运动控制参数集合
        public MotionControlParameters XMotionControlParameters { get; set; } = null!;
        //关注子节点集合
        public ObservableCollection<RelatedComponent> RelatedComponents { get; set; } = null!;
        //采集状态 
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartDataAcquisition))]
        private DataAcquisitionStatus currentDataAcquisitionStatus = DataAcquisitionStatus.NormalStop;
        //可采集标志 
        public bool CanStartDataAcquisition => CurrentDataAcquisitionStatus == DataAcquisitionStatus.NormalStop;
        //当前CTBox连接状态 
        public ConnectionStatus CurrentCTBoxConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
        //当前门状态 
        public DoorStatus CurrentDoorStatus { get; set; } = DoorStatus.Open;
        //当前探测器状态 
        public DetectorStatus CurrentDetectorStatus { get; set; } = DetectorStatus.NotReady;

        partial void OnCurrentDataAcquisitionStatusChanged(DataAcquisitionStatus oldValue, DataAcquisitionStatus newValue)
        {
            DispatcherWrapper.Invoke(() =>
            {
                StartDataAcquisitionCommand.NotifyCanExecuteChanged();
            });
        }

        #endregion

        #region Right-Medium ImageControl

        /// <summary>
        /// 图像控件Manager
        /// </summary>
        public ImageViewerManager ImageViewerManager { get; set; } = null!;
        /// <summary>
        /// 数据加载进度
        /// </summary>
        [ObservableProperty]
        private int dataLoadProgress;
        /// <summary>
        /// 数据加载进度的显隐
        /// </summary>
        [ObservableProperty]
        private bool dataLoadProgressVisibility; 
        /// <summary>
        /// 当前载入数据
        /// </summary>
        public IEnumerable<RawData>? LoadedData => ImageViewerManager.RawDataPool;
        /// <summary>
        /// 是否数据已载入
        /// </summary>
        public bool IsDataLoaded => LoadedData is not null && LoadedData.Count() > 0;

        #endregion

        #region Right-Bottom TabControl Related

        /******************************** Console ********************************/

        [ObservableProperty]
        private string consoleMessage = string.Empty;

        /************* XRaySource kV/mA/ms/HeatCapacity/OilTemperature ***********/

        public ObservableCollection<XRayOriginSource> XRaySources { get; set; } = null!;

        /***************************** Table & Gantry Status *****************************/

        [ObservableProperty]
        private TableSource xTableSource = null!;

        [ObservableProperty]
        private GantrySource xGantrySource = null!;

        /****************************** AcquisitionCard Status ******************************/

        public ObservableCollection<AcquisitionCardSource> AcquisitionCardSources { get; set; } = null!;

        /****************************** Detecor Temperature & Status ******************************/

        [ObservableProperty]
        private bool ignoreDetectorTemperature = false;

        public ObservableCollection<DetectorSource> DetectorSources { get; set; } = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSetTemperatureDataSaveInterval))]
        [NotifyCanExecuteChangedFor(nameof(SetTemperatureDataSaveIntervalCommand))]
        private bool canSaveDetectorTemperatureData = false;

        public bool CanSetTemperatureDataSaveInterval => !CanSaveDetectorTemperatureData;

        [ObservableProperty]
        private float temperatureDataSaveInterval = 1f;

        private System.Timers.Timer DetectorTemperatureDataSaveTimer { get; set; } = null!;

        private volatile bool hitTheTimerTickToSave = false;

        /****************************** Collimator ******************************/

        public ObservableCollection<CollimatorSource> CollimatorSources { get; set; } = null!;

        #endregion

        #endregion

        #region Events

        #region Events Registration

        /// <summary>
        /// 注册Proxy事件
        /// </summary>
        private void RegisterProxyEvents()
        {
            CurrentCTBoxConnectionStatus = 
                DataAcquisitionProxy.Instance.CTBoxConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;

            DataAcquisitionProxy.Instance.DeviceConnectionChanged += DataAcquisition_CTBoxConnectionChanged;
            DataAcquisitionProxy.Instance.CycleStatusChanged += DataAcquisition_CycleStatusChanged;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged += DataAcquisition_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.EventDataReceived += DataAcquisition_EventDataReceived;
            DataAcquisitionProxy.Instance.RawImageSaved += DataAcquisition_RawDataSaved;

            MotionControlProxy.Instance.CollimatorPositionUpdated += DataAcquisition_CollimatorPositionUpdated;
            MotionControlProxy.Instance.CollimatorArrived += DataAcquisition_CollimatorArrived;

        }
        /// <summary>
        /// 注册消息打印事件
        /// </summary>
        private void RegisterMessagePrinterEvents()
        {
            _messagePrintService.OnConsoleMessageChanged += MessagePrintService_OnConsoleMessageChanged;
        }
        /// <summary>
        /// 取消注册Proxy事件
        /// </summary>
        private void UnRegisterProxyEvents()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DataAcquisition}] Un-register DataAcquisitionProxy events.");
            DataAcquisitionProxy.Instance.DeviceConnectionChanged -= DataAcquisition_CTBoxConnectionChanged;
            DataAcquisitionProxy.Instance.CycleStatusChanged -= DataAcquisition_CycleStatusChanged;
            DataAcquisitionProxy.Instance.RealTimeStatusChanged -= DataAcquisition_RealTimeStatusChanged;
            DataAcquisitionProxy.Instance.EventDataReceived -= DataAcquisition_EventDataReceived;
            DataAcquisitionProxy.Instance.RawImageSaved -= DataAcquisition_RawDataSaved;

            MotionControlProxy.Instance.CollimatorPositionUpdated -= DataAcquisition_CollimatorPositionUpdated;
            MotionControlProxy.Instance.CollimatorArrived -= DataAcquisition_CollimatorArrived;
        }
        /// <summary>
        /// 取消注册消息打印事件
        /// </summary>
        private void UnRegisterMessagePrinterEvents()
        {
            _messagePrintService.OnConsoleMessageChanged -= MessagePrintService_OnConsoleMessageChanged;
        }

        #endregion

        #region Cycle Status

        private void DataAcquisition_CycleStatusChanged(object sender, CycleStatusArgs args)
        {
            // 获取DeviceSystem
            var deviceSystem = args.Device;
            //更新门状态 
            this.CurrentDoorStatus = args.Device.DoorClosed ? DoorStatus.Closed : DoorStatus.Open;
            //更新射线源状态 
            ComponentStatusHelper.UpdateXRaySourceStatusByCycle(this.XRaySources, deviceSystem);
            //更新扫描床状态 
            ComponentStatusHelper.UpdateTableStatusByCycle(this.XTableSource, deviceSystem);
            //更新扫描架状态 
            ComponentStatusHelper.UpdateGantryStatusByCycle(this.XGantrySource, deviceSystem);
            //更新采集卡状态 
            ComponentStatusHelper.UpdateAcquisitionCardStatusByCycle(this.AcquisitionCardSources, deviceSystem);
            //更新探测器状态 
            ComponentStatusHelper.UpdateDetectorStatusByCycle(this.DetectorSources, deviceSystem);
            //存储温度数据 
            this.SaveTemperatureDataByCycle(deviceSystem);
        }

        /// <summary>
        /// 存储温控数据
        /// </summary>
        /// <param name="deviceSystem"></param>
        private void SaveTemperatureDataByCycle(DeviceSystem deviceSystem) 
        {
            var detector = deviceSystem.Detector;
            //每1s存储一次温度数据 
            if (hitTheTimerTickToSave)
            {
                detectorTemperatureDataRepository.Add(new DetectorTemperatureData()
                {
                    TimeStamp = DateTime.Now,
                    Sources = this.DetectorSources.ToArray()
                });
                //重置标志位 
                this.hitTheTimerTickToSave = false;
            }
            //根据传输板状态来综合判定探测器Ready状态 
            for (int i = 0; i < detector.DetectorModules.Length; i++)
            {
                var transmissionBoardStatus = detector.DetectorModules[i].TransmissionBoardStatus;
                //所有状态为Normal时，才判定为Ready 
                if (transmissionBoardStatus == PartStatus.Normal)
                {
                    this.CurrentDetectorStatus = DetectorStatus.Ready;
                }
                //只要有一处不为Normal,判定为NotReady,跳出 
                else
                {
                    this.CurrentDetectorStatus = DetectorStatus.NotReady;
                    break;
                }
            }
        }

        #endregion

        #region CTBox Connection

        private void DataAcquisition_CTBoxConnectionChanged(object sender, ConnectionStatusArgs args)
        {
            CurrentCTBoxConnectionStatus = args.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;

            _messagePrintService.PrintLoggerInfo(
                $"[{ComponentDefaults.DataAcquisition}] Current CTBox connection status: [{Enum.GetName(CurrentCTBoxConnectionStatus)}].");
        }

        #endregion

        #region Event Data

        /// <summary>
        /// 事件数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DataAcquisition_EventDataReceived(object? sender, EventDataEventArgs args)
        {
            switch (args.EventDataInfo.Type)
            {
                case EventDataType.DoseInfo: DoseInfoEventDataResolver(args.EventDataInfo.Data); break;
                case EventDataType.TableArrived: break;
            }
        }

        /// <summary>
        /// 剂量信息事件数据解析
        /// </summary>
        /// <param name="data"></param>
        private void DoseInfoEventDataResolver(byte[] data)
        {
            if (data is null) return;
            //拆分地址与内容 
            var pair = EventDataHelper.ParseEventData(data);
            //匹配地址 
            bool result = _eventDataAddressService.MatchDoseInfoAddress(pair.address, out XRaySourceDose? doseInfo);
            //处理 
            if (result)
            {
                if (doseInfo is not null)
                {
                    //更新值(真实值为接收值除以10) 
                    doseInfo.Value = (float)pair.content / 10;
                    //更新页面值 
                    XRaySourceHelper.UpdateXRaySourceDose(this.XRaySources, doseInfo);
                }
                else
                {
                    _messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.DataAcquisition}] Matched doseinfo address: {pair.address.ToString("X")} return null DoseInfo instance.");
                }
            }
            else
            {
                _messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.DataAcquisition}] Mismatched doseinfo address: {pair.address.ToString("X")}.");
            }
        }

        #endregion

        #region Realtime Status

        private void DataAcquisition_RealTimeStatusChanged(object sender, RealtimeEventArgs args)
        {
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Realtime status: [{Enum.GetName(args.Status)}].");
        }

        #endregion

        #region RawData Saved

        /// <summary>
        /// 生数据保存事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void DataAcquisition_RawDataSaved(object sender, RawImageSavedEventArgs args)
        {
            var totalFrames = XDataAcquisitionParameters.ExposureParameters.TotalFrames;

            int coef = totalFrames switch
            {
                >= 100 and < 1000 => 20,
                >= 1000 and < 5000 => 200,
                _ => 1000
            };

            if (args.FinishCount % coef == 0 && !args.IsFinished) 
            {
                _messagePrintService.PrintLoggerInfo($"Rawdata received {args.FinishCount} / {totalFrames}");
            }
            
            if (args.IsFinished) 
            {
                _messagePrintService.PrintLoggerInfo($"Rawdata received finishend, {args.FinishCount} / {totalFrames}");

                await LoadRawDataCommonAsync(args.Directory);

                CurrentDataAcquisitionStatus = DataAcquisitionStatus.NormalStop;
            }
        }

        #endregion

        #endregion

        #region Data Acquisition Control

        /// <summary>
        /// 开始数据采集
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanStartDataAcquisition))]
        private void StartDataAcquisition()
        {
            if (CurrentCTBoxConnectionStatus is not ConnectionStatus.Connected)
            {
                _messagePrintService.PrintLoggerWarn(
                    $"[{ComponentDefaults.DataAcquisition}] CTBox is now disconnected, please check CTBox status!");

                return;
            }
            if (CurrentDoorStatus is not DoorStatus.Closed)
            {
                _messagePrintService.PrintLoggerInfo(
                    $"[{ComponentDefaults.DataAcquisition}] Door is not closed, please close the door!");

                return;
            }

            const string driveLetter = "F:";
            const int thresholdGigabytes = 50;

            var checkResult = DiskSpaceCheckHelper.CheckDiskSpaceBySize(driveLetter, thresholdGigabytes);
            if (!checkResult.IsSufficient)
            {
                _messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.DataAcquisition}] {checkResult.Message}");
                var userMessage = $"Disk {driveLetter} free space is less than {thresholdGigabytes}GB. Please clear space.";
                DialogService.Instance.ShowWarning(userMessage);

                return;
            }

            try
            {
                ImageViewerManager.ImageViewer.ClearView();

                bool result = StartDataAcquisitionCommon();

                if (result)
                {
                    CurrentDataAcquisitionStatus = DataAcquisitionStatus.AcquiringData;
                }
                else
                {
                    _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] Failed to start data acquisition.");
                }
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] Unhandled exception, [Stack]: {ex}.");
            }
        }

        /// <summary>
        /// 停止数据采集
        /// </summary>
        [RelayCommand]
        private void StopDataAcquisition()
        {
            CurrentDataAcquisitionStatus = DataAcquisitionStatus.NormalStop;
            
            StopDataAcquisitionCommon();
        }

        /// <summary>
        /// 开始采集Common
        /// </summary>
        /// <returns></returns>
        private bool StartDataAcquisitionCommon()
        {
            //开始配置
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to configure data acquisition.");
            //根据Related Components计算值
            var relatedComponents = RelatedComponentsHelper.ParseRelatedComponentsValue(RelatedComponents);
            XDataAcquisitionParameters.ExposureParameters.ExposureRelatedChildNodesConfig = relatedComponents;
            //参数转换 
            var proxyParameters = this.XDataAcquisitionParameters.ToProxyParam();
            //配置参数 
            var configureResponse = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(proxyParameters, IgnoreDetectorTemperature);
            //校验 
            if (!configureResponse.Status)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] " +
                    $"Failed to configure data acquisition, error codes - [{configureResponse.ErrorCode}].");

                DialogService.Instance.ShowErrorCode(configureResponse.ErrorCode);

                return false;
            }
            //配置结束
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Data acquisition has been configured.");
            //开始采集
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to start data acquisition.");
            //启动采集
            var startResponse = DataAcquisitionProxy.Instance.StartDataAcquisition(proxyParameters);
            //校验 
            if (!startResponse.Status)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] " +
                    $"Failed to start data acquisition, error codes - [{startResponse.ErrorCode}].");

                DialogService.Instance.ShowErrorCode(startResponse.ErrorCode);

                return false;
            }
            //采集结束
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Data acquisition has been started.");

            return true;
        }

        /// <summary>
        /// 停止采集Common
        /// </summary>
        private bool StopDataAcquisitionCommon()
        {
            //停止采集
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to stop data acquisition.");
            //停止开始
            var stopResponse = DataAcquisitionProxy.Instance.StopDataAcquisition();
            //校验 
            if (!stopResponse.Status)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] " +
                    $"Failed to stop data acquisition, error codes - [{stopResponse.ErrorCode}].");

                DialogService.Instance.ShowErrorCode(stopResponse.ErrorCode);

                return false;
            }
            //停止结束
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Data acquisition has been stopped.");

            return true;
        }

        #endregion

        #region Temperature Control

        /// <summary>
        /// 开启自由模式
        /// </summary>
        [RelayCommand]
        private void StartFreeMode() 
        {
            try
            {
                var result = DataAcquisitionProxy.Instance.StartAcqusitionFreeMode();

                if (result.Status == CommandStatus.Success)
                {
                    _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Free mode has been started.");
                }
                else 
                {
                    _messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.DataAcquisition}] Something wrong when start free mode.");
                }
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] Something wrong when start free mode, [Stack]: {ex}");
            }
        }

        /// <summary>
        /// 停止自由模式
        /// </summary>
        [RelayCommand]
        private void StopFreeMode() 
        {
            try
            {
                var result = DataAcquisitionProxy.Instance.StopAcqusitionFreeMode();
                //结果判定 
                if (result.Status == CommandStatus.Success)
                {
                    _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Free mode has been stopped.");
                }
                else
                {
                    _messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.DataAcquisition}] Something wrong when stop free mode.");
                }
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] Something wrong when stop free mode, [Stack]: {ex}");
            }
        }

        /// <summary>
        /// 初始化DataSaveTimer
        /// </summary>
        private void InitializeDetectorTemperatureDataSaveTimer()
        {
            if (DetectorTemperatureDataSaveTimer is null)
            {
                DetectorTemperatureDataSaveTimer = new System.Timers.Timer() { Interval = (int)(TemperatureDataSaveInterval * 1000) };
                DetectorTemperatureDataSaveTimer.Elapsed += (sender, args) =>
                {
                    hitTheTimerTickToSave = true;
                };
                _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] DetectorTemperatureDataSaveTimer has been initialized.");
            }
        }

        /// <summary>
        /// 启动DataSaveTimer
        /// </summary>
        private void StartDetectorTemperatureDataSaveTimer()
        {
            if (DetectorTemperatureDataSaveTimer == null) return;
            //启动 
            DetectorTemperatureDataSaveTimer.Start();
            //记录 
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] DetectorTemperatureDataSaveTimer has been started.");
        }

        /// <summary>
        /// 停止DataSaveTimer
        /// </summary>
        private void StopDetectorTemperatureDataSaveTimer()
        {
            if (DetectorTemperatureDataSaveTimer == null) return;

            DetectorTemperatureDataSaveTimer.Stop();
 
            hitTheTimerTickToSave = false;

            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] DetectorTemperatureDataSaveTimer has been stopped.");
        }

        /// <summary>
        /// 数据记录开关切换
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnCanSaveDetectorTemperatureDataChanged(bool oldValue, bool newValue)
        {
            //启动温控数据记录 
            if (newValue)
            {
                //初始化Timer 
                this.InitializeDetectorTemperatureDataSaveTimer();
                //启动Timer 
                this.StartDetectorTemperatureDataSaveTimer();
            }
            //关闭温控数据记录 
            else
            {
                //关闭Timer 
                this.StopDetectorTemperatureDataSaveTimer();
            }
        }

        /// <summary>
        /// 设置温度谁保存间隔
        /// </summary>
        [RelayCommand(CanExecute = (nameof(CanSetTemperatureDataSaveInterval)))]
        private void SetTemperatureDataSaveInterval() 
        {
            if (this.TemperatureDataSaveInterval <= 0) 
            {
                //重置为1 
                this.TemperatureDataSaveInterval = 1;
                //显示记录 
                _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Invalid data save interval {this.TemperatureDataSaveInterval}, reset as default value 1.");
            }
            //初始化 
            this.InitializeDetectorTemperatureDataSaveTimer();
            //设置 
            this.DetectorTemperatureDataSaveTimer.Interval = (int)(TemperatureDataSaveInterval * 1000);
        }

        #endregion

        #region Gantry Control

        [RelayCommand]
        private void StartMoveGantry()
        {
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to start gantry move.");

            var response = MotionControlProxy.Instance.StartMoveGantry(XMotionControlParameters.GantryParameters.ToProxyGantryParams());

            _messagePrintService.PrintResponse(response);
        }

        [RelayCommand]
        private void StopMoveGantry()
        {
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to stop gantry move.");

            var response = MotionControlProxy.Instance.StopMoveGantry();

            _messagePrintService.PrintResponse(response);
        }

        #endregion

        #region Collimator Control

        private void DataAcquisition_CollimatorPositionUpdated(object? sender, IEnumerable<CollimatorPositionInfo> collimatorPositionInfos)
        {
            UpdateCurrentCollimatorPositions(collimatorPositionInfos);
        }

        private void UpdateCurrentCollimatorPositions(IEnumerable<CollimatorPositionInfo> collimatorPositionInfos)
        {
            foreach (var collimatorPositionInfo in collimatorPositionInfos)
            {
                var indexInArray = (int)collimatorPositionInfo.Index - 1;

                CollimatorSources[indexInArray].FrontBladeMoveStep = collimatorPositionInfo.FrontBlade;
                CollimatorSources[indexInArray].RearBladeMoveStep = collimatorPositionInfo.RearBlade;
                CollimatorSources[indexInArray].BowtieMoveStep = collimatorPositionInfo.Bowtie;
            }
        }

        private void UpdateCollimatorPositionByReadRegister()
        {
            var result = MotionControlProxy.Instance.TryGetCurrentCollimatorPositions(out var collimatorPositionInfos);

            if (result.Status)
            {
                UpdateCurrentCollimatorPositions(collimatorPositionInfos!);
            }
        }

        private void DataAcquisition_CollimatorArrived(object? sender, CollimatorArrviedInfo collimatorArrviedInfo)
        {
            _collimatorArrivedSignal.Release();
        }

        private SemaphoreSlim _collimatorArrivedSignal = new SemaphoreSlim(0, 1);

        [RelayCommand]
        private async Task ExecuteCollimatorStepMoveAsync()
        {
            var proxyModel = XMotionControlParameters.CollimatorParameters.ToProxyCollimatorParams();
            var response = MotionControlProxy.Instance.MoveCollimator(proxyModel);
            if (!response.Status) 
            {
                _messagePrintService.PrintLoggerError(response.Message);
            }
            await _collimatorArrivedSignal.WaitAsync(TimeSpan.FromSeconds(3));
        }

        #endregion

        #region Table Control

        [RelayCommand]
        private void StartMoveTable()
        {         
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to start table move.");
            //移动扫描床
            var response = MotionControlProxy.Instance.StartMoveTable(XMotionControlParameters.TableParameters.ToProxyTableParams());
            //打印消息
            _messagePrintService.PrintResponse(response);
        }

        [RelayCommand]
        private void StopMoveTable()
        {
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] Prepare to stop table move.");
            //移动扫描床
            var response = MotionControlProxy.Instance.StopMoveTable();
            //打印消息
            _messagePrintService.PrintResponse(response);
        }

        #endregion

        #region Image Load & Image Save

        /// <summary>
        /// 载入单帧数据文件（以多个文件路径形式加载）
        /// </summary>
        [RelayCommand]
        private async Task LoadImagesAsync()
        {
            var openFileDialog = new OpenFileDialog() 
            {
                Multiselect = true,
                Title = "Please select raw data",
                Filter = "生数据文件(*.raw)|*.raw"
            };

            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult == true) 
            {
                await LoadRawDataCommonAsync(openFileDialog.FileNames);
            }
        }

        /// <summary>
        /// 载入生数据序列（以文件夹形式加载）
        /// </summary>
        [RelayCommand]
        private async Task LoadImageSeriesAsync() 
        {
            var folderBrowserDialog = new OpenFolderDialog();

            var dialogResult = folderBrowserDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult == true)
            {
                var result = await LoadRawDataCommonAsync(folderBrowserDialog.FolderName);

                if (!result) 
                {
                    ImageViewerManager.ClearImageViewer();
                }

            }
        }

        /// <summary>
        /// 载入生数据，指定文件夹
        /// </summary>
        private async Task<bool> LoadRawDataCommonAsync(string directory)
        {
            DataLoadProgress = 0;
            DataLoadProgressVisibility = true;

            if (ImageViewerManager.RawDataPool is not null) 
            {
                RawDataReadWriteHelper.Instance.Release();
            }

            var response = await Task.Run(() => RawDataReadWriteHelper.Instance.Read(directory, RawDataReadProgressChanging));

            if (!response.status)
            {
                _messagePrintService.PrintLoggerError($"Failed to read raw data, directory - {directory}");

                DataLoadProgressVisibility = false;

                return false;
            }

            await Task.Delay(100);
            ImageViewerManager.LoadRawDataList(response.data);

            DataLoadProgressVisibility = false;

            return true;
        }

        /// <summary>
        /// 载入生数据，指定生数据路径
        /// </summary>
        private async Task<bool> LoadRawDataCommonAsync(IEnumerable<string> paths) 
        {
            DataLoadProgress = 0;
            DataLoadProgressVisibility = true;

            if (ImageViewerManager.RawDataPool is not null)
            {
                RawDataReadWriteHelper.Instance.Release();
            }

            var response = await Task.Run(() => RawDataReadWriteHelper.Instance.Read(paths, RawDataReadProgressChanging));

            if (!response.status)
            {
                _messagePrintService.PrintLoggerError($"Failed to read raw data by paths.");

                DataLoadProgressVisibility = false;

                return false;
            }

            await Task.Delay(100);
            ImageViewerManager.LoadRawDataList(response.data);
            DataLoadProgressVisibility = false;

            return true;
        }

        /// <summary>
        /// 更新数据载入进度
        /// </summary>
        /// <param name="count"></param>
        /// <param name="total"></param>
        private void RawDataReadProgressChanging(int count, int total)
        {
            DataLoadProgress = Convert.ToInt32((count / (float)total) * 100);
        }
      
        /// <summary>
        /// 将加载数据按单帧保存
        /// </summary>
        [RelayCommand]
        private async Task SaveAsSingleFrameImageAsync() 
        {
            if (ImageViewerManager.CurrentRawDataInfo is null || ImageViewerManager.CurrentScanSeries is null)
            {
                _messagePrintService.PrintLoggerError($"Cached RawDataInfo & ScanSeries is null.");

                return;
            }

            var dialog = new OpenFolderDialog();

            var result = dialog.ShowDialog();

            if (result == true)
            {
                string directory = dialog.FolderName;

                var saveResult = await Task.Run(() =>
                    RawDataSaverWrapper.SaveRawDataList(directory, ImageViewerManager.CurrentRawDataInfo,
                        ImageViewerManager.CurrentScanSeries.SequenceDataList, RawDataSaveType.Single, RawDataSaveProgress));
            }
        }

        /// <summary>
        /// 将加载数据按序列保存
        /// </summary>
        [RelayCommand]
        private async Task SaveAsImageSeriesAsync()
        {
            if (ImageViewerManager.CurrentRawDataInfo is null || ImageViewerManager.CurrentScanSeries is null)
            {
                _messagePrintService.PrintLoggerError($"Cached RawDataInfo & ScanSeries is null.");

                return;
            }

            var dialog = new OpenFolderDialog();

            var result = dialog.ShowDialog();

            if (result == true)
            {
                string directory = dialog.FolderName;

                var saveResult = await Task.Run(() =>
                    RawDataSaverWrapper.SaveRawDataList(directory, ImageViewerManager.CurrentRawDataInfo,
                        ImageViewerManager.CurrentScanSeries.SequenceDataList, RawDataSaveType.Sequence, RawDataSaveProgress));
            }
        }

        /// <summary>
        /// 保存进度反馈
        /// </summary>
        /// <param name="count"></param>
        /// <param name="total"></param>
        private void RawDataSaveProgress(int count, int total)
        {
            if (count == total) 
            {
                _messagePrintService.PrintLoggerError($"Rawdata has been saved successfully, total count - {total}.");
            }
        }

        #endregion
        
        #region Image Actions

        /// <summary>
        /// 执行切图
        /// </summary>
        [RelayCommand]
        private void ExecuteImageCut()
        {
            var response = ImageViewerManager.CutImage(true);

            _messagePrintService.PrintResponse(response);

            if (response.Status) 
            {
                SaveGrayLevelCsvFile();
            }

        }

        /// <summary>
        /// 执行图排序
        /// </summary>
        [RelayCommand]
        private void ExecuteImageSort()
        {
            DialogHelper.ShowDialog<DataAcquisitionImageSortView>(1035, 750);
        }

        /// <summary>
        /// 画横线
        /// </summary>
        [RelayCommand]
        private void DrawHorizontalLineByAxisYValue()
        {
            DialogHelper.ShowDialog<DrawHorizontalLineView>(420, 140);
        }

        /// <summary>
        /// 接收Y坐标画横线
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        private void DrawHorizontalLine(object recipient, DrawHorizontalLineMessage message)
        {
            double[] blueColor = [0, 0, 255];

            if (!IsDataLoaded)
            {
                _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.DataAcquisition}] No valid data when [DrawHorizontalLine].");

                return;
            }

            ImageViewerManager.ImageViewer.DrawHorizontalLine(message.axisYValue, blueColor);
        }

        /// <summary>
        /// 保存灰度文件
        /// </summary>
        private void SaveGrayLevelCsvFile()
        {
            if (LoadedData is null) 
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.DataAcquisition}] Loaded data is null.");

                return;
            }

            if (!Directory.Exists(GrayLevelRecordDirectoryPath))
            {
                Directory.CreateDirectory(GrayLevelRecordDirectoryPath);
            }

            string grayLevelFilePath = Path.Combine(GrayLevelRecordDirectoryPath, $"GrayLevel_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv");

            _messagePrintService.PrintLoggerInfo($"Prepare to calculate gray level csv file, save path - [{grayLevelFilePath}].");

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(File.Open(grayLevelFilePath, FileMode.OpenOrCreate)))
                {
                    streamWriter.AutoFlush = true;

                    streamWriter.WriteLine("Index,SourceNumber,ImageNumber,TablePosition,GantryPosition,GrayLevel,Slope0-1,Slope1-1");
 
                    ImageRectangle imageRectangle = new ImageRectangle();
                    imageRectangle.left = LoadedData.First().ImageSizeX / 2 - (int)(_dataAcquisitionConfig.GrayLevelCalculateWidth / 2);
                    imageRectangle.top = LoadedData.First().ImageSizeY / 2 - (int)(_dataAcquisitionConfig.GrayLevelCalculateHeight / 2);
                    imageRectangle.right = imageRectangle.left + (int)_dataAcquisitionConfig.GrayLevelCalculateWidth;
                    imageRectangle.bottom = imageRectangle.top + (int)_dataAcquisitionConfig.GrayLevelCalculateHeight;

                    for (int i = 0; i < LoadedData.Count(); i++)
                    {
                        var rawdata = LoadedData.ElementAt(i);

                        //计算范围和质量指标 
                        (ImageDynamicRange imageDynamicRange, ImageQualityIndex ImageQualityIndex)
                            = ImageUniversalHelper.CalculateImageDynamicRangeAndQualityIndex(imageRectangle, rawdata);

                        streamWriter.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                            i + 1, rawdata.SourceId, rawdata.FrameNoInSeries,rawdata.TablePosition, rawdata.GantryAngle, ImageQualityIndex.average, rawdata.Slope0[0], rawdata.Slope1[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError(
                    $"[{nameof(DataAcquisitionTestingViewModel)}][{nameof(SaveGrayLevelCsvFile)}] Unhandled exception, [Stack]: {ex}.");
            }

            _messagePrintService.PrintLoggerInfo($"Gray level csv file has been saved successfully, path: [{grayLevelFilePath}].");
        }

        #endregion

        #region Message Printer

        private void MessagePrintService_OnConsoleMessageChanged(object? sender, string message)
        {
            this.ConsoleMessage = message;
        }

        [RelayCommand]
        private void ClearConsoleMessage()
        {
            _messagePrintService.Clear();
        }

        #endregion

        #region Navigation

        public override void BeforeNavigateToCurrentPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DataAcquisition}] Enter [Data Acquisition] testing page.");
            //更新当前限束器位置
            UpdateCollimatorPositionByReadRegister();
            //注册Proxy事件 
            RegisterProxyEvents();
            //注册消息打印事件 
            RegisterMessagePrinterEvents();
            //注册消息
            ImageViewerManager.RegisterMessages();
        }

        public override void BeforeNavigateToOtherPage()
        {
            //取消注册Proxy事件 
            UnRegisterProxyEvents();
            //取消注册消息打印事件 
            UnRegisterMessagePrinterEvents();
            //取消注册消息
            ImageViewerManager.UnRegisterAllMessage();
            //停止温度数据采集 
            CanSaveDetectorTemperatureData = false;
            //记录 
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DataAcquisition}] Leave [Data Acquisition] testing page.");
        }

        #endregion

    }
}
