using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.FacadeProxy.Common.Models.Components;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.ComponentHistory.Enums;
using NV.CT.Service.ComponentHistory.Extensions;
using NV.CT.Service.ComponentHistory.Models;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.Configuration;

namespace NV.CT.Service.ComponentHistory.ViewModels
{
    public partial class ComponentHistoryViewModel : ObservableObject
    {
        #region Field

        private readonly ILogService _logService;
        private readonly IDialogService _dialogService;
        private bool _isBusing;
        private ComponentCategoryModel _selectedComponentCategory;

        #endregion

        public ComponentHistoryViewModel(ILogService logService, IDialogService dialogService)
        {
            _logService = logService;
            _dialogService = dialogService;
            ComponentCategories = CreateComponentCategories();
            _selectedComponentCategory = ComponentCategories[0];
        }

        #region Property

        public bool IsBusing
        {
            get => _isBusing;
            set
            {
                if (SetProperty(ref _isBusing, value))
                {
                    _ = value ? ServiceToken.Take(ComponentHistoryWrapper.ServiceModuleName) : ServiceToken.Release(ComponentHistoryWrapper.ServiceModuleName);
                }
            }
        }

        public ComponentCategoryModel[] ComponentCategories { get; }

        public ComponentCategoryModel SelectedComponentCategory
        {
            get => _selectedComponentCategory;
            set => SetProperty(ref _selectedComponentCategory, value);
        }

        #endregion

        #region Command

        public async Task OnLoaded()
        {
            try
            {
                IsBusing = true;
                await GetSpecifiedCategoriesDeviceSNInfos(ComponentCategories);
            }
            finally
            {
                IsBusing = false;
            }
        }

        [RelayCommand]
        private async Task RefreshCategory(ComponentCategoryModel categoryItem)
        {
            try
            {
                IsBusing = true;
                await GetSpecifiedCategoriesDeviceSNInfos([categoryItem]);
            }
            finally
            {
                IsBusing = false;
            }
        }

        [RelayCommand]
        private async Task RefreshEntryItem(ComponentEntryItemModel entryItem)
        {
            try
            {
                IsBusing = true;
                entryItem.SetWhenReadingDeviceSN();
                var request = new SerialNumberInfoRequest { Index = entryItem.ID, ComponentType = entryItem.ComponentType };
                var getResult = await ComponentStatusProxy.Instance.GetComponentSerialNumberInfoAsync([request]);

                if (!getResult.Status)
                {
                    var errorCode = getResult.ErrorCodes.FirstOrDefault();
                    var errorStr = $"[{errorCode}] {errorCode.GetErrorCodeDescription()}";
                    _logService.Error(ServiceCategory.ComponentHistory, $"[{nameof(RefreshEntryItem)}] Error when get component {entryItem.SeniorName} {entryItem.Name} series number from proxy: {errorStr}");
                    entryItem.ReceivedErrorWhenGetDeviceSN(errorStr);
                    return;
                }

                var deviceInfo = getResult.Data?.FirstOrDefault(i => i.Type == entryItem.ComponentType && i.Index == entryItem.ID);

                if (deviceInfo == null)
                {
                    _logService.Error(ServiceCategory.ComponentHistory, $"[{nameof(RefreshEntryItem)}] Get component {entryItem.SeniorName} {entryItem.Name} series number failed: not matched");
                    entryItem.ReceivedErrorWhenGetDeviceSN("Get device series number failed: not matched");
                }
                else
                {
                    entryItem.ReceivedDeviceSN(deviceInfo.SerialNumber);
                }
            }
            finally
            {
                IsBusing = false;
            }
        }

        [RelayCommand]
        private void UpdateCategory(ComponentCategoryModel categoryItem)
        {
            var items = new List<ComponentEntryItemModel>();

            foreach (var entry in categoryItem.Entries)
            {
                items.AddRange(entry.GetCanUpdateEntryItems());
            }

            if (items.Count == 0)
            {
                _dialogService.ShowWarning("There are no items that need to be updated!");
                return;
            }

            var installTime = DateTime.Now;
            var msg = items.Select(i => $"{i.SeniorName} {i.Name} old SN: {i.LocalSN}, new SN: {i.DeviceSN}");

            if (SystemConfig.AddOrUpdateDeviceComponent(items.Select(i => i.ToComponentInfo(installTime))))
            {
                foreach (var item in items)
                {
                    item.TryUpdate(installTime);
                }

                ComponentService.NotifyComponentExchange(items.Select(i => i.ToComponentExchange()).ToList());
                _logService.Info(ServiceCategory.ComponentHistory, $"Update {categoryItem.CategoryType} successful: {string.Join("; ", msg)}");
                _dialogService.ShowInfo("Update successful, please redo the related calibration items.");
            }
            else
            {
                _logService.Warn(ServiceCategory.ComponentHistory, $"Update {categoryItem.CategoryType} failed: failed to update file. {string.Join("; ", msg)}");
                _dialogService.ShowError("Update file failed!");
            }
        }

        [RelayCommand]
        private void UpdateEntryItem(ComponentEntryItemModel entryItem)
        {
            var oldSN = entryItem.LocalSN;
            var newSN = entryItem.DeviceSN;
            var installTime = DateTime.Now;

            if (SystemConfig.AddOrUpdateDeviceComponent(entryItem.ComponentType.ToDeviceComponentType(), entryItem.ToComponentInfo(installTime)))
            {
                entryItem.TryUpdate(installTime);
                ComponentService.NotifyComponentExchange([entryItem.ToComponentExchange()]);
                _logService.Info(ServiceCategory.ComponentHistory, $"Update {entryItem.SeniorName} {entryItem.Name} successful, old SN: {oldSN}, new SN: {newSN}");
                _dialogService.ShowInfo("Update successful, please redo the related calibration items.");
            }
            else
            {
                _logService.Warn(ServiceCategory.ComponentHistory, $"Update {entryItem.SeniorName} {entryItem.Name} failed: failed to update file. old SN: {oldSN}, new SN: {newSN}");
                _dialogService.ShowError("Update file failed!");
            }
        }

        #endregion

        private ComponentCategoryModel[] CreateComponentCategories()
        {
            return
            [
                new()
                {
                    Name = "XRay Source",
                    CategoryType = ComponentCategoryType.XRaySource,
                    ComponentTypes = [SerialNumberComponentType.XRaySourceTankbox, SerialNumberComponentType.XRaySourceBuckbox],
                    Entries =
                    [
                        ..Enumerable.Range(1, (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount)
                                    .Select(i =>
                                     {
                                         var currentTank = SystemConfig.DeviceComponentConfig.XRaySourceTankboxes.FirstOrDefault(x => x.Id == i);
                                         var currentBuck = SystemConfig.DeviceComponentConfig.XRaySourceBuckboxes.FirstOrDefault(x => x.Id == i);
                                         var name = $"XRay Source {i}";
                                         return new ComponentModuleEntryModel
                                         {
                                             Name = name,
                                             ModuleIndex = i,
                                             EntryItems =
                                             [
                                                 CreateComponentSNModel(name, "Tank Box", i, SerialNumberComponentType.XRaySourceTankbox, currentTank),
                                                 CreateComponentSNModel(name, "Buck Box", i, SerialNumberComponentType.XRaySourceBuckbox, currentBuck),
                                             ]
                                         };
                                     }),
                    ],
                },
                new()
                {
                    Name = "Collimator",
                    CategoryType = ComponentCategoryType.Collimator,
                    ComponentTypes = [SerialNumberComponentType.Collimator],
                    Entries =
                    [
                        ..Enumerable.Range(1, SystemConfig.CollimatorConfig.CollimatorSetting.ModuleCount.Value)
                                    .Select(i =>
                                     {
                                         var current = SystemConfig.DeviceComponentConfig.Collimators.FirstOrDefault(x => x.Id == i);
                                         return new ComponentSingleEntryModel
                                         {
                                             EntryItem = CreateComponentSNModel("Collimator", i.ToString(), i, SerialNumberComponentType.Collimator, current)
                                         };
                                     }),
                    ],
                },
                new()
                {
                    Name = "Detector Module",
                    CategoryType = ComponentCategoryType.DetectorModule,
                    ComponentTypes = [SerialNumberComponentType.TransmitBoard, SerialNumberComponentType.TemperatureControlBoard, SerialNumberComponentType.DetectorUnit],
                    Entries =
                    [
                        ..Enumerable.Range(1, SystemConfig.DetectorConfig.Detector.XModuleCount.Value)
                                    .Select(i =>
                                     {
                                         var name = $"Detector Module {i}";
                                         var currentTrans = SystemConfig.DeviceComponentConfig.TransmissionBoards.FirstOrDefault(x => x.Id == i);
                                         var currentTemp = SystemConfig.DeviceComponentConfig.TemperatureControlBoards.FirstOrDefault(x => x.Id == i);
                                         return new ComponentModuleEntryModel
                                         {
                                             Name = name,
                                             ModuleIndex = i,
                                             EntryItems =
                                             [
                                                 CreateComponentSNModel(name, "Transmit Board", i, SerialNumberComponentType.TransmitBoard, currentTrans),
                                                 CreateComponentSNModel(name, "TempCtrl Board", i, SerialNumberComponentType.TemperatureControlBoard, currentTemp),
                                                 ..Enumerable.Range(1, 4)
                                                             .Select(x =>
                                                              {
                                                                  var id = (i - 1) * 4 + x;
                                                                  var currentUnit = SystemConfig.DeviceComponentConfig.DetectorUnits.FirstOrDefault(m => m.Id == id);
                                                                  return CreateComponentSNModel(name, $"Detector Unit {(id - 1) % 4 + 1}", id, SerialNumberComponentType.DetectorUnit, currentUnit);
                                                              }),
                                             ]
                                         };
                                     })
                    ],
                },
            ];
        }

        private ComponentEntryItemModel CreateComponentSNModel(string seniorName, string name, int id, SerialNumberComponentType type, ComponentInfo? info)
        {
            return new()
            {
                SeniorName = seniorName,
                Name = name,
                ID = id,
                ComponentType = type,
                InstallTime = info?.UsingBeginTime ?? default,
                LocalSN = info?.SerialNumber ?? string.Empty,
                DeviceSN = string.Empty,
                ReadDeviceSNStatus = ReadDeviceSNStatus.Reading,
                ReadDeviceSNStr = "Reading...",
            };
        }

        private async Task GetSpecifiedCategoriesDeviceSNInfos(ComponentCategoryModel[] categories, [CallerMemberName] string? methodName = null)
        {
            var allEntryItems = categories.SelectMany(i => i.GetAllEntryItems()).ToList();

            foreach (var entryItem in allEntryItems)
            {
                entryItem.SetWhenReadingDeviceSN();
            }

            var types = categories.SelectMany(i => i.ComponentTypes);
            var getResult = await ComponentStatusProxy.Instance.GetComponentSerialNumberInfoAsync(types);

            if (!getResult.Status)
            {
                var errorCode = getResult.ErrorCodes.FirstOrDefault();
                var errorStr = $"[{errorCode}] {errorCode.GetErrorCodeDescription()}";
                _logService.Error(ServiceCategory.ComponentHistory, $"[{methodName}] Error when get devices series number from proxy: {errorStr}");

                foreach (var entryItem in allEntryItems)
                {
                    entryItem.ReceivedErrorWhenGetDeviceSN(errorStr);
                }

                return;
            }

            foreach (var category in categories)
            {
                var entryItems = category.GetAllEntryItems();
                var deviceInfos = getResult.Data?.Where(i => category.ComponentTypes.Contains(i.Type)).ToList();

                if (deviceInfos is not { Count: > 0 })
                {
                    _logService.Error(ServiceCategory.ComponentHistory, $"[{methodName}] Get component category {category.CategoryType} series number failed: not matched");

                    foreach (var entryItem in entryItems)
                    {
                        entryItem.ReceivedErrorWhenGetDeviceSN("Get device series number failed: not matched");
                    }

                    continue;
                }

                foreach (var entryItem in entryItems)
                {
                    var deviceInfo = deviceInfos.FirstOrDefault(i => i.Type == entryItem.ComponentType && i.Index == entryItem.ID);

                    if (deviceInfo == null)
                    {
                        _logService.Error(ServiceCategory.ComponentHistory, $"[{methodName}] Get component {entryItem.SeniorName} {entryItem.Name} series number failed: not matched");
                        entryItem.ReceivedErrorWhenGetDeviceSN("Get device series number failed: not matched");
                    }
                    else
                    {
                        entryItem.ReceivedDeviceSN(deviceInfo.SerialNumber);
                    }
                }
            }
        }
    }
}