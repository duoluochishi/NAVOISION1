using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.EventArguments;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.HardwareTest.Models.Integrations.SelfCheck;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.ViewModels.Foundations;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NV.CT.Service.HardwareTest.ViewModels.Integrations.SelfCheck
{
    public partial class SelfCheckTestingViewModel : NavigationViewModelBase
    {
        #region Field

        private readonly ILogService _logService;

        #endregion

        public SelfCheckTestingViewModel(ILogService logService)
        {
            _logService = logService;
            Initialize();
        }

        #region Property

        [ObservableProperty]
        private bool _isDoing;

        [ObservableProperty]
        private bool _isAllChecked;

        public ObservableCollection<SelfCheckPartModel> Items { get; set; }

        #endregion

        #region Command

        [RelayCommand]
        private void CheckedChanged()
        {
            IsAllChecked = Items.All(i => i.IsChecked);
        }

        [RelayCommand]
        private void AllCheckedChanged()
        {
            foreach (var item in Items)
            {
                item.IsChecked = IsAllChecked;
            }
        }

        [RelayCommand]
        private void Start()
        {
            SelfCheckPartType? startTypes = null;

            foreach (var item in Items)
            {
                if (!item.IsChecked)
                {
                    continue;
                }

                if (startTypes == null)
                {
                    startTypes = item.PartType;
                }
                else
                {
                    startTypes |= item.PartType;
                }
            }

            if (startTypes == null)
            {
                return;
            }

            try
            {
                IsDoing = true;
                var res = SelfCheckProxy.Instance.RunSelfCheck(startTypes.Value);

                if (res.Status == CommandStatus.Success)
                {
                    _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Start self check : [{startTypes.Value}]");
                }
                else
                {
                    IsDoing = false;
                    var errorCode = res.ErrorCodes.Codes.First();
                    _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Start self check failed: [{errorCode}] {errorCode.GetErrorCodeDescription()}");
                    DialogService.Instance.ShowErrorCode(errorCode);
                }
            }
            catch (Exception e)
            {
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Start self check exception.", e);
                DialogService.Instance.ShowError($"Start failed: {e.Message}");
            }
        }

        #endregion

        #region Method

        [MemberNotNull(nameof(Items))]
        private void Initialize()
        {
            var detailedTypes = Enum.GetValues<DetailedSelfCheckItemType>();
            Items = new(Enum.GetValues<SelfCheckPartType>()
                            .Select(i => new SelfCheckPartModel()
                             {
                                 PartType = i,
                                 PartName = GetDescription(i),
                                 #if DEBUG
                                 Status = (SelfCheckStatus)Random.Shared.Next(0, 5),
                                 #else
                                 Status = SelfCheckStatus.Unknown,
                                 #endif
                                 DetailedItems =
                                 [
                                     ..detailedTypes.Where(x => x.ToString().StartsWith(Regex.Replace(i.ToString(), @"\d*$", string.Empty), StringComparison.OrdinalIgnoreCase))
                                                    .Select(x => new SelfCheckPartDetailedModel()
                                                     {
                                                         ItemType = x,
                                                         ItemName = GetDescription(x),
                                                         #if DEBUG
                                                         Status = (SelfCheckStatus)Random.Shared.Next(0, 5),
                                                         #else
                                                         Status = SelfCheckStatus.Unknown,
                                                         #endif
                                                     })
                                 ],
                             })
                            .Where(i => i.DetailedItems is { Count : > 0 }));

            string GetDescription<T>(T value) where T : Enum => typeof(T).GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
        }

        private void GetAllStatus()
        {
            try
            {
                var infos = SelfCheckProxy.Instance.GetCompleteSelfCheckInfos().ToList();
                _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Get all parts self-check status: {JsonUtil.Serialize(infos)}");

                foreach (var info in infos)
                {
                    var item = Items.FirstOrDefault(i => i.PartType == info.PartType);

                    if (item == null)
                    {
                        continue;
                    }

                    item.Status = info.Status;

                    foreach (var detailedInfo in info.DetailedSelfCheckInfos)
                    {
                        var detailedItem = item.DetailedItems.FirstOrDefault(i => i.ItemType == detailedInfo.ItemType);

                        if (detailedItem == null)
                        {
                            continue;
                        }

                        detailedItem.Status = detailedInfo.Status;
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Error(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Get all status exception.", e);
            }
        }

        private void OnTotalSelfCheckStatusChanged(object? sender, TotalSelfCheckStatusEventArgs e)
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Received total self check status changed: {e.TotalSelfCheckStatus}");

            if (e.TotalSelfCheckStatus is not (SelfCheckStatus.Unknown or SelfCheckStatus.InProgress))
            {
                IsDoing = false;
            }
        }

        private void OnSelfCheckStatusChanged(object? sender, SelfCheckEventArgs e)
        {
            var item = Items.FirstOrDefault(i => i.PartType == e.SelfCheckInfo.PartType);

            if (item == null)
            {
                return;
            }

            item.Status = e.SelfCheckInfo.Status;

            foreach (var detailedInfo in e.SelfCheckInfo.DetailedSelfCheckInfos)
            {
                var detailedItem = item.DetailedItems.FirstOrDefault(i => i.ItemType == detailedInfo.ItemType);

                if (detailedItem == null)
                {
                    continue;
                }

                detailedItem.Status = detailedInfo.Status;
            }
        }

        #endregion

        #region Register

        private void RegisterProxyEvents()
        {
            SelfCheckProxy.Instance.TotalSelfCheckStatusChanged += OnTotalSelfCheckStatusChanged;
            SelfCheckProxy.Instance.SelfCheckStatusChanged += OnSelfCheckStatusChanged;
        }

        private void UnRegisterProxyEvents()
        {
            SelfCheckProxy.Instance.TotalSelfCheckStatusChanged -= OnTotalSelfCheckStatusChanged;
            SelfCheckProxy.Instance.SelfCheckStatusChanged -= OnSelfCheckStatusChanged;
        }

        #endregion

        #region Navigation

        public override void BeforeNavigateToCurrentPage()
        {
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Enter [Self Check] testing page.");
            GetAllStatus();
            RegisterProxyEvents();
        }

        public override void BeforeNavigateToOtherPage()
        {
            UnRegisterProxyEvents();
            _logService.Info(ServiceCategory.HardwareTest, $"[{ComponentDefaults.SelfCheck}] Leave [Self Check] testing page.");
        }

        #endregion
    }
}