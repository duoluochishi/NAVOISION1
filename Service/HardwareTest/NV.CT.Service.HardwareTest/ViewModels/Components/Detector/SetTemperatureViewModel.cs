using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.Detector
{
    public partial class SetTemperatureViewModel : NavigationViewModelBase
    {
        private readonly ILogService _logService;
        private bool _isDoing;
        private double _allValue;
        private ObservableCollection<DetectorTargetTemperatureModel> _detectors;

        public SetTemperatureViewModel(ILogService logService)
        {
            _logService = logService;
            _detectors = new();
            var configModules = SystemConfig.DetectorTemperatureConfig.DetectorModules;

            for (int i = 1; i <= SystemConfig.DetectorConfig.Detector.XModuleCount.Value; i++)
            {
                var item = new DetectorTargetTemperatureModel
                {
                    DetectorIndex = i,
                    Detects =
                    [
                        new DetectBoardSetTempModel() { Index = 1 },
                        new DetectBoardSetTempModel() { Index = 2 },
                        new DetectBoardSetTempModel() { Index = 3 },
                        new DetectBoardSetTempModel() { Index = 4 }
                    ]
                };
                var currentConfigModule = configModules.FirstOrDefault(x => x.Index == i);

                if (currentConfigModule != null)
                {
                    Map(item.Detects[0], currentConfigModule.Channel1Temperature);
                    Map(item.Detects[1], currentConfigModule.Channel2Temperature);
                    Map(item.Detects[2], currentConfigModule.Channel3Temperature);
                    Map(item.Detects[3], currentConfigModule.Channel4Temperature);
                }

                _detectors.Add(item);
            }

            void Map(DetectBoardSetTempModel model, int temp)
            {
                var value = Math.Round(((double)temp).ReduceTen(), 1);
                model.SetValue = value;
                model.CurrentSetValue = value;
            }
        }

        public bool IsDoing
        {
            get => _isDoing;
            set => SetProperty(ref _isDoing, value);
        }

        public double AllValue
        {
            get => _allValue;
            set => SetProperty(ref _allValue, value);
        }

        public ObservableCollection<DetectorTargetTemperatureModel> Detectors
        {
            get => _detectors;
            set => SetProperty(ref _detectors, value);
        }

        [RelayCommand]
        private void ApplyAllValue()
        {
            foreach (var detector in Detectors)
            {
                foreach (var detect in detector.Detects)
                {
                    detect.SetValue = AllValue;
                }
            }
        }

        [RelayCommand]
        private async Task Set(DetectorTargetTemperatureModel item)
        {
            IsDoing = true;
            var itemProxy = item.ToDetectorTargetTemperature();
            var ret = await Task.Run(() => DeviceInteractProxy.Instance.SetDetectorTargetTemperature(itemProxy));

            if (ret.Status != CommandStatus.Success)
            {
                IsDoing = false;
                var errorCode = ret.ErrorCodes.Codes.FirstOrDefault();
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Set Failed: [{errorCode}] {errorCode.GetErrorCodeDescription()}{Environment.NewLine}{JsonUtil.Serialize(itemProxy)}");
                DialogService.Instance.ShowErrorCode(errorCode!);
                return;
            }

            foreach (var detect in item.Detects)
            {
                detect.CurrentSetValue = detect.SetValue;
            }

            var currentConfigModule = SystemConfig.DetectorTemperatureConfig.DetectorModules.FirstOrDefault(i => i.Index == item.DetectorIndex);

            if (currentConfigModule == null)
            {
                currentConfigModule = new() { Index = item.DetectorIndex };
                SystemConfig.DetectorTemperatureConfig.DetectorModules.Add(currentConfigModule);
            }

            currentConfigModule.Channel1Temperature = (int)item.Detects[0].SetValue.ExpandTen();
            currentConfigModule.Channel2Temperature = (int)item.Detects[1].SetValue.ExpandTen();
            currentConfigModule.Channel3Temperature = (int)item.Detects[2].SetValue.ExpandTen();
            currentConfigModule.Channel4Temperature = (int)item.Detects[3].SetValue.ExpandTen();

            if (!SystemConfig.SaveDetectorTemperatures())
            {
                IsDoing = false;
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Save Config Failed{Environment.NewLine}{JsonUtil.Serialize(itemProxy)}");
                DialogService.Instance.ShowError(HardwareTest_Lang.Hardware_SaveConfigFailed);
                return;
            }

            IsDoing = false;
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Set Successful{Environment.NewLine}{JsonUtil.Serialize(itemProxy)}");
            DialogService.Instance.ShowInfo("Set Successful");
        }

        [RelayCommand]
        private async Task SetAll()
        {
            IsDoing = true;
            var itemsProxy = Detectors.ToDetectorTargetTemperature().ToList();
            var ret = await Task.Run(() => DeviceInteractProxy.Instance.SetDetectorTargetTemperature(itemsProxy));

            if (ret.Status != CommandStatus.Success)
            {
                IsDoing = false;
                var errorCode = ret.ErrorCodes.Codes.FirstOrDefault();
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Set All Failed: [{errorCode}] {errorCode.GetErrorCodeDescription()} {JsonUtil.Serialize(itemsProxy)}");
                DialogService.Instance.ShowErrorCode(errorCode!);
                return;
            }

            foreach (var detector in Detectors)
            {
                foreach (var detect in detector.Detects)
                {
                    detect.CurrentSetValue = detect.SetValue;
                }
            }

            SystemConfig.DetectorTemperatureConfig.DetectorModules =
            [
                ..Detectors.Select(i => new DetectorModuleTemperatureInfo
                {
                    Index = i.DetectorIndex,
                    Channel1Temperature = (int)i.Detects[0].SetValue.ExpandTen(),
                    Channel2Temperature = (int)i.Detects[1].SetValue.ExpandTen(),
                    Channel3Temperature = (int)i.Detects[2].SetValue.ExpandTen(),
                    Channel4Temperature = (int)i.Detects[3].SetValue.ExpandTen(),
                })
            ];

            if (!SystemConfig.SaveDetectorTemperatures())
            {
                IsDoing = false;
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Save Config Failed{Environment.NewLine}{JsonUtil.Serialize(itemsProxy)}");
                DialogService.Instance.ShowError(HardwareTest_Lang.Hardware_SaveConfigFailed);
                return;
            }

            IsDoing = false;
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Set All Successful: {JsonUtil.Serialize(itemsProxy)}");
            DialogService.Instance.ShowInfo("Set All Successful");
        }

        private void OnComponentCycleStatusChanged(object sender, CycleStatusArgs arg)
        {
            foreach (var module in arg.Device.Detector.DetectorModules)
            {
                var item = Detectors.FirstOrDefault(i => i.DetectorIndex == module.Number);

                if (item == null)
                {
                    continue;
                }

                for (int i = 0; i < module.DetectBoards.Length; i++)
                {
                    if (item.Detects.Length > i)
                    {
                        item.Detects[i].UpTemp = Math.Round(((double)module.DetectBoards[i].Chip1Temperature).ReduceTen(), 1);
                        item.Detects[i].DownTemp = Math.Round(((double)module.DetectBoards[i].Chip2Temperature).ReduceTen(), 1);
                    }
                }

                for (int i = 0; i < module.TemperatureControlBoard.Powers.Length; i++)
                {
                    if (item.Detects.Length > i)
                    {
                        item.Detects[i].Power = module.TemperatureControlBoard.Powers[i];
                    }
                }
            }
        }

        #region Register And Navigation

        private void RegisterProxyEvents()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Register events.");
            ComponentStatusProxy.Instance.CycleStatusChanged += OnComponentCycleStatusChanged;
        }

        private void UnRegisterProxyEvents()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorSetTemperature}] Un-register events.");
            ComponentStatusProxy.Instance.CycleStatusChanged -= OnComponentCycleStatusChanged;
        }

        public override void BeforeNavigateToCurrentPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorComponent}] Enter [Set Temperature] testing page.");
            RegisterProxyEvents();
        }

        public override void BeforeNavigateToOtherPage()
        {
            UnRegisterProxyEvents();
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.DetectorComponent}] Leave [Set Temperature] testing page.");
        }

        #endregion
    }
}