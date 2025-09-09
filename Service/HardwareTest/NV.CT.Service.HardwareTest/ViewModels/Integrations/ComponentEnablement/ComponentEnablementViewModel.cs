using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ShieldableComponent;
using NV.CT.FacadeProxy.Common.Models.ShieldableComponent;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.HardwareTest.Models.Integrations.ComponentEnablement;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using NV.MPS.Configuration;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.ComponentEnablement
{
    public partial class ComponentEnablementViewModel : NavigationViewModelBase
    {
        private readonly ILogService _logService;

        public ComponentEnablementViewModel(ILogService logService)
        {
            _logService = logService;
            Components =
            [
                new()
                {
                    Name = "XRay Source",
                    ComponentType = ShieldableComponentType.XRaySource,
                    DetailedItems = Enumerable.Range(1, (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount)
                                              .Select(i => new DetailedEnableModel
                                               {
                                                   Name = i.ToString(),
                                                   Index = i,
                                                   ComponentType = ShieldableComponentType.XRaySource,
                                                   IsEnabled = false,
                                               })
                                              .ToArray(),
                },
                new()
                {
                    Name = "Collimator",
                    ComponentType = ShieldableComponentType.Collimator,
                    DetailedItems = Enumerable.Range(1, SystemConfig.CollimatorConfig.CollimatorSetting.ModuleCount.Value)
                                              .Select(i => new DetailedEnableModel
                                               {
                                                   Name = i.ToString(),
                                                   Index = i,
                                                   ComponentType = ShieldableComponentType.Collimator,
                                                   IsEnabled = false,
                                               })
                                              .ToArray(),
                },
                /* TODO:探测器的使能，ACQ那边还没实现，暂时屏蔽
                new()
                {
                    Name = "Detector Module",
                    ComponentType = ShieldableComponentType.Detector,
                    DetailedItems = Enumerable.Range(1, SystemConfig.DetectorConfig.Detector.XModuleCount.Value)
                                              .Select(i => new DetailedEnableModel
                                               {
                                                   Name = i.ToString(),
                                                   Index = i,
                                                   ComponentType = ShieldableComponentType.Detector,
                                                   IsEnabled = false,
                                               })
                                              .ToArray(),
                },
                */
            ];
        }

        public ComponentModel[] Components { get; }

        [RelayCommand]
        private void EnableCheck(DetailedEnableModel model)
        {
            var component = Components.First(i => i.ComponentType == model.ComponentType);
            component.IsAllEnabled = component.DetailedItems.All(i => i.IsEnabled);
        }

        [RelayCommand]
        private void AllEnableCheck(ComponentModel model)
        {
            foreach (var item in model.DetailedItems)
            {
                item.IsEnabled = model.IsAllEnabled;
            }
        }

        [RelayCommand]
        private async Task Set()
        {
            var infos = Components.Select(i => new ComponentsShieldableInfo
            {
                ComponentType = i.ComponentType,
                ComponentShieldableFlags = i.DetailedItems.Select(x => x.IsEnabled).ToList(),
            });
            var commandResult = await Task.Run(() => DeviceInteractProxy.Instance.ConfigureShieldableComponents(infos));

            if (commandResult.Status == CommandStatus.Success)
            {
                _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}] Set Successful{Environment.NewLine}{JsonUtil.Serialize(infos)}");
                DialogService.Instance.ShowInfo(HardwareTest_Lang.Hardware_SetSuccess);
            }
            else
            {
                var errorCode = commandResult.ErrorCodes.Codes.FirstOrDefault();
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}] Set Failed: [{errorCode}] {errorCode.GetErrorCodeDescription()}{Environment.NewLine}{JsonUtil.Serialize(infos)}");
                DialogService.Instance.ShowErrorCode(errorCode!);
            }
        }

        private async Task GetComponentEnablement()
        {
            var infos = await Task.Run(() => DeviceInteractProxy.Instance.GetShieldableComponentsInfos(Components.Select(i => i.ComponentType)));

            if (infos == null)
            {
                foreach (var component in Components)
                {
                    component.IsAllEnabled = false;

                    foreach (var item in component.DetailedItems)
                    {
                        item.IsEnabled = false;
                    }
                }

                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}][{nameof(DeviceInteractProxy.Instance.GetShieldableComponentsInfos)}] Get failed, received null.");
                DialogService.Instance.ShowError("Get hardware status failed.");
                return;
            }

            foreach (var component in Components)
            {
                var currentInfo = infos.FirstOrDefault(i => i.ComponentType == component.ComponentType);

                if (currentInfo == null)
                {
                    _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}][{nameof(DeviceInteractProxy.Instance.GetShieldableComponentsInfos)}] {component.ComponentType} not matched.");
                    continue;
                }

                var infoCount = currentInfo.ComponentShieldableFlags.Count;

                if (infoCount != component.DetailedItems.Length)
                {
                    _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}][{nameof(DeviceInteractProxy.Instance.GetShieldableComponentsInfos)}] {component.ComponentType} unmatched count, need {currentInfo.ComponentShieldableFlags.Count}, received {infoCount}.");
                }

                foreach (var item in component.DetailedItems)
                {
                    item.IsEnabled = infoCount >= item.Index && currentInfo.ComponentShieldableFlags[item.Index - 1];
                }

                component.IsAllEnabled = component.DetailedItems.All(i => i.IsEnabled);
            }
        }

        #region Navigation

        public override async void BeforeNavigateToCurrentPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}] Enter [Component Enablement] testing page.");
            await GetComponentEnablement();
        }

        public override void BeforeNavigateToOtherPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.ComponentEnablement}] Leave [Component Enablement] testing page.");
        }

        #endregion
    }
}