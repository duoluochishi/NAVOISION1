using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Models.Components.Collimator;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Models.Components.Gantry;
using NV.CT.Service.HardwareTest.Models.Components.Table;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.ComponentStatus
{
    public partial class ComponentStatusTestingViewModel : ObservableObject, INavigationAware
    {
        private readonly ILogService logService;
        private readonly ComponentStatusConfigOptions componentStatusConfigService;

        public ComponentStatusTestingViewModel(ILogService logService, IOptions<ComponentStatusConfigOptions> componentStatusConfigOptions)
        {
            //Get from DI
            this.logService = logService;
            this.componentStatusConfigService = componentStatusConfigOptions.Value;
            //Initialize
            InitialProperties();
        }

        #region Initialize

        private void InitialProperties()
        {
            //主要部件状态列表
            MajorComponentSources = new()
            {
                new PDUSource(),
                new CTBoxSource(),
                new IFBoxSource(),
                new GantrySource(),
                new TableSource(),
                new AuxBoardSource(),
                new ExtBoardSource(),
                new ControlBoxSource()
            };
            //次要部件状态列表
            SecondaryComponentSources = new()
            {
                new WirelessPanelSource(),
                new GantryPanelSource(),
                new BreathPanelSource()
            };
            //6 Tube Interfaces
            TubeInterfaceSources = new();
            for (int i = 1; i <= componentStatusConfigService.TubeInterfaceCount; i++)
            {
                TubeInterfaceSources.Add(new TubeInterfaceSource() { Name = $"Tube Interface {i.ToString("00")}" });
            }
            //24 XRay Sources
            XRaySources = new();
            for (int i = 1; i <= componentStatusConfigService.XRaySourceCount; i++)
            {
                XRaySources.Add(new XRayOriginSource() { Name = $"XRaySource {i.ToString("00")}" });
            }
            //2 Acquisition Cards
            AcquisitionCardSources = new()
            {
                new AcquisitionCardSource { SerialNumber = 1 },
                new AcquisitionCardSource { SerialNumber = 2 }
            };
            //16 Detectors
            DetectorSources = new();
            for (int i = 1; i <= componentStatusConfigService.DetectorSourceCount; i++)
            {
                DetectorSources.Add(new DetectorSource { Name = $"Detector {i.ToString("00")}" });
            }
            //24 Collimators
            CollimatorSources = new();
            for (int i = 1; i <= componentStatusConfigService.CollimatorSourceCount; i++)
            {
                CollimatorSources.Add(new CollimatorSource { Name = $"Collimator {i.ToString("00")}" });
            }
        }

        #endregion

        #region Properties

        [ObservableProperty]
        private string lastUpdatedTime = "Not Updated";
        [ObservableProperty]
        private bool isOnlineStatusUpdated = false;
        [ObservableProperty]
        private bool isFirmwareVersionUpdated = false;
        [ObservableProperty]
        private ConnectionStatus currentCTBoxConnectionStatus = ConnectionStatus.Disconnected;
        public ObservableCollection<AbstractSource> MajorComponentSources { get; set; } = null!;
        public ObservableCollection<AbstractSource> SecondaryComponentSources { get; set; } = null!;
        public ObservableCollection<TubeInterfaceSource> TubeInterfaceSources { get; set; } = null!;
        public ObservableCollection<XRayOriginSource> XRaySources { get; set; } = null!;
        public ObservableCollection<AcquisitionCardSource> AcquisitionCardSources { get; set; } = null!;
        public ObservableCollection<DetectorSource> DetectorSources { get; set; } = null!;
        public ObservableCollection<CollimatorSource> CollimatorSources { get; set; } = null!;

        #endregion

        #region CTBox Connection Status Monitor

        partial void OnCurrentCTBoxConnectionStatusChanged(ConnectionStatus oldValue, ConnectionStatus newValue)
        {
            if (newValue == oldValue)
            {
                return;
            }

            if (newValue == ConnectionStatus.Disconnected)
            {
                // 更新Components在线状态为断连
                this.MajorComponentSources.ForEach(t => t.Online = XOnlineStatus.Offline);
                // 更新tube interfaces在线状态为断连
                this.TubeInterfaceSources.ForEach(t => t.Online = XOnlineStatus.Offline);
                //更新XRay sources在线状态为断连
                this.XRaySources.ForEach(t => t.Online = XOnlineStatus.Offline);
                // 更新 acquisition card在线状态为断连
                this.AcquisitionCardSources.ForEach(t => t.Status = FacadeProxy.Common.Enums.PartStatus.Disconnection);
                // 更新detector sources在线状态为断连
                this.DetectorSources.ForEach(t => t.ResetOnlineStatus());
                //更新Collimator在线状态为断连
                this.CollimatorSources.ForEach(t => t.Online = XOnlineStatus.Offline);
            }
        }

        #endregion

        #region Events

        #region Events Registration

        private void RegisterProxyEvents()
        {
            // 缓存连接状态
            this.CurrentCTBoxConnectionStatus = ComponentStatusProxy.Instance.CTBoxConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            // Register
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Register proxy events.");
            ComponentStatusProxy.Instance.DeviceConnectionChanged += OnlineStatus_CTBoxConnectionChanged;
            ComponentStatusProxy.Instance.CycleStatusChanged += OnlineStatus_CycleStatusChanged;
        }

        private void UnRegisterProxyEvents()
        {
            // UnRegister
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Un-register proxy events.");
            ComponentStatusProxy.Instance.DeviceConnectionChanged -= OnlineStatus_CTBoxConnectionChanged;
            ComponentStatusProxy.Instance.CycleStatusChanged -= OnlineStatus_CycleStatusChanged;
        }

        #endregion

        #region CTBox Connection

        private void OnlineStatus_CTBoxConnectionChanged(object sender, ConnectionStatusArgs args)
        {
            // 更新
            this.CurrentCTBoxConnectionStatus = args.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            // 信息
            string message = $"[{ComponentDefaults.ComponentStatus}] Current CTBox connection status: [{Enum.GetName(CurrentCTBoxConnectionStatus)}].";
            // 记录
            logService.Info(ServiceCategory.HardwareTest, message);
        }

        #endregion

        #region Cycle Status

        private void OnlineStatus_CycleStatusChanged(object sender, CycleStatusArgs args)
        {
            // 获取devices
            var devices = args.Device;
            // 校验
            if (devices is null) return;
            // 更新主要部件状态
            ComponentStatusHelper.UpdateMajorComponentStatusByCycle(MajorComponentSources, devices);
            // 更新Tube Interface状态
            ComponentStatusHelper.UpdateTubeInterfaceStatusByCycle(TubeInterfaceSources, devices);
            // 更新XRay Source状态
            ComponentStatusHelper.UpdateXRaySourceStatusByCycle(XRaySources, devices);
            // 获取采集卡状态
            ComponentStatusHelper.UpdateAcquisitionCardStatusByCycle(AcquisitionCardSources, devices);
            // 获取探测器状态
            ComponentStatusHelper.UpdateDetectorStatusByCycle(DetectorSources, devices);
        }

        #endregion

        #endregion

        #region Command

        [RelayCommand]
        private async Task UpdateAsync()
        {
            await UpdateComponentOnlineStatusAndFirmwareVersionAsync();
        }

        private async Task UpdateComponentOnlineStatusAndFirmwareVersionAsync() 
        {
            //更新在线状态
            var onlineStatusResponse = await UpdateComponentOnlineStatusAsync();
            //更新
            IsOnlineStatusUpdated = onlineStatusResponse.Status;
            //校验
            if (!onlineStatusResponse.Status) 
            {
                return;
            }        
            //更新固件版本
            var firmwareVersionResponse = await UpdateComponentFirmwareInfoAsync();
            //更新
            IsFirmwareVersionUpdated = firmwareVersionResponse.Status;
            //校验
            if (!firmwareVersionResponse.Status) 
            {
                return;
            }
            //更新时间
            LastUpdatedTime = DateTime.Now.ToLongTimeString();
        }

        #endregion

        #region Online Status

        /// <summary>
        /// 更新周期状态外的部件在线状态
        /// </summary>
        /// <returns></returns>
        private async Task<GenericResponse<bool>> UpdateComponentOnlineStatusAsync()
        {
            //获取在线状态（二级节点、射线源、限束器）
            var componentOnlineStatusInfos = await Task.Run(() => ComponentStatusProxy.Instance.GetAllComponentOnlineStatus());
            //校验
            if (componentOnlineStatusInfos is null)
            {
                logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Failed to get all component online status, please check proxy log.");

                return new (false, "Failed to get all component online status.");
            }
            //打印
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Online Status Infos: {JsonSerializer.Serialize(componentOnlineStatusInfos)})");
            //转换
            Func<bool, XOnlineStatus> OnlineConverter = isOnline => isOnline ? XOnlineStatus.Online : XOnlineStatus.Offline;
            //更新在线状态
            foreach (var item in componentOnlineStatusInfos)
            {
                switch (item.Type)
                {
                    //Secondary Components
                    case ComponentType.WirelessPanel:
                        SecondaryComponentSources.First(t => t is WirelessPanelSource).Online = OnlineConverter(item.Online); break;
                    case ComponentType.GantryPanel:
                        SecondaryComponentSources.First(t => t is GantryPanelSource).Online = OnlineConverter(item.Online); break;
                    case ComponentType.BreathPanel:
                        SecondaryComponentSources.First(t => t is BreathPanelSource).Online = OnlineConverter(item.Online); break;
                    //XRay Source
                    case ComponentType.XRaySource:
                        XRaySources[(int)item.Index].Online = OnlineConverter(item.Online); break;
                    //Collimator
                    case ComponentType.Collimator:
                        CollimatorSources[(int)item.Index].Online = OnlineConverter(item.Online); break;
                }
            }

            return new(true, "All component online status has been updated.");
        }

        #endregion

        #region Firmware Version

        private async Task<GenericResponse<bool>> UpdateComponentFirmwareInfoAsync() 
        {
            //获取所有固件版本(除采集卡、探测器)
            var componentFirmwareInfos = await Task.Run(() => ComponentStatusProxy.Instance.GetAllComponentFirmwareVersion());
            //校验
            if (componentFirmwareInfos is null)
            {
                logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Failed to get all component firmware version, please check proxy log.");

                return new(false, "Failed to get all component firmware version.");
            }
            //打印
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Firmware Infos: {JsonSerializer.Serialize(componentFirmwareInfos)})");
            //更新版本
            foreach (var item in componentFirmwareInfos)
            {
                switch (item.Type)
                {
                    //Major Components
                    case ComponentType.CTBox:
                        MajorComponentSources.First(t => t is CTBoxSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.PDU:
                        MajorComponentSources.First(t => t is PDUSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.IFBox:
                        MajorComponentSources.First(t => t is IFBoxSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.Gantry:
                        MajorComponentSources.First(t => t is GantrySource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.AuxBoard:
                        MajorComponentSources.First(t => t is AuxBoardSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.ExtBoard:
                        MajorComponentSources.First(t => t is ExtBoardSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.Table:
                        MajorComponentSources.First(t => t is TableSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.ControlBox:
                        MajorComponentSources.First(t => t is ControlBoxSource).FirmwareVersion = item.FirmwareVersion; break;
                    //Secondary Components
                    case ComponentType.WirelessPanel:
                        SecondaryComponentSources.First(t => t is WirelessPanelSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.GantryPanel:
                        SecondaryComponentSources.First(t => t is GantryPanelSource).FirmwareVersion = item.FirmwareVersion; break;
                    case ComponentType.BreathPanel:
                        SecondaryComponentSources.First(t => t is BreathPanelSource).FirmwareVersion = item.FirmwareVersion; break;
                    //Tube Interface
                    case ComponentType.TubeInterface:
                        TubeInterfaceSources[(int)item.Index].FirmwareVersion = item.FirmwareVersion; break;
                    //XRay Source
                    case ComponentType.XRaySource:
                        XRaySources[(int)item.Index].FirmwareVersion = item.FirmwareVersion; break;
                    //Collimator
                    case ComponentType.Collimator:
                        CollimatorSources[(int)item.Index].FirmwareVersion = item.FirmwareVersion; break;
                }
            }

            //获取采集卡、探测器固件版本
            var acqCardAndDetectorFirmwareInfos = await Task.Run(() => ComponentStatusProxy.Instance.GetAcqCardAndDetectorFirmwareVersion());
            //校验
            if (acqCardAndDetectorFirmwareInfos is null)
            {
                logService.Error(
                    ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.ComponentStatus}] Failed to get acq card & detector firmware version, please check proxy log.");

                return new(false, "Failed to get acq card & detector firmware version.");
            }
            //更新Acq版本显示
            var acqCardsFirmwareVersion = acqCardAndDetectorFirmwareInfos.Where(t => t.Type == ComponentType.AcqCard).ToArray();
            for (int i = 0; i < AcquisitionCardSources.Count; i++)
            {
                this.AcquisitionCardSources[i].FirmwareVersion = acqCardsFirmwareVersion[i].FirmwareVersion;
            }
            //获取探测器处理板显示版本
            var detectorUnitFirmwareVersions = acqCardAndDetectorFirmwareInfos.Where(t => t.Type == ComponentType.DetectorUnit).ToArray();
            //打印
            logService.Info(ServiceCategory.HardwareTest,
                $"[{ComponentDefaults.ComponentStatus}] receive detector units version: {JsonSerializer.Serialize(detectorUnitFirmwareVersions)}.");
            // 获取探测器传输板显示版本
            var transmitBoardsFirmwareVersion = acqCardAndDetectorFirmwareInfos.Where(t => t.Type == ComponentType.TransmitBoard).ToArray();
            // 获取探测器温控板显示版本
            var tempControlBoardsFirmwareVersion = acqCardAndDetectorFirmwareInfos.Where(t => t.Type == ComponentType.TemperatureControlBoard).ToArray();
            // 每个探测器包含4块处理板，1块传输板，1块温控板
            foreach (var source in DetectorSources)
            {
                // 获取index
                int index = this.DetectorSources.IndexOf(source);
                //每个源的处理板数量
                var processBoardCountPerSource = source.ProcessBoards.Count;
                // 更新处理板显示
                for (int p = 0; p < processBoardCountPerSource; p++) 
                {
                    source.ProcessBoards[p].FirmwareVersion = detectorUnitFirmwareVersions[(index * processBoardCountPerSource) + p].FirmwareVersion;
                }
                // 更新传输板显示
                source.TransmissionBoard.FirmwareVersion = transmitBoardsFirmwareVersion[index].FirmwareVersion;
                // 更新温控板显示
                source.TemperatureControlBoard.FirmwareVersion = tempControlBoardsFirmwareVersion[index].FirmwareVersion;
            }

            return new(true, "All component firmware version has been updated.");
        }

        #endregion

        #region Navigation

        public async void BeforeNavigateToCurrentPage()
        {
            //记录
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Enter [Component Status] testing page.");
            //注册Proxy事件
            this.RegisterProxyEvents();
            //更新
            await UpdateComponentOnlineStatusAndFirmwareVersionAsync();
        }

        public void BeforeNavigateToOtherPage()
        {
            // 取消注册Proxy事件
            this.UnRegisterProxyEvents();
            // 记录
            logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentStatus}] Leave [Component Status] testing page.");
        }

        #endregion

    }
}
