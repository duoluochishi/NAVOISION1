using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Fields;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Core.DeviceInteract.Models;
using NV.CT.FacadeProxy.Essentials.EventArguments;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Managers.Abstractions;
using NV.CT.Service.HardwareTest.Attachments.Repository;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Services.Universal.EventData.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.PrintMessage.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Utils;
using NV.CT.Service.HardwareTest.UserControls.Components.XRaySource;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using Panuon.WPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource
{
    public partial class XRaySourceComprehensiveTestingViewModel : NavigationViewModelBase, IModuleDirectory
    {
        private readonly ILogService logService;
        private readonly IMessagePrintService messagePrintService;
        private readonly IEventDataAddressService eventDataAddressService;
        private readonly IXRaySourceInteractManager xraySourceInteractManager;
        private readonly XRaySourceConfigOptions xraySourceConfigService;
        private readonly HardwareTestConfigOptions hardwareTestConfigService;
        private readonly IRepository<XRaySourceHistoryData> xRaySourceHistoryDataRepository;

        public XRaySourceComprehensiveTestingViewModel(
            ILogService logService,
            IMessagePrintService messagePrintService,
            IEventDataAddressService eventDataAddressService,
            IXRaySourceInteractManager xRaySourceInteractManager,
            IOptions<XRaySourceConfigOptions> xRaySourceConfigOptions,
            IOptions<HardwareTestConfigOptions> hardwareTestConfigOptions,
            IRepository<XRaySourceHistoryData> xRaySourceHistoryDataRepository)
        {
            //get services from DI 
            this.logService = logService;
            this.messagePrintService = messagePrintService;
            this.eventDataAddressService = eventDataAddressService;
            this.xraySourceInteractManager = xRaySourceInteractManager;
            this.xraySourceConfigService = xRaySourceConfigOptions.Value;
            this.hardwareTestConfigService = hardwareTestConfigOptions.Value;
            this.xRaySourceHistoryDataRepository = xRaySourceHistoryDataRepository;
            //Initialize 
            this.InitializeProxy();
            this.InitializeXRaySourceComponent();
            this.InitializeModuleDirectory();
        }

        #region Initialize

        private void InitializeXRaySourceComponent()
        {
            this.XRaySources = new ObservableCollection<XRayOriginSource>();

            for (uint i = 1; i <= xraySourceConfigService.TubeInterfaceCount * xraySourceConfigService.XRaySourceCountPerTubeInterface; i++)
            {
                XRaySources.Add(new XRayOriginSource() { Name = $"XRay Source - {i.ToString("00")}", Index = i });
            }

            this.XRaySourceParameters = new();
            this.XDataAcquisitionParameters = new();
            XDataAcquisitionParameters.ExposureParameters.KVs = [xraySourceConfigService.Voltage, 0,0,0,0,0,0,0];
            XDataAcquisitionParameters.ExposureParameters.MAs = [xraySourceConfigService.Current, 0,0,0,0,0,0,0];
            XDataAcquisitionParameters.ExposureParameters.ExposureTime = xraySourceConfigService.ExposureTime;
            XDataAcquisitionParameters.ExposureParameters.FrameTime = xraySourceConfigService.FrameTime;
            XDataAcquisitionParameters.ExposureParameters.TotalFrames = xraySourceConfigService.SeriesLength;
            XDataAcquisitionParameters.ExposureParameters.ExposureDelayTime = xraySourceConfigService.DelayExposureTime;
            XDataAcquisitionParameters.ExposureParameters.ExposureRelatedChildNodesConfig = xraySourceConfigService.ExposureRelatedChildNodesConfig;
            XDataAcquisitionParameters.ExposureParameters.GantryVelocity = 0;          
        }

        private void InitializeProxy()
        {
            try
            { 
                AcqReconProxy.Instance.Init(new());
                //记录 
                logService.Info(ServiceCategory.HardwareTest, 
                    $"[{ComponentDefaults.XRaySourceComponent}] AcqReconProxy has been initialized.");
            }
            catch (Exception ex)
            {
                logService.Error(ServiceCategory.HardwareTest, 
                    $"[{ComponentDefaults.XRaySourceComponent}] Something wrong when initailize AcqReconProxy, [Stack]: {ex.ToString()}.");
            }
        }

        public void InitializeModuleDirectory()
        {
            try
            {
                FileUtils.EnsureDirectoryPath(ModuleDataDirectoryPath);
            }
            catch (Exception ex)
            {
                logService.Error(ServiceCategory.HardwareTest, 
                    $"[{nameof(XRaySourceComprehensiveTestingViewModel)}][{nameof(InitializeModuleDirectory)}] Unhandled exception, [Stack]: {ex}");
            }        
        }

        public void InitializeTimer() 
        {
            HistoryDataSaveTimer = new() 
            {
                Interval = 1000
            };
            HistoryDataSaveTimer.Elapsed += (_, _) => 
            {
                HitClockTickToSave = true;
            };
            HistoryDataSaveTimer.Start();
        }

        public void StopTimer() 
        {
            HistoryDataSaveTimer?.Stop();

            HitClockTickToSave = false;
        }

        #endregion

        #region Fields

        private static uint currentCycleIndex = 1;

        #endregion

        #region Properties

        [ObservableProperty]
        private string consoleMessage = string.Empty;

        public ObservableCollection<XRayOriginSource> XRaySources { get; private set; } = null!;
        public DataAcquisitionParameters XDataAcquisitionParameters { get; private set; } = null!;
        public XRaySourceParameters XRaySourceParameters { get; private set; } = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanRunningXRaySourceTest))]
        private XRaySourceTestStatus currentXRaySourceTestStatus = XRaySourceTestStatus.NormalStop;

        public bool CanRunningXRaySourceTest => this.CurrentXRaySourceTestStatus == XRaySourceTestStatus.NormalStop;

        public string ModuleDataDirectoryPath
            => Path.Combine(hardwareTestConfigService.DataDirectoryPath, ComponentDefaults.XRaySourceComponent);

        [ObservableProperty]
        private bool canSaveXRaySourceHistoryData = false;

        private Timer HistoryDataSaveTimer { get; set; } = null!;

        private bool HitClockTickToSave { get; set; } = false;

        #endregion

        #region PropertiesChanged

        partial void OnCurrentXRaySourceTestStatusChanged(XRaySourceTestStatus oldValue, XRaySourceTestStatus newValue)
        {
            DispatcherWrapper.Invoke(() =>
            {
                StartXRaySourceTestCommand.NotifyCanExecuteChanged();
            });
        }

        partial void OnCanSaveXRaySourceHistoryDataChanged(bool oldValue, bool newValue)
        {
            if (newValue) 
            {
                InitializeTimer();
            }
            else
            {
                StopTimer();
            }
        }

        #endregion

        #region Plot Chart

        [RelayCommand]
        private void PlotXRaySourceChart() 
        {
            DialogHelper.ShowDialog<XRaySourceChartPlotView>();
        }

        #endregion

        #region XRaySource Calibration

        [RelayCommand]
        private void XRaySourceCalibrationSwitch(XRayOriginSource source)
        {
            string message = string.Empty;
            //开始/停止训管前弹窗确认 
            message = source.IsChecked ? $"Start XRay Source {source.Index} calibration ?" : $"Stop XRay Source {source.Index} calibration ?";
            //弹窗确认 
            var result = DialogService.Instance.ShowConfirm(message);
            //若取消，恢复状态 
            if (!result)
            {
                source.IsChecked = !source.IsChecked;
                return;
            }
            //开始训管 
            var response = xraySourceInteractManager.SwitchXRaySourceCalibration(XRaySources, source, ComponentDefaults.XRaySourceComponent);
            //判定 
            if (!response.status) 
            {
                source.IsChecked = !source.IsChecked;
            }
            //打印过程信息 
            messagePrintService.PrintEnumerable(response.message);
        }

        #endregion

        #region Events

        #region Event Data

        /// <summary>
        /// 事件数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void XRaySourceTest_EventDataReceived(object? sender, EventDataEventArgs args)
        {
            switch (args.EventDataInfo.Type) 
            {
                case EventDataType.Unknown: CommonEventDataResolver(args.EventDataInfo.Data); break;
                case EventDataType.DoseInfo: DoseInfoEventDataResolver(args.EventDataInfo.Data); break;
                case EventDataType.TableArrived: break;               
            }
        }

        /// <summary>
        /// 通用事件数据解析
        /// </summary>
        /// <param name="data">事件数据</param>
        private void CommonEventDataResolver(byte[] data)
        {
            //数据合法性校验 
            if (data is null) return;
            //拆分地址与内容 
            var pair = EventDataHelper.ParseEventData(data);
            //匹配地址 
            bool result = eventDataAddressService.MatchStatusInfoAddress(pair.address, out XRaySourceStatusInfo? xRaySourceStatusInfo);
            //处理 
            if (result) 
            {
                if (xRaySourceStatusInfo is not null) 
                {
                    //打印接收到的值 
                    logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Received XRaySource Status Code Value: {pair.content}");
                    //更新状态 
                    xRaySourceStatusInfo.Status = XRaySourceHelper.AnalyseXRaySourceStatus(pair.content);
                    //显示记录 
                    logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Received XRaySource status from event data, " +
                        $"index: [{xRaySourceStatusInfo.Index}], status: [{Enum.GetName(xRaySourceStatusInfo.Status)}].");
                    //更新XRaySource对应状态 
                    this.UpdateXRaySourceStatusInfo(xRaySourceStatusInfo);
                }
                else
                {
                    messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.XRaySourceComponent}] " +
                        $"Matched doseinfo address: {pair.address.ToString("X")} return null XRaySourceStatusInfo instance.");
                }
            }
        }

        /// <summary>
        /// 剂量信息事件数据解析
        /// </summary>
        /// <param name="data">事件数据</param>
        private void DoseInfoEventDataResolver(byte[] data) 
        {
            //数据合法性校验 
            if (data is null) return;
            //拆分地址与内容 
            var pair = EventDataHelper.ParseEventData(data);
            //匹配地址 
            bool result = eventDataAddressService.MatchDoseInfoAddress(pair.address, out XRaySourceDose? doseInfo);
            //处理 
            if (result)
            {
                if (doseInfo is not null)
                {
                    //更新值(真实值为接收值除以10) 
                    doseInfo.Value = pair.content * Coefficients.ReduceCoef_10;
                    //更新页面值 
                    XRaySourceHelper.UpdateXRaySourceDose(this.XRaySources, doseInfo);
                    //若可记录，由于kv/mA/ms的更新速率偏低，不做存储间隔限制
                    if (CanSaveXRaySourceHistoryData) 
                    {
                        xRaySourceHistoryDataRepository.Add(doseInfo.ToXRaySourceHistoryData());
                    }            
                }
                else 
                {
                    messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.XRaySourceComponent}] " +
                        $"Matched doseinfo address: {pair.address.ToString("X")} return null DoseInfo instance.");
                }
            }
            else 
            {
                messagePrintService.PrintLoggerWarn($"[{ComponentDefaults.XRaySourceComponent}] " +
                    $"Mismatched doseinfo address: {pair.address.ToString("X")}.");
            }
        }

        /// <summary>
        /// 更新XRaySource的status
        /// </summary>
        private void UpdateXRaySourceStatusInfo(XRaySourceStatusInfo xRaySourceStatusInfo)
        {
            //获取对应的XraySource 
            var source = this.XRaySources[(int)xRaySourceStatusInfo.Index - 1];
            //更新状态 
            source.Status = xRaySourceStatusInfo.Status;
            //若校准完成 
            if (xRaySourceStatusInfo.Status == XRaySourceStatus.CalibrationComplete)
            {
                //复原按钮 
                source.IsChecked = false;
                //更新信息 
                string message = $"[{ComponentDefaults.XRaySourceComponent}] " +
                    $"The No.{source.Index} XRaySource has been completed, the button is reset.";
                //显示记录 
                messagePrintService.PrintLoggerInfo(message);
                //若没有源在进行校准，关闭灯 
                if (!this.XRaySources.Any(t => t.IsChecked == true)) 
                {
                    //关灯 
                    var response = xraySourceInteractManager.SwitchXRayPromptLight(XRayPromptLightSwitch.OFF, ComponentDefaults.XRaySourceComponent);
                    //打印信息 
                    messagePrintService.PrintEnumerable(response.message);
                }
            }
        }

        #endregion

        #region Realtime Status

        /// <summary>
        /// 扫描过程中实时状态接收
        /// </summary>
        /// <param name="data"></param>
        private void XRaySourceTest_RealTimeStatusChanged(object sender, RealtimeEventArgs args)
        {
            this.RealTimeStatusResolver(args.Status);
        }

        /// <summary>
        /// 扫描过程中实时状态解析
        /// </summary>
        /// <param name="status">实时状态</param>
        private async void RealTimeStatusResolver(RealtimeStatus status)
        {
            //打印状态 
            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.XRaySourceComponent}] Realtime status: {Enum.GetName(status)}");
            //数据解析 
            StringBuilder message = new();
            //判断停止 
            if (status == RealtimeStatus.NormalScanStopped)
            {
                //根据模式判断 
                if (this.CurrentXRaySourceTestStatus == XRaySourceTestStatus.RunningCycleTest)
                {
                    //累加 
                    currentCycleIndex++;
                    //若未达到循环次数，继续循环 
                    if (currentCycleIndex <= XRaySourceParameters.CycleCount)
                    {
                        //等待时间间隔 
                        await Task.Delay((int)XRaySourceParameters.CycleInterval * 1000);
                        //重启单次测试 
                        this.Start();
                        //更新信息 
                        message.Append($"[{ComponentDefaults.XRaySourceComponent}] Running cycle test，" +
                            $"currently No.{currentCycleIndex}，" +
                            $"totally {this.XRaySourceParameters.CycleCount}.");
                    }
                    else
                    {
                        //重置计数 
                        currentCycleIndex = 1;
                        //更新系统状态 
                        this.CurrentXRaySourceTestStatus = XRaySourceTestStatus.NormalStop;
                        //恢复模式切换 
                        this.XRaySourceParameters.CanTestModeSwitch = true;
                        //更新信息 
                        message.Append($"[{ComponentDefaults.XRaySourceComponent}] Cycle test normally put to an end, " +
                            $"current status: [{this.CurrentXRaySourceTestStatus}], " +
                            $"current cycle index: [{currentCycleIndex}].");
                    }
                }
                else if (this.CurrentXRaySourceTestStatus == XRaySourceTestStatus.RunningSingleTest)
                {
                    //更新系统状态 
                    this.CurrentXRaySourceTestStatus = XRaySourceTestStatus.NormalStop;
                    //恢复模式切换 
                    this.XRaySourceParameters.CanTestModeSwitch = true;
                    //更新信息 
                    message.Append($"[{ComponentDefaults.XRaySourceComponent}] Single test normally put to an end.");
                }
            }
            //打印信息 
            if (!string.IsNullOrWhiteSpace(message.ToString()))
            {
                messagePrintService.PrintLoggerInfo(message.ToString());
            }
        }

        #endregion

        #region Cycle Status

        /// <summary>
        /// 周期状态接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void XRaySourceTest_CycleStatusChanged(object sender, CycleStatusArgs args)
        {
            this.CycleStatusResolver(args.Device);
        }

        /// <summary>
        /// 周期状态解析
        /// </summary>
        /// <param name="device">相关设备</param>
        private void CycleStatusResolver(DeviceSystem device)
        {
            //更新、记录XRayItems的热容、油温 
            for (int i = 0; i < device.XRaySources.Length; i++) 
            {
                var heatCap = device.XRaySources[i].XRaySourceHeatCap;
                var oilTemp = device.XRaySources[i].XRaySourceOilTemp;
                //更新 
                this.XRaySources[i].HeatCapacity = heatCap;
                this.XRaySources[i].OilTemperature = oilTemp * Coefficients.ReduceCoef_10;
                //若可记录 
                if (CanSaveXRaySourceHistoryData && HitClockTickToSave) 
                {
                    //记录 
                    xRaySourceHistoryDataRepository.Add(
                        new()
                        {
                            Index = (uint)i + 1,
                            TimeStamp = DateTime.Now,
                            DataType = XRaySourceHistoryDataType.HeatCap,
                            Value = heatCap
                        });
                    xRaySourceHistoryDataRepository.Add(
                        new()
                        {
                            Index = (uint)i + 1,
                            TimeStamp = DateTime.Now,
                            DataType = XRaySourceHistoryDataType.OilTemp,
                            Value = oilTemp
                        });
                }
            }

            //当处于周期测试状态下，有热容达到上限时，挂起，停止扫描 
            if (CurrentXRaySourceTestStatus == XRaySourceTestStatus.RunningCycleTest)
            {
                foreach (var tube in device.XRaySources)
                {
                    if (tube.XRaySourceHeatCap >= XRaySourceParameters.HeatCapacityUpperLimit)
                    {
                        logService.Warn(
                            ServiceCategory.HardwareTest, 
                            $"[{ComponentDefaults.XRaySourceComponent}] " +
                            $"XRaySource heat capacity {JsonSerializer.Serialize(tube)} beyond HeatCapacityUpperLimit [{XRaySourceParameters.HeatCapacityUpperLimit.ToString()}]," +
                            $"the cycle test will be suspended.");
                        this.SuspendCycleTest();                       
                        break;
                    }
                }                
            }

            //当处于挂起状态下，所有热容低于下限，恢复周期测试状态,重新开始扫描 
            if (CurrentXRaySourceTestStatus == XRaySourceTestStatus.SuspendCycleTest)
            {
                bool canRecover = true;

                foreach (var tube in device.XRaySources)
                {
                    if (tube.XRaySourceHeatCap > XRaySourceParameters.HeatCapacityLowerLimit)
                    {
                        canRecover = false; 
                        break;
                    }
                }

                if (canRecover) 
                {
                    logService.Warn(
                        ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] " +
                        $"All XRaySource heat capacity is lower than HeatCapacityLowerLimit [{XRaySourceParameters.HeatCapacityLowerLimit.ToString()}]," +
                        $"the cycle test will be recovered.");
                    this.RecoverCycleTest();
                }
            }

            HitClockTickToSave = false;
        }

        #endregion

        #region Events Registration

        /// <summary>
        /// 注册Proxy事件
        /// </summary>
        private void RegisterProxyEvents() 
        {
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Register AcqReconProxy events.");
            AcqReconProxy.Instance.EventDataReceived += XRaySourceTest_EventDataReceived;
            AcqReconProxy.Instance.RealTimeStatusChanged += XRaySourceTest_RealTimeStatusChanged;
            AcqReconProxy.Instance.CycleStatusChanged += XRaySourceTest_CycleStatusChanged;
        }

        private void RegisterMessagePrinterEvents()
        {
            this.messagePrintService.OnConsoleMessageChanged += MessagePrintService_OnConsoleMessageChanged;
        }

        /// <summary>
        /// 解除Proxy事件
        /// </summary>
        private void UnRegisterProxyEvents() 
        {
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Un-register AcqReconProxy events.");
            AcqReconProxy.Instance.EventDataReceived -= XRaySourceTest_EventDataReceived;
            AcqReconProxy.Instance.RealTimeStatusChanged -= XRaySourceTest_RealTimeStatusChanged;
            AcqReconProxy.Instance.CycleStatusChanged -= XRaySourceTest_CycleStatusChanged;
        }

        private void UnRegisterMessagePrinterEvents()
        {
            this.messagePrintService.OnConsoleMessageChanged -= MessagePrintService_OnConsoleMessageChanged;
        }

        #endregion

        #endregion

        #region TestControl

        [RelayCommand(CanExecute = nameof(CanRunningXRaySourceTest))]
        private void StartXRaySourceTest()
        {
            //更新当前测试状态 
            this.CurrentXRaySourceTestStatus =
                (this.XRaySourceParameters.CurrentXRaySourceTestMode == XRaySourceTestMode.Single)
                    ? XRaySourceTestStatus.RunningSingleTest : XRaySourceTestStatus.RunningCycleTest;
            logService.Info(ServiceCategory.HardwareTest,
                $"[{ComponentDefaults.XRaySourceComponent}] Start x-ray source test, current x-ray source test mode: " +
                $"[{Enum.GetName(this.CurrentXRaySourceTestStatus)}].");
            //禁用模式切换 
            this.XRaySourceParameters.CanTestModeSwitch = false;

            //打印信息 
            string message = this.XRaySourceParameters.CurrentXRaySourceTestMode == XRaySourceTestMode.Single
                ? $"[{ComponentDefaults.XRaySourceComponent}] Running single test"
                : $"[{ComponentDefaults.XRaySourceComponent}] Running cycle test，" +
                $"currently No.{currentCycleIndex}，totally {this.XRaySourceParameters.CycleCount}.";
            messagePrintService.PrintLoggerInfo(message);

            //启动测试 
            var result = this.Start();

            //若发送失败，更新当前测试状态并取消禁用模式切换 
            if (!result)
            {
                //切换系统状态 
                this.CurrentXRaySourceTestStatus = XRaySourceTestStatus.NormalStop;
                //恢复模式切换 
                this.XRaySourceParameters.CanTestModeSwitch = true;
            }
        }

        [RelayCommand]
        private void StopXRaySourceTest()
        {
            this.Terminate();
        }

        //开启测试 
        private bool Start() 
        {
            return this.StartScanCommon();
        }

        //挂起周期测试 
        private void SuspendCycleTest() 
        {
            //挂起 
            this.CurrentXRaySourceTestStatus = XRaySourceTestStatus.SuspendCycleTest;
            //记录 
            logService.Info(ServiceCategory.HardwareTest, 
                $"[{ComponentDefaults.XRaySourceComponent}] Suspending cycle test, " +
                $"current status: [{Enum.GetName(this.CurrentXRaySourceTestStatus)}].");
            //停止 
            bool result = this.StopScanCommon();
            //记录 
            if (result)
            {
                logService.Info(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] The cycle test has been suspended.");
            }
            else 
            {
                logService.Info(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] Suspending cycle test has a exception from AcqReconProxy when stop scan.");
            }

        }

        //恢复周期测试 
        private void RecoverCycleTest() 
        {
            //恢复 
            this.CurrentXRaySourceTestStatus = XRaySourceTestStatus.RunningCycleTest;
            //记录 
            logService.Info(ServiceCategory.HardwareTest,
                $"[{ComponentDefaults.XRaySourceComponent}] Recovering cycle test, " +
                $"current status: [{Enum.GetName(this.CurrentXRaySourceTestStatus)}].");
            //启动 
            this.StartScanCommon();
            //记录 
            logService.Info(ServiceCategory.HardwareTest,
                $"[{ComponentDefaults.XRaySourceComponent}] The cycle test has been recovered.");
        }

        //终止测试 
        private void Terminate() 
        {
            //周期计数清零
            currentCycleIndex = 1;
            //更新系统状态
            this.CurrentXRaySourceTestStatus = XRaySourceTestStatus.NormalStop;
            //恢复模式切换 
            this.XRaySourceParameters.CanTestModeSwitch = true;
            //记录 
            logService.Info(ServiceCategory.HardwareTest,
                $"[{ComponentDefaults.XRaySourceComponent}] Terminating test, " +
                $"current status: [{Enum.GetName(this.CurrentXRaySourceTestStatus)}]," +
                $"current cycle index: [{currentCycleIndex}].");
            //* 停止 *
            bool result = this.StopScanCommon();
            //记录 
            if (result)
            {
                logService.Info(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] The test has been terminated.");
            }
            else 
            {
                logService.Info(ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] Terminating test has a exception from AcqReconProxy when stop scan.");
            }
        }

        //启动扫描 
        private bool StartScanCommon() 
        {
            //显示记录 
            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.XRaySourceComponent}] Prepare to start scan.");
            //记录参数 
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Parameters: {JsonSerializer.Serialize(this.XRaySourceParameters)}].");

            var proxyParameters = XDataAcquisitionParameters.ToProxyParam();

            var configureResponse = DataAcquisitionProxy.Instance.ConfigureDataAcquisition(proxyParameters);
            if (!configureResponse.Status) 
            {
                messagePrintService.PrintLoggerError(configureResponse.Message);

                return false;
            }

            var startResponse = DataAcquisitionProxy.Instance.StartDataAcquisition(proxyParameters);
            if (!startResponse.Status)
            {
                messagePrintService.PrintLoggerError(startResponse.Message);

                return false;
            }

            return true;
        }

        //停止扫描 
        private bool StopScanCommon() 
        {
            //显示记录 
            messagePrintService.PrintLoggerInfo($"[{ComponentDefaults.XRaySourceComponent}] Prepare to stop scan.");
            //停止扫描 
            var response = DataAcquisitionProxy.Instance.StopDataAcquisition();
            //显示过程信息 
            messagePrintService.PrintResponse(response);

            return response.Status;
        }

        #endregion

        #region Anode Rotation Control

        [RelayCommand]
        private void StopAllAnodeRotation() 
        {
            var response = XRaySourceInteractProxy.Instance.StopAllXRaySourceAnodeRotation();

            messagePrintService.PrintResponse(response);
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
            messagePrintService.Clear();
        }

        #endregion

        #region Navigation

        public override void BeforeNavigateToCurrentPage()
        {
            //记录 
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Enter [XRaySource Comprehensive Testing] page.");
            //开启事件监听 
            this.RegisterProxyEvents();
            //注册消息打印 
            this.RegisterMessagePrinterEvents();
        }

        public override void BeforeNavigateToOtherPage()
        {
            //取消事件监听 
            this.UnRegisterProxyEvents();
            //取消消息打印 
            this.UnRegisterMessagePrinterEvents();
            //记录 
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.XRaySourceComponent}] Leave [XRaySource Comprehensive Testing] page.");
        }

        #endregion

    }

}
