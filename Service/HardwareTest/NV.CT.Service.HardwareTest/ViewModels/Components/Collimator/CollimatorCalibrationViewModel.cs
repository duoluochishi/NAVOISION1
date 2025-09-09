using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.Collimator;
using NV.CT.FacadeProxy.Models.MotionControl.Collimator;
using NV.CT.FacadeProxy.Models.MotionControl.Table;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.LibraryCallers;
using NV.CT.Service.HardwareTest.Categories;
using NV.CT.Service.HardwareTest.Models.Components.Collimator;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.ErrorCodes;
using NV.CT.Service.HardwareTest.Share.Fields;
using NV.CT.Service.HardwareTest.Share.Utils;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.Collimator
{
    public partial class CollimatorCalibrationViewModel : NavigationViewModelBase, IModuleDirectory
    {
        private readonly ILogService _logService;
        private readonly IMessagePrintService _messagePrintService;
        private readonly CollimatorConfigOptions _collimatorConfig;
        private readonly HardwareTestConfigOptions _hardwareTestConfig;

        public CollimatorCalibrationViewModel(
            ILogService logService,
            IMessagePrintService messagePrintService,
            IOptions<CollimatorConfigOptions> collimatorConfigOptions,
            IOptions<HardwareTestConfigOptions> hardwareTestOptions)
        {
            this._logService = logService;
            this._messagePrintService = messagePrintService;
            this._collimatorConfig = collimatorConfigOptions.Value;
            this._hardwareTestConfig = hardwareTestOptions.Value;

            InitializeProperties();
            InitializeModuleDirectory();
        }

        #region Initialize

        private void InitializeProperties()
        { 
            ScanParameters = new();           
            ScanParameters.ExposureParameters.KVs = [90,0,0,0,0,0,0,0];
            ScanParameters.ExposureParameters.GantryVelocity = 0;
            ScanParameters.DetectorParameters.PreOffsetEnable = CommonSwitch.Enable;

            InitializeCollimatorOpeningTypes();
            InitializeCollimatorCalibrationMotorTypes();

            CollimatorSources =
            [
                .. Enumerable.Range(1, _collimatorConfig.CollimatorSourceCount).Select(i => new CollimatorSource 
                { 
                    Name = $"{i.ToString("00")}" 
                }),
            ];

            IterativeResultsCache =
            [
                .. Enumerable.Range(1, _collimatorConfig.CollimatorSourceCount).Select(i => new CollimatorCalibrationIterativeResult()
                {
                    CollimatorName = i.ToString("00")
                }),
            ];


            FrontBladeTargetPosition = _collimatorConfig.MaxFrontBladeMoveStep;
            RearBladeTargetPosition = _collimatorConfig.MinMoveStep;
            BowtieTargetPosition = _collimatorConfig.MaxBowtie;
        }

        private void InitializeCalibrationLibrary()
        {
            try
            {
                var response = CollimatorCalibrationLibraryCaller.Initialize(ImageCutXmlConfig.Instance.XmlConfigFilePath);
                if (!response.status)
                {
                    _messagePrintService.PrintLoggerError($"[{ComponentDefaults.CollimatorComponent}] Failed to initialize collimator calibration library.");
                }
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError(
                    $"[{ComponentDefaults.CollimatorComponent}][{nameof(InitializeCalibrationLibrary)}] Unhandled exception, [Stack]: {ex}.");
            }
        }

        private void InitializeCollimatorOpeningTypes()
        {
            CollimatorOpeningTypes =
            [
                new(CollimatorOpenMode.NearCenter, CollimatorOpenWidth.FullOpen),        
                new(CollimatorOpenMode.NearSmallAngle, CollimatorOpenWidth.Rows256),
                new(CollimatorOpenMode.NearSmallAngle, CollimatorOpenWidth.Rows242),
                new(CollimatorOpenMode.NearSmallAngle, CollimatorOpenWidth.Rows210),
                new(CollimatorOpenMode.NearSmallAngle, CollimatorOpenWidth.Rows144),
                new(CollimatorOpenMode.NearSmallAngle, CollimatorOpenWidth.Rows128),
                new(CollimatorOpenMode.NearSmallAngle, CollimatorOpenWidth.Rows64),       
                new(CollimatorOpenMode.NearCenter, CollimatorOpenWidth.Rows128),
                new(CollimatorOpenMode.NearCenter, CollimatorOpenWidth.Rows64)
            ];
        }

        private void InitializeCollimatorCalibrationMotorTypes()
        {
            CollimatorCalibrationMotorTypes = new()
            {
                CollimatorMotorType.FrontBlade, CollimatorMotorType.RearBlade
            };
        }

        public void InitializeModuleDirectory()
        {
            FileUtils.EnsureDirectoryPath(ModuleDataDirectoryPath);
        }

        #endregion

        #region Fields

        /// <summary>
        /// 库初始化标志
        /// </summary>
        private bool _libraryInitializeFlag = false;
        /// <summary>
        /// 限束器到位超时时间 - 5s
        /// </summary>
        private readonly TimeSpan _collimatorArrivedTimeout = TimeSpan.FromSeconds(5);
        /// <summary>
        /// 生数据保存超时时间 - 3min
        /// </summary>
        private readonly TimeSpan _rawdataSavedTimeout = TimeSpan.FromMinutes(3);
        /// <summary>
        /// 全开口校准表名称
        /// </summary>
        public const string FullOpenCalibrationTableName = "288 × 0.165.json";
        /// <summary>
        /// 模块文件夹路径
        /// </summary>
        public string ModuleDataDirectoryPath => Path.Combine(_hardwareTestConfig.DataDirectoryPath, ComponentDefaults.CollimatorComponent);

        #endregion

        #region Properties

        #region Scan Parameters

        /// <summary>
        /// 扫描参数 
        /// </summary>
        [ObservableProperty]
        public DataAcquisitionParameters scanParameters = null!;
        /// <summary>
        /// 实时状态
        /// </summary>
        public RealtimeStatus CurrentRealtimeStatus { get; set; }

        #endregion

        #region Collimator Control And Realtime Positions

        /// <summary>
        /// 前端限束器源 
        /// </summary>
        public ObservableCollection<CollimatorSource> CollimatorSources { get; set; } = null!;
        /// <summary>
        /// 当前限束器位置信息
        /// </summary>
        public List<CollimatorPositionInfo> CurrentCollimatorPositions => MotionControlProxy.Instance.CurrentCollimatorPositions;

        /// <summary>
        /// 限束器是否在移动
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanOperateCollimator))]
        private volatile bool isCollimatorMoving = false;
        partial void OnIsCollimatorMovingChanged(bool oldValue, bool newValue)
        {
            DispatcherWrapper.Invoke(() =>
            {
                MoveCollimatorSpecificMotorToTargetPositionCommand.NotifyCanExecuteChanged();
                MoveBySelectedCalibrationTableCommand.NotifyCanExecuteChanged();
                MoveBySelectedOpeningCommand.NotifyCanExecuteChanged();
                ConfigureCalibrationTableCommand.NotifyCanExecuteChanged();
            });
        }
        public bool CanOperateCollimator => !IsCollimatorMoving && CurrentCalibrationStatus == CollimatorCalibrationStatus.NormalStop;

        /// <summary>
        /// 限束器源编号
        /// </summary>
        [ObservableProperty]
        private CollimatorSourceIndex selectedCollimatorSourceIndex;
        /// <summary>
        /// 限束器前遮挡电机目标位置 
        /// </summary>
        [ObservableProperty]
        private uint frontBladeTargetPosition;
        /// <summary>
        /// 限束器后遮挡电机目标位置 
        /// </summary>
        [ObservableProperty]
        private uint rearBladeTargetPosition;
        /// <summary>
        /// 限束器波太电机目标位置 
        /// </summary>
        [ObservableProperty]
        private uint bowtieTargetPosition;

        #endregion

        #region Collimator Calibration Properties

        /** 类型集合 **/

        /// <summary>
        /// 限束器开口类型集合 
        /// </summary>
        public ObservableCollection<CollimatorOpenType> CollimatorOpeningTypes { get; set; } = null!;
        /// <summary>
        /// 校准电机类型集合
        /// </summary>
        public ObservableCollection<CollimatorMotorType> CollimatorCalibrationMotorTypes { get; set; } = null!;

        /** 校准开口类型相关 **/

        /// <summary>
        /// 当前限束器校准开口类型Index 
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCalibrationOpeningType))]
        private int currentCalibrationOpeningTypeIndex = 2;
        /// <summary>
        /// 当前被选中校准开口类型
        /// </summary>
        public CollimatorOpenType SelectedCalibrationOpeningType => CollimatorOpeningTypes[CurrentCalibrationOpeningTypeIndex];
        /// <summary>
        /// 当前校准电机类型
        /// </summary>
        [ObservableProperty]
        private CollimatorMotorType currentCalibratingMotorType = CollimatorMotorType.FrontBlade;

        /** 校准状态相关 **/

        /// <summary>
        /// 当前限束器校准状态 
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCalibrationStop))]
        [NotifyPropertyChangedFor(nameof(CanOperateCollimator))]
        private volatile CollimatorCalibrationStatus currentCalibrationStatus = CollimatorCalibrationStatus.NormalStop;
        /// <summary>
        /// 记录变化状态 
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnCurrentCalibrationStatusChanged(CollimatorCalibrationStatus oldValue, CollimatorCalibrationStatus newValue)
        {
            _messagePrintService.PrintLoggerInfo(
                $"Current calibration status changed from [{Enum.GetName(oldValue)}] to [{Enum.GetName(newValue)}].");

            DispatcherWrapper.Invoke(() =>
            {
                StartCalibrationCommand.NotifyCanExecuteChanged();
                ResetCalibrationCommand.NotifyCanExecuteChanged();
                MoveCollimatorSpecificMotorToTargetPositionCommand.NotifyCanExecuteChanged();
                MoveBySelectedCalibrationTableCommand.NotifyCanExecuteChanged();
                MoveBySelectedOpeningCommand.NotifyCanExecuteChanged();
                ConfigureCalibrationTableCommand.NotifyCanExecuteChanged();
            });
        }
        /// <summary>
        /// 校准是否已停止
        /// </summary>
        public bool IsCalibrationStop => CurrentCalibrationStatus == CollimatorCalibrationStatus.NormalStop;

        /** 迭代过程相关 **/

        /// <summary>
        /// 当前迭代轮次 
        /// </summary>
        [ObservableProperty]
        private int currentIterationRound = 1;
        /// <summary>
        /// 当前生数据文件夹
        /// </summary>
        public string CurrentRawDataDirectory { get; set; } = string.Empty;
        /// <summary>
        /// 迭代结果缓存 
        /// </summary>
        public ObservableCollection<CollimatorCalibrationIterativeResult> IterativeResultsCache { get; set; } = null!;
        /// <summary>
        /// 校准表记录缓存
        /// </summary>
        public CollimatorCalibrationTable CalibrationTableCache { get; set; } = null!;

        /// <summary>
        /// 校准完是否配置
        /// </summary>
        [ObservableProperty]
        private bool isConfigureAfterCalibration = true;

        #endregion

        #region Collimator Calibration Table Configuration

        /// <summary>
        /// 当前限束器配置应用开口类型Index 
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedApplicationOpeningType))]
        private int currentApplicationOpeningTypeIndex = 1;
        /// <summary>
        /// 当前被选中配置应用开口类型
        /// </summary>
        public CollimatorOpenType SelectedApplicationOpeningType => CollimatorOpeningTypes[CurrentApplicationOpeningTypeIndex];
        /// <summary>
        /// 当前波太移动选项
        /// </summary>
        [ObservableProperty]
        private BowtieMoveOption currentBowtieMoveOption = BowtieMoveOption.KeepStill;

        #endregion

        #region Console

        //输出信息 
        [ObservableProperty]
        private string consoleMessage = string.Empty;

        #endregion

        #endregion

        #region Events

        #region Events Registration

        /// <summary>
        /// 注册Proxy事件
        /// </summary>
        private void RegisterProxyEvents()
        {           
            DataAcquisitionProxy.Instance.RealTimeStatusChanged += CollimatorCalibration_RealtimeStatusChanged;
            DataAcquisitionProxy.Instance.RawImageSaved += CollimatorCalibration_RawDataSaved;

            MotionControlProxy.Instance.CollimatorPositionUpdated += CollimatorCalibration_CollimatorPositionUpdated;
            MotionControlProxy.Instance.CollimatorArrived += CollimatorCalibration_CollimatorArrived;
            MotionControlProxy.Instance.CollimatorCalibrationTableConfigured += CollimatorCalibration_CollimatorCalibrationTableConfigured;
        }

        /// <summary>
        /// 注册消息打印事件
        /// </summary>
        private void RegisterMessagePrinterEvents()
        {
            _messagePrintService.OnConsoleMessageChanged += MessagePrintService_OnConsoleMessageChanged;
        }

        /// <summary>
        /// 解除Proxy事件
        /// </summary>
        private void UnRegisterProxyEvents()
        {
            DataAcquisitionProxy.Instance.RealTimeStatusChanged -= CollimatorCalibration_RealtimeStatusChanged;
            DataAcquisitionProxy.Instance.RawImageSaved -= CollimatorCalibration_RawDataSaved;

            MotionControlProxy.Instance.CollimatorPositionUpdated -= CollimatorCalibration_CollimatorPositionUpdated;
            MotionControlProxy.Instance.CollimatorArrived -= CollimatorCalibration_CollimatorArrived;
            MotionControlProxy.Instance.CollimatorCalibrationTableConfigured -= CollimatorCalibration_CollimatorCalibrationTableConfigured;
        }

        /// <summary>
        /// 解除消息打印事件
        /// </summary>
        private void UnRegisterMessagePrinterEvents()
        {
            _messagePrintService.OnConsoleMessageChanged -= MessagePrintService_OnConsoleMessageChanged;
        }

        #endregion

        #region View Loaded

        [RelayCommand]
        private void ViewLoaded()
        {
            if (!_libraryInitializeFlag)
            {
                InitializeCalibrationLibrary();

                _libraryInitializeFlag = true;
            }
        }

        #endregion 

        #region Realtime Status

        private SemaphoreSlim _realtimeNormalStopSignal = new SemaphoreSlim(0, 1);

        private void RefreshRealtimeNormalStopSignal() 
        {
            _realtimeNormalStopSignal.Dispose();
            _realtimeNormalStopSignal = new SemaphoreSlim(0, 1);
        }

        private void CollimatorCalibration_RealtimeStatusChanged(object sender, RealtimeEventArgs eventArgs)
        {
            CurrentRealtimeStatus = eventArgs.Status;

            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.CollimatorComponent}] Realtime status - [{Enum.GetName(eventArgs.Status)}].");

            //校准状态下，在NormalScanStopped时，需释放信号量
            if (eventArgs.Status is RealtimeStatus.NormalScanStopped 
                && CurrentCalibrationStatus is not CollimatorCalibrationStatus.NormalStop) 
            {
                if (_realtimeNormalStopSignal.CurrentCount == 0) 
                {
                    _realtimeNormalStopSignal.Release();
                }
            }

            if (eventArgs.Status is RealtimeStatus.Error or RealtimeStatus.EmergencyScanStopped) 
            {
                _calibrationCTS?.Cancel();
            }
        }

        #endregion

        #region RawData Saved

        private async void CollimatorCalibration_RawDataSaved(object sender, RawImageSavedEventArgs eventArgs)
        {
            if (eventArgs.IsFinished)
            {
                if (CurrentCalibrationStatus is not CollimatorCalibrationStatus.NormalStop)
                {
                    bool requried = await _realtimeNormalStopSignal.WaitAsync(TimeSpan.FromSeconds(3));

                    bool dataValidity = ValidateRawDataDirectoryIntegrity(eventArgs.Directory);

                    if (requried && dataValidity) 
                    {
                        CurrentRawDataDirectory = eventArgs.Directory;

                        RawDataSaveFinishedDuringCalibrationSignal.Release();
                    }

                    RefreshRealtimeNormalStopSignal();
                }
            }
        }

        #endregion

        #endregion

        #region Collimator Motion Control

        /// <summary>
        /// 移动单个或所有限束器单电机至同一位置
        /// </summary>
        /// <param name="motorType"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        private bool MoveCollimator(CollimatorSourceIndex sourceIndex, CollimatorMotorType motorType, uint targetPosition)
        {
            _messagePrintService.PrintLoggerInfo(
                $"[{ComponentDefaults.CollimatorComponent}] Prepare to move all collimators's {Enum.GetName(motorType)} to [{targetPosition}].");

            var parameters = new CollimatorBaseCategory()
            {
                SourceIndex = sourceIndex,
                MotorType = motorType,
                MoveStep = targetPosition
            };

            IsCollimatorMoving = true;

            var response = MotionControlProxy.Instance.MoveCollimator(parameters.ToProxyCollimatorParams());

            if (!response.Status)
            {
                IsCollimatorMoving = false;
            }

            _messagePrintService.PrintResponse(response);

            return response.Status;
        }

        /// <summary>
        /// 移动所有限束器单电机至各自指定位置
        /// </summary>
        /// <returns></returns>
        private bool MoveCollimator(CollimatorMotorType motorType, IEnumerable<uint> targetPositions)
        {
            IsCollimatorMoving = true;

            var response = MotionControlProxy.Instance.MoveCollimator(motorType, targetPositions);

            if (!response.Status)
            {
                IsCollimatorMoving = false;
            }

            _messagePrintService.PrintResponse(response);

            return response.Status;
        }

        /// <summary>
        /// 以开口移动限束器
        /// </summary>
        /// <param name="openMode"></param>
        /// <param name="openWidth"></param>
        /// <returns></returns>
        private bool MoveCollimator(CollimatorOpenMode openMode, CollimatorOpenWidth openWidth, BowtieMoveOption bowtieMoveOption) 
        {
            IsCollimatorMoving = true;

            _messagePrintService.PrintLoggerInfo($"Prepare to open move, open mode - {Enum.GetName(openMode)}, open width - {Enum.GetName(openWidth)}.");

            var response = MotionControlProxy.Instance.MoveCollimator(openMode, openWidth, bowtieMoveOption);

            if (!response.Status) 
            {
                IsCollimatorMoving = false;
            }

            _messagePrintService.PrintResponse(response);

            return response.Status;

        }

        #region 单个或所有限束器到同一指定位置的运动控制

        [RelayCommand(CanExecute = nameof(CanOperateCollimator))]
        private async Task MoveCollimatorSpecificMotorToTargetPositionAsync(string inputMotorType) 
        {
            var motorType = inputMotorType switch
            {
                "FrontBlade" => CollimatorMotorType.FrontBlade,
                "RearBlade" => CollimatorMotorType.RearBlade,
                "Bowtie" => CollimatorMotorType.Bowtie,
                _ => throw new ArgumentException()
            };
            var targetPosition = motorType switch 
            {
                CollimatorMotorType.FrontBlade => FrontBladeTargetPosition,
                CollimatorMotorType.RearBlade => RearBladeTargetPosition,
                CollimatorMotorType.Bowtie => BowtieTargetPosition,
                _ => throw new ArgumentException()
            };

            var validateResult = ValidateInputTargetPosition(motorType, targetPosition);

            if (validateResult)
            {
                await Task.Run(() => MoveCollimator(SelectedCollimatorSourceIndex, motorType, targetPosition));
            }
        }

        /// <summary>
        /// 校验输入的目标位置
        /// </summary>
        /// <returns></returns>
        private bool ValidateInputTargetPosition(CollimatorMotorType motorType, uint targetPosition) 
        {
            switch (motorType)
            {
                case CollimatorMotorType.FrontBlade:
                    {
                        if (targetPosition > _collimatorConfig.MaxFrontBladeMoveStep)
                        {
                            _messagePrintService.PrintLoggerError(
                                $"Input motor type - {Enum.GetName(motorType)}'s target position - {targetPosition} is out of range, please check.");

                            return false;
                        }
                    }; 
                    break;
                case CollimatorMotorType.RearBlade:
                    {
                        if (targetPosition > _collimatorConfig.MaxRearBladeMoveStep)
                        {
                            _messagePrintService.PrintLoggerError(
                                $"Input motor type - {Enum.GetName(motorType)}'s target position - {targetPosition} is out of range, please check.");

                            return false;
                        }
                    }; 
                    break;
                case CollimatorMotorType.Bowtie: 
                    {
                        if (targetPosition > _collimatorConfig.MaxBowtie)
                        {
                            _messagePrintService.PrintLoggerError(
                                $"Input motor type - {Enum.GetName(motorType)}'s target position - {targetPosition} is out of range, please check.");

                            return false;
                        }
                    }; 
                    break;
            }

            return true;
        }

        #endregion

        #region 限束器实时位置更新

        private void CollimatorCalibration_CollimatorPositionUpdated(object? sender, IEnumerable<CollimatorPositionInfo> collimatorPositionInfos)
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

        #endregion

        #region 限束器到位状态判断

        private void CollimatorCalibration_CollimatorArrived(object? sender, CollimatorArrviedInfo arrviedInfo)
        {
            IsCollimatorMoving = false;

            switch (arrviedInfo.ArrivedStatus)
            {
                case CollimatorArrivedStatus.None:
                    {
                        _messagePrintService.PrintLoggerError("Received none arrived status.");
                    }
                    break;
                case CollimatorArrivedStatus.StepMoveArrived:
                    {
                        _messagePrintService.PrintLoggerInfo("Step move arrived.");

                        if (MoveByCalibrationTableFlag) 
                        {
                            MoveByCalibrationTableSignal.Release();
                        }

                        if (CurrentCalibrationStatus is not CollimatorCalibrationStatus.NormalStop) 
                        {
                            CollimatorArrivedDuringCalibrationSignal.Release();
                        }
                    }
                    break;
                case CollimatorArrivedStatus.StepMoveNotArrived:
                    {
                        _messagePrintService.PrintLoggerError("Step move not arrived.");
                    }
                    break;
                case CollimatorArrivedStatus.OpenMoveArrived:
                    {
                        _messagePrintService.PrintLoggerInfo("Open move arrived.");
                    }
                    break;
                case CollimatorArrivedStatus.OpenMoveNotArrived:
                    {
                        _messagePrintService.PrintLoggerError("Open move not arrived.");
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Scan Control

        /// <summary>
        /// 开始扫描
        /// </summary>
        /// <returns></returns>
        private bool StartScan()
        {
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.CollimatorComponent}] Prepare to start data acquisition.");

            var proxyParameters = ScanParameters.ToProxyParam();

            var configureResponse = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(proxyParameters);

            if (!configureResponse.Status)
            {
                _messagePrintService.PrintResponse(configureResponse);

                return false;
            }
 
            var startResponse = DataAcquisitionProxy.Instance.StartDataAcquisition(proxyParameters);
 
            if (!startResponse.Status)
            {
                _messagePrintService.PrintResponse(startResponse);

                return false;
            }
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.CollimatorComponent}] Data acquisition started.");

            return true;
        }

        /// <summary>
        /// 停止扫描
        /// </summary>
        private void StopScan()
        {
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.CollimatorComponent}] Prepare to stop data acquisition.");

            var stopResponse = DataAcquisitionProxy.Instance.StopDataAcquisition();
 
            if (!stopResponse.Status)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.CollimatorComponent}] Failed to stop data acquisition."); 
                
                return;
            }
            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.CollimatorComponent}] Data acquisition has been stopped.");
        }

        #endregion

        #region Collimator Calibration Control

        /// <summary>
        /// 开始校准
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(IsCalibrationStop))]
        private async Task StartCalibrationAsync()
        {
            _messagePrintService.PrintLoggerInfo($"Start collimator calibration.");

            const string driveLetter = "F:";
            const int thresholdGigabytes = 50;

            var checkResult = DiskSpaceCheckHelper.CheckDiskSpaceBySize(driveLetter, thresholdGigabytes);
            if (!checkResult.IsSufficient)
            {
                _messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.CollimatorComponent}] {checkResult.Message}");
                var userMessage = $"Disk {driveLetter} free space is less than {thresholdGigabytes}GB. Please clear space.";
                DialogService.Instance.ShowWarning(userMessage);

                return;
            }

            ResetCore();

            bool result = await StartCalibrationAsync(_calibrationCTS.Token);

            var openMode = SelectedCalibrationOpeningType.OpenMode;
            var openWidth = SelectedCalibrationOpeningType.OpenWidth;

            if (result)
            {
                _messagePrintService.PrintLoggerInfo(
                    $"Open Mode - [{Enum.GetName(openMode)}], Open Width - [{Enum.GetName(openWidth)}] " +
                    $"Calibration table has been generated successfully.");
            }
            else
            {
                _messagePrintService.PrintLoggerError(
                    $"Open Mode - [{Enum.GetName(openMode)}], Open Width - [{Enum.GetName(openWidth)}] " +
                    $"calibration failed, status - [{Enum.GetName(CurrentCalibrationStatus)}]");
            }


            await Task.Delay(TimeSpan.FromSeconds(3));

            CurrentCalibrationStatus = CollimatorCalibrationStatus.NormalStop;

            ClearSignal();
            ClearCalibrationCTS();
        }

        /// <summary>
        /// 结束校准
        /// </summary>
        [RelayCommand]
        private void StopCalibration()
        {
            _messagePrintService.PrintLoggerInfo($"Stop collimator calibration.");

            CurrentCalibrationStatus = CollimatorCalibrationStatus.NormalStop;
            _calibrationCTS?.Cancel();

            if (CurrentRealtimeStatus > RealtimeStatus.Standby 
                && CurrentRealtimeStatus < RealtimeStatus.NormalScanStopped) 
            {
                StopScan();
            }

            ResetCore();
        }

        /// <summary>
        /// 重置校准
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsCalibrationStop))]
        private void ResetCalibration()
        {
            ResetCore();
        }

        /// <summary>
        /// 开始校准
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> StartCalibrationAsync(CancellationToken token)
        {
            try
            {
                bool prepahseExecuteResult = await ExecutePrephaseCalibrationAsync(token);
                if (!prepahseExecuteResult)
                {
                    return false;
                }

                bool frontBladeCalibrationResult = await ExecuteFrontBladeCalibrationAsync(token);
                if (!frontBladeCalibrationResult)
                {
                    return false;
                }

                if (SelectedCalibrationOpeningType.OpenMode is CollimatorOpenMode.NearCenter)
                {
                    bool rearBladeCalibrationResult = await ExecuteRearBladeCalibrationAsync(token);
                    if (!rearBladeCalibrationResult)
                    {
                        return false;
                    }
                }

                bool calibrationTableSaveResult = await ExecuteCalibrationTableSaveAndConfigureAsync();
                if (!calibrationTableSaveResult)
                {
                    return false;
                }                
            }
            catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
            {
                _messagePrintService.PrintLoggerWarn($"Calibration cancelled.");
            }

            return true;
        }

        /// <summary>
        /// 执行前置阶段校准
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> ExecutePrephaseCalibrationAsync(CancellationToken token)
        {
            CurrentCalibrationStatus = CollimatorCalibrationStatus.PrephaseCalibrating;

            if (token.IsCancellationRequested)
            {
                return false;
            }

            bool prephaseExecutionResult = await ExecuteFullOpenRawDataValidationAsync(token);

            if (!prephaseExecutionResult)
            {
                CurrentCalibrationStatus = CollimatorCalibrationStatus.PrephaseCalibrationFailed;

                DialogService.Instance.ShowErrorCode(
                    CollimatorTestingErrorCodes.MRS_HardwareTest_CollimatorTesting_CalibrationFailure);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行前遮挡校准
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> ExecuteFrontBladeCalibrationAsync(CancellationToken token)
        {
            CurrentCalibrationStatus = CollimatorCalibrationStatus.FrontBladeCalibrating;

            if (token.IsCancellationRequested)
            {
                return false;
            }

            bool frontBladeCalibrationResult = await ExecuteCalibrationAsync(CollimatorMotorType.FrontBlade, token);

            if (!frontBladeCalibrationResult)
            {
                CurrentCalibrationStatus = CollimatorCalibrationStatus.FrontBladeCalibrationFailed;

                DialogService.Instance.ShowErrorCode(
                    CollimatorTestingErrorCodes.MRS_HardwareTest_CollimatorTesting_CalibrationFailure);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行后遮挡校准
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> ExecuteRearBladeCalibrationAsync(CancellationToken token)
        {
            CurrentCalibrationStatus = CollimatorCalibrationStatus.RearBladeCalibrating;

            if (token.IsCancellationRequested)
            {
                return false;
            }

            bool rearBladeCalibrationResult = await ExecuteCalibrationAsync(CollimatorMotorType.RearBlade, token);

            if (!rearBladeCalibrationResult)
            {
                CurrentCalibrationStatus = CollimatorCalibrationStatus.RearBladeCalibrationFailed;

                DialogService.Instance.ShowErrorCode(
                    CollimatorTestingErrorCodes.MRS_HardwareTest_CollimatorTesting_CalibrationFailure);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行校准表生成与保存
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ExecuteCalibrationTableSaveAndConfigureAsync()
        {
            CurrentCalibrationStatus = CollimatorCalibrationStatus.CalibrationTableSaving;

            if (SelectedCalibrationOpeningType.OpenMode == CollimatorOpenMode.NearSmallAngle)
            {
                if (TryGetAllFullOpenCalibrationTable(out var calibrationTable))
                {
                    CalibrationTableCache.RearBladeMoveSteps = calibrationTable!.RearBladeMoveSteps;
                }
            }

            (bool finalSaveResult, string calibrationFileName) = await SaveCalibrationTableAsync();

            if (!finalSaveResult)
            {
                DialogService.Instance.ShowErrorCode(
                    CollimatorTestingErrorCodes.MRS_HardwareTest_CollimatorTesting_CalibrationFailure);

                CurrentCalibrationStatus = CollimatorCalibrationStatus.CalibrationTableSaveAndConfigureFailed;

                return false;
            }

            if (IsConfigureAfterCalibration) 
            {
                bool configureResult = await ConfigureCalibrationTableCoreAsync(calibrationFileName);
                if (!configureResult) 
                {
                    DialogService.Instance.ShowErrorCode(
                        CollimatorTestingErrorCodes.MRS_HardwareTest_CollimatorTesting_CalibrationFailure);

                    CurrentCalibrationStatus = CollimatorCalibrationStatus.CalibrationTableSaveAndConfigureFailed;

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 保存校准表
        /// </summary>
        /// <returns></returns>
        private async Task<(bool, string)> SaveCalibrationTableAsync()
        {
            try
            {
                string fileName = (SelectedCalibrationOpeningType.OpenWidth == CollimatorOpenWidth.FullOpen) ? FullOpenCalibrationTableName :
                    $"{SelectedCalibrationOpeningType.Name} - {DateTime.Now.ToString("yyyyMMddHHmmss")}.json";
                string resultPath = Path.Combine(ModuleDataDirectoryPath, fileName);

                await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(CalibrationTableCache));

                _messagePrintService.PrintLoggerInfo($"Calibration result has been saved, file path - [{resultPath}]");

                return (true, resultPath);
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(SaveCalibrationTableAsync)}] Unhandled exception, [Stack]: {ex}");

                return (false, string.Empty);
            }
        }

        /// <summary>
        /// 获取全开口校准表内容
        /// </summary>
        private bool TryGetAllFullOpenCalibrationTable(out CollimatorCalibrationTable? calibrationTable)
        {
            try
            {
                var calibrationTablePath = Path.Combine(ModuleDataDirectoryPath, FullOpenCalibrationTableName);
                var content = Task.Run(() => File.ReadAllText(calibrationTablePath)).Result;
                calibrationTable = JsonSerializer.Deserialize<CollimatorCalibrationTable>(content)!;

                return true;
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(TryGetAllFullOpenCalibrationTable)}] Unhandled exception, [Stack]: {ex}.");

                calibrationTable = default;

                return false;
            }
        }

        #endregion

        #region Collimator Calibration Handle

        #region Flow Handle

        /// <summary>
        /// 移动波太
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<bool> MoveBowtieAsync(bool enable, CancellationToken token) 
        {
            uint bowtiePosition = enable ? 0u : 3733u;
            bool moveBowtieResult = MoveCollimator(CollimatorSourceIndex.All, CollimatorMotorType.Bowtie, bowtiePosition);
            if (!moveBowtieResult)
            {
                return false;
            }
            bool isBowtieArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
            if (!isBowtieArrivedSignalAcquired)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移动限束器前后遮挡到指定位置（可指定先移动前遮挡还是后遮挡）
        /// </summary>
        /// <param name="frontBladeTargetPosition"></param>
        /// <param name="rearBladeTargetPosition"></param>
        /// <param name="isRearBladeMoveFirst"></param>
        /// <returns></returns>
        private async Task<bool> MoveCollimatorFrontRearBladeToTargetPositionAsync(
            uint frontBladeTargetPosition, uint rearBladeTargetPosition, bool isRearBladeMoveFirst, CancellationToken token)
        {
            if (isRearBladeMoveFirst)
            {
                bool moveRearResult = MoveCollimator(CollimatorSourceIndex.All, CollimatorMotorType.RearBlade, rearBladeTargetPosition);
                if (!moveRearResult)
                {
                    return false;
                }

                bool isRearBladeArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
                if (!isRearBladeArrivedSignalAcquired)
                {
                    return false;
                }

                bool moveFrontResult = MoveCollimator(CollimatorSourceIndex.All, CollimatorMotorType.FrontBlade, frontBladeTargetPosition);
                if (!moveFrontResult)
                {
                    return false;
                }

                bool isFrontBladeArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
                if (!isFrontBladeArrivedSignalAcquired)
                {
                    return false;
                }
            }
            else
            {
                bool moveFrontResult = MoveCollimator(CollimatorSourceIndex.All, CollimatorMotorType.FrontBlade, frontBladeTargetPosition);
                if (!moveFrontResult)
                {
                    return false;
                }

                bool isFrontBladeArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
                if (!isFrontBladeArrivedSignalAcquired)
                {
                    return false;
                }

                bool moveRearResult = MoveCollimator(CollimatorSourceIndex.All, CollimatorMotorType.RearBlade, rearBladeTargetPosition);
                if (!moveRearResult)
                {
                    return false;
                }

                bool isRearBladeArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
                if (!isRearBladeArrivedSignalAcquired)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 开始一次完整的生数据采集
        /// </summary>
        /// <returns></returns>
        private async Task<bool> StartCompleteRawDataAcquisitionAsync(CancellationToken token)
        {
            bool startScanResult = StartScan();
            if (!startScanResult)
            {
                return false;
            }

            bool isSignalAcquired = await WaitRawDataSavedSignalAsync(token);
            if (!isSignalAcquired)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行FullOpen生数据的可用性校验（即前遮挡最大，后遮挡0的情况下，对采集的图像进行校验）
        /// </summary>
        private async Task<bool> ExecuteFullOpenRawDataValidationAsync(CancellationToken token)
        {
            const int defaultTablePosition = 50 * 1000;

            try
            {
                if (Math.Abs(DeviceSystem.Instance.Table.HorizontalPosition) > defaultTablePosition) 
                {
                    _messagePrintService.PrintLoggerInfo($"Move table to horizontal 30mm.");
                    var tableMoveResult = MotionControlProxy.Instance.StartMoveTable(new TableParams
                    {
                        Direction = TableMoveDirection.Horizontal,
                        HorizontalPosition = 30 * 1000,
                        HorizontalVelocity = 250 * 1000
                    });
                    if (!tableMoveResult.Status)
                    {
                        _messagePrintService.PrintLoggerError($"Failed to send table move command, message - {tableMoveResult.Message}.");

                        return false;
                    }
                }

                bool closeBowtieResult = await MoveBowtieAsync(false, token);
                if (!closeBowtieResult) 
                {
                    return false;
                }

                bool moveResult = await MoveCollimatorFrontRearBladeToTargetPositionAsync(_collimatorConfig.MaxFrontBladeMoveStep, _collimatorConfig.MinMoveStep, true, token);
                if (!moveResult)
                {
                    return false;
                }

                bool dataAcquisitionResult = await StartCompleteRawDataAcquisitionAsync(token);
                if (!dataAcquisitionResult)
                {
                    return false;
                }

                bool checkResult = await CheckFullOpenRawDataIntegrityAsync(CurrentRawDataDirectory);
                if (!checkResult)
                {
                    return false;
                }

                return true;
            }
            catch (OperationCanceledException) 
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(ExecuteFullOpenRawDataValidationAsync)}] Cancelled.");

                return false;
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(ExecuteFullOpenRawDataValidationAsync)}] Unhandled exception, [Stack]: {ex}.");

                return false;
            }
        }

        /// <summary>
        /// 执行校准
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ExecuteCalibrationAsync(CollimatorMotorType motorType, CancellationToken token)
        {
            try
            {
                CurrentCalibratingMotorType = motorType;

                /** 重置校准环境 **/

                ResetIterativeCalibrationEnvironment();

                /** 配置校准库 **/

                bool configureResult = await ConfigureCalibrationLibraryAsync();
                if (!configureResult)
                {
                    return false;
                }

                /** 第一轮特殊处理 **/

                var firstRoundFrontTargetPosition = motorType == CollimatorMotorType.FrontBlade ?
                    SelectedCalibrationOpeningType.FrontBladeTargetPosition : _collimatorConfig.MaxFrontBladeMoveStep;
                var firstRoundRearTargetPosition = motorType == CollimatorMotorType.FrontBlade ?
                    _collimatorConfig.MinMoveStep : SelectedCalibrationOpeningType.RearBladeTargetPosition;
                var isRearMoveFirst = motorType == CollimatorMotorType.FrontBlade;

                bool firstRoundMoveResult = await MoveCollimatorFrontRearBladeToTargetPositionAsync(
                    firstRoundFrontTargetPosition, firstRoundRearTargetPosition, isRearMoveFirst, token);
                if (!firstRoundMoveResult)
                {
                    return false;
                }

                bool firstRoundDataAcquisitionResult = await StartCompleteRawDataAcquisitionAsync(token);
                if (!firstRoundDataAcquisitionResult)
                {
                    return false;
                }

                var firstRoundIterativeStatus = await ExecuteIterativeCalibrationAsync(CurrentRawDataDirectory);
                if (firstRoundIterativeStatus == IterativeStatus.Error)
                {
                    return false;
                }

                /** 第二轮特殊处理，若OpenWidth为288，则需要在将当前校准的电机移动至2500 **/

                if (SelectedCalibrationOpeningType.OpenWidth == CollimatorOpenWidth.FullOpen)
                {
                    bool secondRoundMoveResult = MoveCollimator(CollimatorSourceIndex.All, motorType, 2500);
                    if (!secondRoundMoveResult)
                    {
                        return false;
                    }
                    bool isSecondRoundMoveArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
                    if (!isSecondRoundMoveArrivedSignalAcquired)
                    {
                        return false;
                    }

                    bool secondRoundDataAcquisitionResult = await StartCompleteRawDataAcquisitionAsync(token);
                    if (!secondRoundDataAcquisitionResult)
                    {
                        return false;
                    }

                    var secondRoundIterativeStatus = await ExecuteIterativeCalibrationAsync(CurrentRawDataDirectory);
                    if (secondRoundIterativeStatus == IterativeStatus.Error)
                    {
                        return false;
                    }
                }

                /** 开始迭代 **/

                while (!token.IsCancellationRequested)
                {
                    bool iterativeMoveResult = MoveCollimator(motorType, IterativeResultsCache.ToNextRoundTargetPositions());
                    if (!iterativeMoveResult)
                    {
                        return false;
                    }
                    bool isIterativeMoveArrivedSignalAcquired = await WaitCollimatorArrivedSignalAsync(token);
                    if (!isIterativeMoveArrivedSignalAcquired)
                    {
                        return false;
                    }

                    bool iterativeDataAcquisitionResult = await StartCompleteRawDataAcquisitionAsync(token);
                    if (!iterativeDataAcquisitionResult)
                    {
                        return false;
                    }

                    var iterativeStatus = await ExecuteIterativeCalibrationAsync(CurrentRawDataDirectory);

                    if (iterativeStatus == IterativeStatus.Error)
                    {
                        return false;
                    }
                    else if (iterativeStatus == IterativeStatus.Complete)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (OperationCanceledException)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(ExecuteCalibrationAsync)}] Cancelled.");

                return false;
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(ExecuteCalibrationAsync)}] Unhandled exception, [Stack]: {ex}.");

                return false;
            }
        }

        /// <summary>
        /// 重置迭代校准环境
        /// </summary>
        /// <returns></returns>
        private void ResetIterativeCalibrationEnvironment()
        {
            try
            {
                ResetCalibrationLibrary();
                CurrentIterationRound = 1;
                IterativeResultsCache.Reset();           
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(ResetIterativeCalibrationEnvironment)}] Unhandled exception, [Stack]: {ex}.");
            }
        }

        /// <summary>
        /// 等待限束器到位信号
        /// </summary>
        /// <returns></returns>
        private async Task<bool> WaitCollimatorArrivedSignalAsync(CancellationToken token)
        {
            var acquired = await CollimatorArrivedDuringCalibrationSignal.WaitAsync(_collimatorArrivedTimeout, token);

            if (!acquired)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(WaitCollimatorArrivedSignalAsync)}] Timeout occurred when waiting for collimator arrived signal.");
            }

            return acquired;
        }

        /// <summary>
        /// 等待生数据保存结束信号
        /// </summary>
        /// <returns></returns>
        private async Task<bool> WaitRawDataSavedSignalAsync(CancellationToken token)
        {
            var acquired = await RawDataSaveFinishedDuringCalibrationSignal.WaitAsync(_rawdataSavedTimeout, token);

            if (!acquired)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(WaitRawDataSavedSignalAsync)}] Timeout occurred when waiting for collimator arrived signal.");
            }

            return acquired;
        }

        #endregion

        #region Iteration Round Handle

        private object _interLocker = new();

        /// <summary>
        /// 累加更新迭代轮次
        /// </summary>
        /// <returns></returns>
        private void AccumulateIterationRound()
        {
            lock (_interLocker) 
            {
                CurrentIterationRound++;

                //迭代大于15轮，警告提醒
                if (CurrentIterationRound > 15) 
                {
                    _messagePrintService.PrintLoggerWarn($"Current iteration round - [{CurrentIterationRound}] is beyond expexted.");
                }
                //迭代达到20抡，终止校准
                if (CurrentIterationRound == 20) 
                {
                    StopCalibration();
                }
            }       
        }

        #endregion

        #region RawData Integrity Handle

        /// <summary>
        /// 验证生数据文件夹下的数据完整性
        /// </summary>
        private bool ValidateRawDataDirectoryIntegrity(string directory)
        {
            bool isValidData = Directory.GetFiles(directory).Any(t => t.EndsWith(".raw"));

            if (isValidData)
            {
                _messagePrintService.PrintLoggerInfo($"Data acquisition has been finished, data path: {directory}.");
            }
            else
            {
                _messagePrintService.PrintLoggerError($"No valid data in {directory}.");
            }

            return isValidData;
        }

        #endregion

        #region CTS Lifetime Handle

        /// <summary>
        /// 校准取消Token
        /// </summary>
        private CancellationTokenSource _calibrationCTS = null!;

        /// <summary>
        /// 清除CTS
        /// </summary>
        private void ClearCalibrationCTS()
        {
            _calibrationCTS.Dispose();
            _calibrationCTS = null!;
        }

        /// <summary>
        /// 重置CTS
        /// </summary>
        private void ResetCalibrationCTS()
        {
            _calibrationCTS = new();
        }

        #endregion

        #region Signal Lifetime Handle

        /// <summary>
        /// 校准过程中限束器到位信号量
        /// </summary>
        public SemaphoreSlim CollimatorArrivedDuringCalibrationSignal { get; private set; } = new(0, 1);
        /// <summary>
        /// 校准过程中数据采集成功信号量
        /// </summary>
        public SemaphoreSlim RawDataSaveFinishedDuringCalibrationSignal { get; private set; } = new(0, 1);

        /// <summary>
        /// 清除信号量
        /// </summary>
        private void ClearSignal()
        {
            CollimatorArrivedDuringCalibrationSignal.Dispose();
            CollimatorArrivedDuringCalibrationSignal = null!;

            RawDataSaveFinishedDuringCalibrationSignal.Dispose();
            RawDataSaveFinishedDuringCalibrationSignal = null!;
        }

        /// <summary>
        /// 重置信号量
        /// </summary>
        private void ResetSignal()
        {
            CollimatorArrivedDuringCalibrationSignal = new SemaphoreSlim(0, 1);
            RawDataSaveFinishedDuringCalibrationSignal = new SemaphoreSlim(0, 1);
        }

        #endregion

        #region Reset Handle

        private void ResetCore() 
        {
            ResetSignal();
            ResetCalibrationCTS();
            ResetCalibrationLibrary();
            ResetCalibrationTableCache();
            ResetCalibrationProperties();
        }

        /// <summary>
        /// 重置校准属性
        /// </summary>
        private void ResetCalibrationProperties()
        {
            CurrentIterationRound = 1;
            CurrentCalibratingMotorType = CollimatorMotorType.FrontBlade;
            IterativeResultsCache.Reset();
        }

        /// <summary>
        /// 重置校准表记录
        /// </summary>
        private void ResetCalibrationTableCache()
        {
            CalibrationTableCache = new()
            {
                OpenMode = SelectedCalibrationOpeningType.OpenMode,
                OpenWidth = SelectedCalibrationOpeningType.OpenWidth
            };
        }

        #endregion

        #endregion

        #region Calibration Library Caller

        /// <summary>
        /// 检查在限束器前遮挡4000，后遮挡0的情况下的数据可迭代完整性
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckFullOpenRawDataIntegrityAsync(string directory) 
        {
            try
            {
                var checkResponse = await Task.Run(() => CollimatorCalibrationLibraryCaller.CheckAllFullOpenRawDataFile(directory));

                if (!checkResponse.status)
                {
                    _messagePrintService.PrintLoggerError(checkResponse.message);
                    _messagePrintService.PrintLoggerError($"Failed to pass full open rawdata integrity check, directory - [{directory}]");
                }
                else
                {
                    _messagePrintService.PrintLoggerInfo(checkResponse.message);
                    _messagePrintService.PrintLoggerInfo($"Full open rawdata integrity check has been passed successfully.");
                }

                return checkResponse.status;

            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{nameof(CollimatorCalibrationViewModel)}]" +
                    $"[{nameof(CheckFullOpenRawDataIntegrityAsync)}] Unhandled exception, [Stack]: {ex}.");

                return false;
            }
        }

        /// <summary>
        /// 配置校准库
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private async Task<bool> ConfigureCalibrationLibraryAsync()
        {
            try
            {
                _messagePrintService.PrintLoggerInfo($"Prepare to configure calibration library, " +              
                    $"Motor Type - [{Enum.GetName(CurrentCalibratingMotorType)}], " +
                    $"Opening Mode - [{Enum.GetName(SelectedCalibrationOpeningType.OpenMode)}], " +
                    $"Opening Width - [{Enum.GetName(SelectedCalibrationOpeningType.OpenWidth)}].");

                var targetDetectorWidth = CurrentCalibratingMotorType switch
                {
                    CollimatorMotorType.FrontBlade => SelectedCalibrationOpeningType.FrontBladeWidth,
                    CollimatorMotorType.RearBlade => SelectedCalibrationOpeningType.RearBladeWidth,
                    _ => throw new NotSupportedException()
                };

                IterativeResultsCache.ForEach(t => t.TargetDetectorPosition = (int)targetDetectorWidth);

                var configureResponse = await Task.Run(()
                    => CollimatorCalibrationLibraryCaller.ConfigureCalibration((int)targetDetectorWidth, (int)CurrentCalibratingMotorType));
        
                if (!configureResponse.status)
                {
                    _messagePrintService.PrintLoggerError($"Failed to configure calibration library, error message - [{configureResponse.message}].");

                    return false;
                }

                _messagePrintService.PrintLoggerInfo($"Calibration library has been configured successfully.");

                return true;
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.CollimatorComponent}]" +
                    $"[{nameof(ConfigureCalibrationLibraryAsync)}] Unhandled exception, [Stack]: {ex}.");

                return false;
            }
        }

        /// <summary>
        /// 执行算法迭代校准
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private async Task<IterativeStatus> ExecuteIterativeCalibrationAsync(string directory)
        {
            try
            {
                var collimatorDataSet = new CollimatorDataSet()
                {
                    collimatorDatas = CurrentCollimatorPositions.ToAlgorithmModel().ToArray()
                };

                var iterativeResult = await Task.Run(() =>
                {
                   return CollimatorCalibrationLibraryCaller.ApplyCalibration(directory, ref collimatorDataSet);
                });

                IterativeResultsCache.Update(iterativeResult);

                _messagePrintService.PrintLoggerInfo(iterativeResult.ToString());

                if (iterativeResult.status == (int)IterativeStatus.Complete)
                {
                    _messagePrintService.PrintLoggerInfo($"[Final {Enum.GetName(CurrentCalibratingMotorType)} " +
                        $"Calibrated Collimator Position] - {iterativeResult.collimatorTargetPosition.ToFormatString()}.");

                    CalibrationTableCache.Update(CurrentCalibratingMotorType, CurrentCollimatorPositions.ToAlgorithmModel());
                }
                else if (iterativeResult.status == (int)IterativeStatus.Error)
                {
                    _messagePrintService.PrintLoggerError($"Error message from iterative calibration - [{iterativeResult.message}].");
                }

                AccumulateIterationRound();

                return (IterativeStatus)iterativeResult.status;
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.CollimatorComponent}]" +
                    $"[{nameof(ExecuteIterativeCalibrationAsync)}] Unhandled exception, [Stack]: {ex}");

                return IterativeStatus.Error;
            }
        }

        /// <summary>
        /// 重置校准库
        /// </summary>
        /// <returns></returns>
        private void ResetCalibrationLibrary() 
        {
            CollimatorCalibrationLibraryCaller.Reset();
        }

        #endregion

        #region Calibration Table Configuration

        /** 配置限束器校准表 **/

        /// <summary>
        /// 可进行校准表配置信号量
        /// </summary>
        public SemaphoreSlim ConfigurationFinishedSignal { get; private set; } = new(0, 1);

        /// <summary>
        /// 配置校准表
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanOperateCollimator))]
        private async Task ConfigureCalibrationTableAsync()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Please select a calibration json file.",
                Filter = "Json(.json)|*.json",
                Multiselect = false,
                RestoreDirectory = true
            };

            var dialogResult = openFileDialog.ShowDialog();

            try
            {
                if (dialogResult is true)
                {
                    await ConfigureCalibrationTableCoreAsync(openFileDialog.FileName);                    
                }
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError($"[{ComponentDefaults.CollimatorComponent}]" +
                    $"[{nameof(ConfigureCalibrationTableAsync)}] Unhandled exception, [Stack]: {ex}");
            }
        }

        /// <summary>
        /// 配置校准表Core
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private async Task<bool> ConfigureCalibrationTableCoreAsync(string filepath) 
        {
            var calibratedFileContent = await File.ReadAllTextAsync(filepath);
            var calibrationTable = JsonSerializer.Deserialize<CollimatorCalibrationTable>(calibratedFileContent)!;

            _messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.CollimatorComponent}][{nameof(ConfigureCalibrationTableCoreAsync)}] " +
                $"OpenMode - {Enum.GetName(calibrationTable.OpenMode)}, OpenWidth - {calibrationTable.OpenWidth}");

            var response = MotionControlProxy.Instance.ConfigureCollimatorCalibrationTable(calibrationTable);

            _messagePrintService.PrintResponse(response);

            if (!response.Status)
            {
                return false;
            }

            var acquired = await ConfigurationFinishedSignal.WaitAsync(TimeSpan.FromSeconds(30));

            if (!acquired)
            {
                _messagePrintService.PrintLoggerError($"Calibration table configuration timeout.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// 校准表配置结果判断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="configureResult"></param>
        private void CollimatorCalibration_CollimatorCalibrationTableConfigured(object? sender, CollimatorCalibrationTableConfigureResult configureResult)
        {
            if (configureResult.Status)
            {
                _messagePrintService.PrintLoggerInfo("Collimator calibration table configuration finished.");
            }
            else
            {
                _messagePrintService.PrintLoggerError("Collimator calibration table configuration failed, please check ifbox log.");
            }

            ConfigurationFinishedSignal.Release();
        }

        /** 根据校准表进行前后遮挡移动 **/

        public SemaphoreSlim MoveByCalibrationTableSignal { get; private set; } = new SemaphoreSlim(0, 1);
        public bool MoveByCalibrationTableFlag { get; private set; } = false;
        public TimeSpan MoveByCalibrationTableTimeout { get; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 根据校准表进行步进移动
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanOperateCollimator))]
        private async Task MoveBySelectedCalibrationTableAsync()
        {
            /** 文件选择 **/
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Please select a calibration json file.",
                Filter = "Json(.json)|*.json",
                Multiselect = false,
                RestoreDirectory = true
            };
            /** 打开对话框 **/
            var dialogResult = openFileDialog.ShowDialog();

            try
            {
                /** 处理文件 **/
                if (dialogResult is true)
                {
                    MoveByCalibrationTableFlag = true;

                    //读校准表
                    var calibratedFileContent = await File.ReadAllTextAsync(openFileDialog.FileName);
                    //读步进
                    var calibrationTable = JsonSerializer.Deserialize<CollimatorCalibrationTable>(calibratedFileContent)!;
                    //移动后遮
                    bool result1 = MoveCollimator(CollimatorMotorType.RearBlade, calibrationTable.RearBladeMoveSteps);
                    //校验
                    if (!result1)
                    {
                        return;
                    }

                    bool rearArrivedSignalAcquired = await MoveByCalibrationTableSignal.WaitAsync(MoveByCalibrationTableTimeout);
                    if (!rearArrivedSignalAcquired)
                    {
                        IsCollimatorMoving = false;

                        _messagePrintService.PrintLoggerError("Wait collimator arrived signal timeout when moving rear blade.");

                        return;
                    }

                    //移动前遮
                    bool result2 = MoveCollimator(CollimatorMotorType.FrontBlade, calibrationTable.FrontBladeMoveSteps);
                    //校验
                    if (!result2)
                    {
                        return;
                    }

                    bool frontArrivedSignalAcquired = await MoveByCalibrationTableSignal.WaitAsync(MoveByCalibrationTableTimeout);
                    if (!frontArrivedSignalAcquired)
                    {
                        IsCollimatorMoving = false;

                        _messagePrintService.PrintLoggerError("Wait collimator arrived signal timeout when moving front blade.");

                        return;
                    }
                }
                else
                {
                    _logService.Warn(ServiceCategory.HardwareTest, $"[{ComponentDefaults.CollimatorComponent}] Open file dialog failed.");
                }
            }
            finally 
            {
                MoveByCalibrationTableFlag = false;
            }
        }

        /** 开口移动 **/
        [RelayCommand(CanExecute = nameof(CanOperateCollimator))]
        private async Task MoveBySelectedOpeningAsync()
        {
            try
            {
                await Task.Run(() => MoveCollimator(SelectedApplicationOpeningType.OpenMode, SelectedApplicationOpeningType.OpenWidth, CurrentBowtieMoveOption));
            }
            catch (Exception ex)
            {
                _messagePrintService.PrintLoggerError(
                    $"[{ComponentDefaults.CollimatorComponent}] Something wrong when [MoveBySelectedOpening], [Stack]: {ex}.");
            }
        }

        #endregion

        #region Console Message

        private void MessagePrintService_OnConsoleMessageChanged(object? sender, string message)
        {
            ConsoleMessage = message;
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
            //记录 
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.CollimatorComponent}] Enter [Collimator Calibration] testing page.");
            //更新当前限束器位置
            UpdateCollimatorPositionByReadRegister();
            //开启事件监听 
            RegisterProxyEvents();
            //注册消息打印 
            RegisterMessagePrinterEvents();
        }

        public override void BeforeNavigateToOtherPage()
        {
            //取消事件监听 
            UnRegisterProxyEvents();
            //取消消息打印 
            UnRegisterMessagePrinterEvents();
            //记录 
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.CollimatorComponent}] Leave [Collimator Calibration] testing page.");
        }

        #endregion

    }

}
