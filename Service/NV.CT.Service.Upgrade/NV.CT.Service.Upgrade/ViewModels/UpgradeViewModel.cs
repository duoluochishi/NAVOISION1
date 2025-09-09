using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.EventArguments;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Models.Upgrade;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.Upgrade.Enums;
using NV.CT.Service.Upgrade.Extensions;
using NV.CT.Service.Upgrade.Models;
using NV.CT.Service.Upgrade.Services.Interfaces;
using NV.CT.Service.Upgrade.Utilities;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;

namespace NV.CT.Service.Upgrade.ViewModels
{
    public partial class UpgradeViewModel : ObservableObject
    {
        #region Field

        private string _upgradeFolder = string.Empty;
        private string _stepMessage = string.Empty;
        private uint _checkedCount;
        private uint _totalCount;
        private uint _succeededCount;
        private uint _failedCount;
        private double _totalProgress;
        private bool _isAllChecked;
        private bool _isNotUpgradeWhenSameVer;
        private bool _isUpgrading;
        private ObservableCollection<FirmwareTypeModel> _firmwareTypes;
        private ObservableCollection<FirmwareModel> _firmwareTree;
        private ObservableCollection<FirmwareModel> _firmwareList = new();

        /// <summary>
        /// 配置项
        /// </summary>
        private readonly UpgradeConfigModel _config;

        /// <summary>
        /// 定时器，定时循环获取信息（版本号、是否可升级等）
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// 真正进行升级的列表
        /// </summary>
        private List<FirmwareModel> _upgradeList = new();

        /// <summary>
        /// pdu重上电后，需要重新读取升级项目的版本号
        /// </summary>
        private volatile bool _isGetUpgradeItemsVersion;

        #endregion

        public UpgradeViewModel(ILogService logService, IDialogService dialogService, IConfigService configService)
        {
            Log = logService;
            Dialog = dialogService;
            _config = configService.GetConfig();
            _firmwareTypes = new ObservableCollection<FirmwareTypeModel>(configService.GetFwTypes());
            _firmwareTree = new ObservableCollection<FirmwareModel>(configService.GetFws());
            var idValue = 0;
            var canUpgradeFileList = configService.GetFwCanUpgradeFiles().ToList();
            InitFirmwares(FirmwareTree, null, canUpgradeFileList, ref idValue);
            FilterFirmwareTypes = _firmwareList.GroupBy(i => i.FirmwareType)
                                               .Select(i => new FirmwareTypeModel()
                                                {
                                                    FirmwareType = i.Key,
                                                    IsChecked = true,
                                                    IsEnabled = true,
                                                })
                                               .ToList();
            UpgradeFolder = RuntimeConfig.Console.Firmware.Path;
            _timer = new Timer(1000) { AutoReset = true, Enabled = false, };
        }

        #region Property

        public IDialogService Dialog { get; }
        public ILogService Log { get; }
        public List<FirmwareTypeModel> FilterFirmwareTypes { get; set; }

        public string UpgradeFolder
        {
            get => _upgradeFolder;
            set => SetProperty(ref _upgradeFolder, value);
        }

        public string StepMessage
        {
            get => _stepMessage;
            set => SetProperty(ref _stepMessage, value);
        }

        public uint CheckedCount
        {
            get => _checkedCount;
            set => SetProperty(ref _checkedCount, value);
        }

        public uint TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public uint SucceededCount
        {
            get => _succeededCount;
            set => SetProperty(ref _succeededCount, value);
        }

        public uint FailedCount
        {
            get => _failedCount;
            set => SetProperty(ref _failedCount, value);
        }

        public double TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        public bool IsAllChecked
        {
            get => _isAllChecked;
            set => SetProperty(ref _isAllChecked, value);
        }

        public bool IsNotUpgradeWhenSameVer
        {
            get => _isNotUpgradeWhenSameVer;
            set => SetProperty(ref _isNotUpgradeWhenSameVer, value);
        }

        public bool IsUpgrading
        {
            get => _isUpgrading;
            set
            {
                if (SetProperty(ref _isUpgrading, value))
                {
                    _ = value ? ServiceToken.Take(Global.ServiceAppName) : ServiceToken.Release(Global.ServiceAppName);
                }
            }
        }

        public ObservableCollection<FirmwareTypeModel> FirmwareTypes
        {
            get => _firmwareTypes;
            set => SetProperty(ref _firmwareTypes, value);
        }

        /// <summary>
        /// 树型固件列表，包含节点项
        /// </summary>
        public ObservableCollection<FirmwareModel> FirmwareTree
        {
            get => _firmwareTree;
            set => SetProperty(ref _firmwareTree, value);
        }

        /// <summary>
        /// 列表型固件列表，不包含节点项
        /// </summary>
        public ObservableCollection<FirmwareModel> FirmwareList
        {
            get => _firmwareList;
            set => SetProperty(ref _firmwareList, value);
        }

        #endregion

        #region Loaded/Close/Check

        public void OnLoaded()
        {
            UnRegisterAll();
            RegisterEvent();
            _timer.Start();
        }

        public void OnContentRendered()
        {
            AnalyzeUpgradeFolder(UpgradeFolder);
        }

        public void OnUnLoaded()
        {
            UnRegisterAll();
            _timer.Stop();
        }

        public string OnMCSClosing()
        {
            if (IsUpgrading)
            {
                return Upgrade_Lang.Upgrade_Leave_InUpgrading;
            }

            var items = FirmwareList.Where(i => i is { IsEverUpgraded: true, UpgradeStatus: UpgradeStatusType.Fail }).ToList();

            if (items.Count > 0)
            {
                return Upgrade_Lang.Upgrade_Leave_Confirm;
            }

            return string.Empty;
        }

        [RelayCommand]
        private void OnFirmwareTypeClicked(FirmwareTypeModel fwTypeModel)
        {
            fwTypeModel.IsChecked ??= false;
            var typeFirmwares = FirmwareList.Where(i => i.FirmwareType == fwTypeModel.FirmwareType && i.IsEnabled).ToList();

            foreach (var item in typeFirmwares)
            {
                if (item.IsChecked == fwTypeModel.IsChecked)
                    continue;

                item.IsChecked = fwTypeModel.IsChecked;

                if (item.Children is { Count: > 0 })
                {
                    SetChildrenIsChecked(item.Children, item.IsChecked.Value);
                }
            }

            foreach (var parentGroup in typeFirmwares.Where(i => i.Parent != null).GroupBy(i => i.Parent))
            {
                SetParentIsChecked(parentGroup.Key!);
            }

            UpdateFirmwareTypeIsChecked();
            UpdateCheckedCountAndIsAllChecked();
        }

        [RelayCommand]
        private void OnFirmwareClicked(FirmwareModel fwModel)
        {
            fwModel.IsChecked ??= false;

            if (fwModel.Children is { Count: > 0 })
            {
                SetChildrenIsChecked(fwModel.Children, fwModel.IsChecked.Value);
            }

            if (fwModel.Parent != null)
            {
                SetParentIsChecked(fwModel.Parent);
            }

            UpdateFirmwareTypeIsChecked();
            UpdateCheckedCountAndIsAllChecked();
        }

        [RelayCommand]
        private void OnAllClicked()
        {
            UpdateAllIsChecked(IsAllChecked);
        }

        #endregion

        #region Upgrade

        [RelayCommand]
        private async Task OnUpgrade()
        {
            var checkedItems = FirmwareList.Where(i => i is { IsChecked: true, IsEnabled: true }).ToList();

            if (checkedItems.Count == 0)
            {
                return;
            }

            if (DeviceSystem.Instance.RealtimeStatus == RealtimeStatus.OnlineUpgrading)
            {
                UpgradeEnd(Upgrade_Lang.Upgrade_StepInfo_InUpgrading, Upgrade_Lang.Upgrade_Upgrade_DialogMsg_InUpgrading, MessageLeveles.Warning);
                return;
            }

            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Start");
            BeforeUpgrade();
            _upgradeList = GetUpgradeList(checkedItems);

            if (_upgradeList.Count == 0)
            {
                UpgradeEnd(Upgrade_Lang.Upgrade_StepInfo_NoCanUpgrade, Upgrade_Lang.Upgrade_Upgrade_DialogMsg_NoCanUpgrade, MessageLeveles.Warning);
                return;
            }

            await SendUpgradePrepareCommand();
        }

        private async Task SendUpgradePrepareCommand()
        {
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Send hardware upgrade prepare command");
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_SendUpgradePrepareCommand;
            var res = await UpgradeProxy.Instance.UpgradePrepare();

            if (res.Result.Status != CommandStatus.Success)
            {
                if (res.IsRestartPower)
                {
                    await FailedBeforeUpgrade(res.Result.ErrorCodes, "Hardware prepare command error", "Send hardware upgrade prepare command error", Upgrade_Lang.Upgrade_Upgrade_DialogMsg_Part_PrepareCommandError);
                }
                else
                {
                    UpdateUpgradeStatusWhenFailed(res.Result.ErrorCodes, "Hardware prepare command error");
                    UpgradeEnd(Upgrade_Lang.Upgrade_StepInfo_PrepareCommandError, res.Result.ErrorCodes, "send hardware upgrade prepare command error");
                }

                return;
            }

            TotalProgress = 10;
            WaitUpgradePrepareResult();
        }

        private void WaitUpgradePrepareResult()
        {
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Wait hardware upgrade prepare result");
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_WaitPrepareResult;
            UnRegisterUpgradePrepareEvent();
            RegisterUpgradePrepareEvent(); //订阅Proxy相关事件，从事件中获取硬件升级前准备的结果，并进行下一步处理
        }

        private async Task SetCTBoxToUpgradingStatus()
        {
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Set ctbox to upgrading status");
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_SetUpgradingStatus;
            var setRes = await UpgradeProxy.Instance.SetUpgradingStatus();
            TotalProgress = 30;

            if (setRes.Status != CommandStatus.Success)
            {
                await FailedBeforeUpgrade(setRes.ErrorCodes, "Set ctbox to upgrading status error", "Set ctbox to upgrading status error", Upgrade_Lang.Upgrade_Upgrade_DialogMsg_Part_SetUpgradingStatusError);
                return;
            }

            await Upgrade();
        }

        private async Task Upgrade()
        {
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Upgrading...");
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_Upgrading;
            await UpgradeProxy.Instance.UpgradeFirmwares(_upgradeList.Select(i => i.ToProto()).ToList());
            DeleteUnzipFolders();
            TotalProgress = 70;

            if (_upgradeList.Any(i => i is { FirmwareType: FirmwareType.Pdu, UpgradeStatus: UpgradeStatusType.UpgradeCompletedAndWaitRestart }))
            {
                WaitPowerRestartAndSelfCheckResult();
                Dialog.ShowInfo(Upgrade_Lang.Upgrade_Upgrade_DialogMsg_RestartPowreManually);
            }
            else
            {
                await SendPowerRestartCommand();
            }
        }

        private async Task SendPowerRestartCommand()
        {
            var res = await SendPowerRestartCommandOnly();
            WaitPowerRestartAndSelfCheckResult();

            if (res.Status != CommandStatus.Success)
            {
                var errorCode = res.ErrorCodes.Codes.FirstOrDefault();
                var errorDesc = errorCode.GetErrorCodeDescription();
                Dialog.ShowWarning(string.Format(Upgrade_Lang.Upgrade_Upgrade_DialogMsg_RestartPowerCommandError, $"[{errorCode}] {errorDesc}"));
            }
        }

        private void WaitPowerRestartAndSelfCheckResult()
        {
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_WaitPowerRestartAndSelfCheckResult;

            if (ComponentStatusProxy.Instance.CTBoxConnected)
            {
                Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] CTBox still connected, Wait ctbox disconnect");
                UnRegisterCtBoxConnectedEvent();
                RegisterCtBoxConnectedEvent(); //订阅CTBox连接事件，当CTBox断联后，再订阅自检的相关事件
            }
            else
            {
                Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Wait power restart and hardware self check result");
                UnRegisterSelfCheckEvent();
                RegisterSelfCheckEvent(); //订阅Proxy相关事件，从事件中获取自检的结果，并进行下一步处理
            }
        }

        private void SetUpgradeStatusAfterCompareVersion()
        {
            // 在 OnVersionReceived 中，已经实时对每一个板子比对了版本号并处理了状态，本方法只需根据状态判断最终升级结果即可
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Set upgrade status after compare upgrade items version");

            foreach (var item in _upgradeList)
            {
                if (item.UpgradeStatus == UpgradeStatusType.Fail)
                {
                    continue;
                }

                switch (item.GetVerStatus)
                {
                    case GetVersionStatusType.Success:
                    {
                        item.UpgradeStatus = UpgradeStatusType.Success;
                        item.UpgradeMsg = string.Empty;
                        break;
                    }
                    case GetVersionStatusType.Warning:
                    {
                        item.UpgradeStatus = UpgradeStatusType.Fail;
                        item.UpgradeMsg = Upgrade_Lang.Upgrade_Upgrade_Fail_VerNotMatch;
                        break;
                    }
                    case GetVersionStatusType.Error:
                    {
                        item.UpgradeStatus = UpgradeStatusType.Fail;
                        item.UpgradeMsg = Upgrade_Lang.Upgrade_Upgrade_Fail_GetVerError;
                        break;
                    }
                }
            }

            UpdateUpgradeCount();
            UpgradeEnd(Upgrade_Lang.Upgrade_StepInfo_Completed, $"{Upgrade_Lang.Upgrade_Upgrade_Completed}{GetFailedStr()}", MessageLeveles.Info);
        }

        private void BeforeUpgrade()
        {
            _upgradeList.Clear();
            _isGetUpgradeItemsVersion = false;
            TotalProgress = 0;
            SucceededCount = 0;
            FailedCount = 0;
            IsUpgrading = true;
        }

        private List<FirmwareModel> GetUpgradeList(IEnumerable<FirmwareModel> checkedItems)
        {
            var upgradeList = new List<FirmwareModel>();

            foreach (var item in checkedItems)
            {
                if (IsNotUpgradeWhenSameVer && item.CurrentVersion == item.UpgradeVersion)
                {
                    item.UpgradeMsg = string.Empty;
                    item.UpgradeStatus = UpgradeStatusType.Success;
                    item.Progress = 100;
                    UpdateUpgradeCount();
                    continue;
                }

                if (!item.IsCanUpgrade)
                {
                    item.UpgradeMsg = Global.ErrorCode_NotConnect.GetErrorCodeDescription();
                    item.UpgradeStatus = UpgradeStatusType.Fail;
                    item.Progress = 0;
                    UpdateUpgradeCount();
                    continue;
                }

                item.UpgradeMsg = string.Empty;
                item.UpgradeStatus = UpgradeStatusType.Upgrading;
                item.Progress = 0;
                item.IsEverUpgraded = true;
                upgradeList.Add(item);
            }

            return upgradeList;
        }

        private async Task<CommandResult> SendPowerRestartCommandOnly()
        {
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Send power restart command");
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_SendPowerRestartCommand;
            var ret = await UpgradeProxy.Instance.PowerRestart();
            TotalProgress = 75;

            if (ret.Status != CommandStatus.Success)
            {
                var errorCode = ret.ErrorCodes.Codes.FirstOrDefault();
                var errorDesc = errorCode.GetErrorCodeDescription();
                Log.Error(ServiceCategory.UpgradeFirmware, $"[Upgrade] Send power restart command error: [{errorCode}] {errorDesc}");
            }

            return ret;
        }

        private void DeleteUnzipFolders()
        {
            var folders = _upgradeList.Select(i => Path.ChangeExtension(i.PackagePath, null)).Distinct();

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception e)
                {
                    Log.Error(ServiceCategory.UpgradeFirmware, $"[Upgrade] Delete unzip folder \"{folder}\" error", e);
                }
            }
        }

        private (string?, string?) UpdateUpgradeStatusWhenFailed(FacadeProxy.Common.Models.ErrorCodes errorCodes, string upgradeMsgPrefix)
        {
            var errorCode = errorCodes.Codes.FirstOrDefault();
            var errorDesc = errorCode.GetErrorCodeDescription();

            foreach (var item in _upgradeList)
            {
                item.UpgradeStatus = UpgradeStatusType.Fail;
                item.UpgradeMsg = $"{upgradeMsgPrefix}: [{errorCode}] {errorDesc}";
            }

            UpdateUpgradeCount();
            return (errorCode, errorDesc);
        }

        private async Task FailedBeforeUpgrade(FacadeProxy.Common.Models.ErrorCodes errorCodes, string upgradeMsgPrefix, string logMsg, string dialogMsg)
        {
            var (errorCode, errorDesc) = UpdateUpgradeStatusWhenFailed(errorCodes, upgradeMsgPrefix);
            Log.Error(ServiceCategory.UpgradeFirmware, $"[Upgrade] {logMsg}: [{errorCode}] {errorDesc}");
            var res = await SendPowerRestartCommandOnly();
            WaitPowerRestartAndSelfCheckResult();

            if (res.Status == CommandStatus.Success)
            {
                Dialog.ShowWarning(string.Format(Upgrade_Lang.Upgrade_Upgrade_DialogMsg_FailedBeforeUpgrade, dialogMsg, $"[{errorCode}] {errorDesc}"));
            }
            else
            {
                var errorCode2 = res.ErrorCodes.Codes.FirstOrDefault();
                var errorDesc2 = errorCode2.GetErrorCodeDescription();
                Dialog.ShowWarning(string.Format(Upgrade_Lang.Upgrade_Upgrade_DialogMsg_FailedBeforeUpgradeAndREstartPowerError, dialogMsg, $"[{errorCode}] {errorDesc}", $"[{errorCode2}] {errorDesc2}"));
            }
        }

        private void UpgradeEnd(string stepMsg)
        {
            TotalProgress = 100;
            IsUpgrading = false;
            StepMessage = stepMsg;
            _upgradeList.Clear();
            _isGetUpgradeItemsVersion = false;
        }

        private void UpgradeEnd(string stepMsg, FacadeProxy.Common.Models.ErrorCodes errorCodes, string logInfo)
        {
            UpgradeEnd(stepMsg);
            var errorCode = errorCodes.Codes.FirstOrDefault();
            var errorDesc = errorCode.GetErrorCodeDescription();
            Log.Error(ServiceCategory.UpgradeFirmware, $"[Upgrade] End, {logInfo}: [{errorCode}] {errorDesc}");
            Dialog.ShowErrorCode(errorCode!);
        }

        private void UpgradeEnd(string stepMsg, string showMsg, MessageLeveles level)
        {
            UpgradeEnd(stepMsg);

            switch (level)
            {
                case MessageLeveles.Info:
                {
                    Log.Info(ServiceCategory.UpgradeFirmware, $"[Upgrade] End, {showMsg}");
                    Dialog.ShowInfo(showMsg);
                    return;
                }
                case MessageLeveles.Warning:
                {
                    Log.Warn(ServiceCategory.UpgradeFirmware, $"[Upgrade] End, {showMsg}");
                    Dialog.ShowWarning(showMsg);
                    return;
                }
                case MessageLeveles.Error:
                {
                    Log.Error(ServiceCategory.UpgradeFirmware, $"[Upgrade] End, {showMsg}");
                    Dialog.ShowError(showMsg);
                    return;
                }
            }
        }

        #endregion

        #region Event Received

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            var isGetUpgradeItemsVersion = _isGetUpgradeItemsVersion;
            ConfirmCanUpgrade(FirmwareList).GetAwaiter().GetResult();
            // var queryVersionList = FirmwareList.Where(i => i.IsCanUpgrade || (isGetUpgradeItemsVersion && _upgradeList.Contains(i)));
            var queryVersionList = FirmwareList; //查询版本号的固件，不再进行筛选，如果返回的某个固件版本号有问题，找对应的硬件负责人解决
            GetFirmwaresVersion(queryVersionList).GetAwaiter().GetResult();
            _timer.Start();

            if (isGetUpgradeItemsVersion)
            {
                _isGetUpgradeItemsVersion = false;
                TotalProgress = 90;
                SetUpgradeStatusAfterCompareVersion();
            }
        }

        private void OnVersionReceived(object? sender, VersionEventArgs e)
        {
            var item = FirmwareList.FirstOrDefault(i => i.ID == e.ID);

            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(e.Version)) //空值代表获取版本号失败
            {
                item.GetVerStatus = GetVersionStatusType.Error;
                item.GetVerMsg = $"[{e.Code}] {e.Code.GetErrorCodeDescription()}";
            }
            else
            {
                if (item.IsEnabled && e.Version != item.UpgradeVersion)
                {
                    item.GetVerStatus = GetVersionStatusType.Warning;
                    item.GetVerMsg = Upgrade_Lang.Upgrade_Info_VersionNotMatch;
                }
                else
                {
                    item.GetVerStatus = GetVersionStatusType.Success;
                    item.GetVerMsg = string.Empty;
                }
            }

            item.CurrentVersion = e.Version;
        }

        private void OnCanUpgradeReceived(object? sender, CanUpgradeEventArgs e)
        {
            var item = FirmwareList.FirstOrDefault(i => i.ID == e.ID);

            if (item == null)
            {
                return;
            }

            // 特殊情况下，当需要不检测可升级的状态时，解除这里的注释即可
            // item.IsCanUpgrade = true;
            // return;

            if (!e.CanUpgrade)
            {
                item.CurrentVersion = string.Empty;
                item.GetVerStatus = GetVersionStatusType.None;
                item.GetVerMsg = string.Empty;
            }

            item.IsCanUpgrade = e.CanUpgrade;
            item.CanUpgradeMsg = string.IsNullOrWhiteSpace(e.Code) ? string.Empty : $"[{e.Code}] {e.Code.GetErrorCodeDescription()}";
        }

        private async void OnUpgradeReadyStatusReceived(object? sender, UpgradeReadyStatusEventArgs e)
        {
            if (!e.IsReady)
            {
                Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Received hardware upgrade prepare status: Not OK, continue waiting");
                return;
            }

            UnRegisterUpgradePrepareEvent();
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Received hardware upgrade prepare status: OK");
            TotalProgress = 25;
            await SetCTBoxToUpgradingStatus();
        }

        private async void OnDeviceErrorCodeReceived(object? sender, ErrorInfoEventArgs e)
        {
            // 只有在等待硬件升级准备的结果时才订阅了此事件
            // 此期间由硬件保证仅有升级相关的错误码传递到上位机
            // 因此收到任何错误码都认为是升级准备失败

            UnRegisterUpgradePrepareEvent();
            await FailedBeforeUpgrade(e.ErrorCodes, "Hardware prepare failed", "Received device error code while waiting for hardware upgrade prepare status", Upgrade_Lang.Upgrade_Upgrade_DialogMsg_Part_PrepareResultError);
        }

        private void OnProgressChanged(object? sender, UpgradeProgressEventArgs e)
        {
            var item = FirmwareList.FirstOrDefault(i => i.ID == e.ID);

            if (item == null)
            {
                return;
            }

            if (e.Progress >= 0) //小于0时不刷新进度，代表升级失败了或者故意不刷新
            {
                item.Progress = e.Progress;
            }

            item.UpgradeStatus = e.UpgradeStatus;
            item.UpgradeMsg = string.IsNullOrWhiteSpace(e.Code) ? string.Empty : $"[{e.Code}] {e.Code.GetErrorCodeDescription()}";

            if (e.UpgradeStatus == UpgradeStatusType.Fail)
            {
                UpdateUpgradeCount();
            }
        }

        private void OnCTBoxConnectionChanged(object sender, ConnectionStatusArgs e)
        {
            Log.Info(ServiceCategory.UpgradeFirmware, $"[Upgrade] Received ctbox connected status: {e.Connected}");

            if (!e.Connected)
            {
                Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Wait power restart and hardware self check result");
                UnRegisterCtBoxConnectedEvent();
                UnRegisterSelfCheckEvent();
                RegisterSelfCheckEvent(); //订阅Proxy相关事件，从事件中获取自检的结果，并进行下一步处理
            }
        }

        private async void OnTotalSelfCheckStatusChanged(object? sender, TotalSelfCheckStatusEventArgs e)
        {
            Log.Info(ServiceCategory.UpgradeFirmware, $"[Upgrade] Received hardware self check status: {e.TotalSelfCheckStatus}");

            if (e.TotalSelfCheckStatus is SelfCheckStatus.InProgress or SelfCheckStatus.Unknown)
            {
                return;
            }

            UnRegisterSelfCheckEvent();
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Get self check result completed, wait hardware synchronization version");
            await Task.Delay(15 * 1000);
            Log.Info(ServiceCategory.UpgradeFirmware, "[Upgrade] Hardware synchronization version completed, wait compare version after power restart");
            StepMessage = Upgrade_Lang.Upgrade_StepInfo_GetVersionAfterSelfCheck;
            TotalProgress = 80;
            _isGetUpgradeItemsVersion = true; //在下一轮的TimerElapsed中获取版本号时进行判断
        }

        #endregion

        #region Init

        private void InitFirmwares(IEnumerable<FirmwareModel> items, FirmwareModel? parentItem, IList<UpgradeFileModel> canUpgradeFileList, ref int idValue)
        {
            foreach (var item in items)
            {
                item.ID = idValue++;
                item.Parent = parentItem;
                item.CanUpgradeFiles = canUpgradeFileList.FirstOrDefault(i => i.FirmwareType == item.FirmwareType)?.Files ?? new List<string>();

                if (item.FirmwareType != FirmwareType.Node)
                {
                    FirmwareList.Add(item);
                }

                if (item.Children is { Count: > 0 })
                {
                    InitFirmwares(item.Children, item, canUpgradeFileList, ref idValue);
                }
            }
        }

        public void AnalyzeUpgradeFolder(string folderPath)
        {
            Init();

            if (!Directory.Exists(folderPath))
                return;

            var sb = new StringBuilder();
            var separator = ", ";
            var zipFiles = Directory.GetFiles(folderPath).Where(i => Path.GetExtension(i) == ".zip");

            foreach (var file in zipFiles)
            {
                if (_config.CheckMD5)
                {
                    var md5File = Path.ChangeExtension(file, ".md5");

                    if (!File.Exists(md5File))
                    {
                        sb.Append($"{Path.GetFileNameWithoutExtension(file)}{separator}");
                        continue;
                    }

                    var readMd5 = File.ReadAllLines(md5File).FirstOrDefault(i => Regex.IsMatch(i, "^[a-fA-F0-9]{32}$"));

                    if (string.IsNullOrWhiteSpace(readMd5) || !MD5Utility.IsMatchFromFile(file, readMd5))
                    {
                        sb.Append($"{Path.GetFileNameWithoutExtension(file)}{separator}");
                        continue;
                    }
                }

                var array = Path.GetFileNameWithoutExtension(file).Split('_');
                var relativeFirmwares = FirmwareList.Where(i => string.Equals(i.UpgradeName, array[0], StringComparison.OrdinalIgnoreCase)).ToList();

                if (relativeFirmwares.Count <= 0)
                    continue;

                foreach (var firmware in relativeFirmwares)
                {
                    firmware.IsEnabled = true;
                    firmware.PackagePath = file;

                    if (array.Length > 1)
                    {
                        firmware.UpgradeVersion = array[1];
                    }
                }
            }

            foreach (var firmware in FirmwareTree)
            {
                SetIsEnabled(firmware);
            }

            foreach (var firmwareType in FirmwareTypes)
            {
                firmwareType.IsEnabled = FirmwareList.Any(i => i.FirmwareType == firmwareType.FirmwareType && i.IsEnabled);
            }

            TotalCount = (uint)FirmwareList.Count(i => i.IsEnabled);
            UpdateAllIsChecked(true);
            var str = sb.ToString().TrimEnd(separator.ToCharArray());

            if (!string.IsNullOrWhiteSpace(str))
            {
                Dialog.ShowErrorCode(Global.ErrorCode_MD5ValidateFail, str);
            }
        }

        private void Init()
        {
            _upgradeList.Clear();
            _isGetUpgradeItemsVersion = false;
            TotalCount = 0;
            SucceededCount = 0;
            FailedCount = 0;
            TotalProgress = 0;
            StepMessage = string.Empty;

            foreach (var fw in FirmwareTree)
            {
                InitFw(fw);
            }

            foreach (var fwType in FirmwareTypes)
            {
                fwType.IsChecked = false;
                fwType.IsEnabled = false;
            }

            void InitFw(FirmwareModel item)
            {
                item.PackagePath = string.Empty;
                item.Progress = 0;
                item.UpgradeVersion = string.Empty;
                item.IsChecked = false;
                item.IsEnabled = false;
                item.UpgradeStatus = UpgradeStatusType.None;

                if (item.Children is not { Count: > 0 })
                {
                    return;
                }

                foreach (var child in item.Children)
                {
                    InitFw(child);
                }
            }
        }

        #endregion

        #region Register

        private void RegisterEvent()
        {
            UpgradeProxy.Instance.VersionReceived += OnVersionReceived;
            UpgradeProxy.Instance.CanUpgradeReceived += OnCanUpgradeReceived;
            UpgradeProxy.Instance.ProgressChanged += OnProgressChanged;
            _timer.Elapsed += TimerElapsed;
        }

        private void RegisterUpgradePrepareEvent()
        {
            UpgradeProxy.Instance.UpgradeReadyStatusReceived += OnUpgradeReadyStatusReceived;
            UpgradeProxy.Instance.DeviceErrorCodeReceived += OnDeviceErrorCodeReceived;
        }

        private void RegisterCtBoxConnectedEvent()
        {
            ComponentStatusProxy.Instance.CTBoxConnectionChanged += OnCTBoxConnectionChanged;
        }

        private void RegisterSelfCheckEvent()
        {
            SelfCheckProxy.Instance.TotalSelfCheckStatusChanged += OnTotalSelfCheckStatusChanged;
        }

        private void UnRegisterEvent()
        {
            UpgradeProxy.Instance.VersionReceived -= OnVersionReceived;
            UpgradeProxy.Instance.CanUpgradeReceived -= OnCanUpgradeReceived;
            UpgradeProxy.Instance.ProgressChanged -= OnProgressChanged;
            _timer.Elapsed -= TimerElapsed;
        }

        private void UnRegisterUpgradePrepareEvent()
        {
            UpgradeProxy.Instance.UpgradeReadyStatusReceived -= OnUpgradeReadyStatusReceived;
            UpgradeProxy.Instance.DeviceErrorCodeReceived -= OnDeviceErrorCodeReceived;
        }

        private void UnRegisterCtBoxConnectedEvent()
        {
            ComponentStatusProxy.Instance.CTBoxConnectionChanged -= OnCTBoxConnectionChanged;
        }

        private void UnRegisterSelfCheckEvent()
        {
            SelfCheckProxy.Instance.TotalSelfCheckStatusChanged -= OnTotalSelfCheckStatusChanged;
        }

        private void UnRegisterAll()
        {
            UnRegisterEvent();
            UnRegisterUpgradePrepareEvent();
            UnRegisterCtBoxConnectedEvent();
            UnRegisterSelfCheckEvent();
        }

        #endregion

        #region IsEnabled/IsChecked

        private void SetIsEnabled(FirmwareModel item)
        {
            if (item.IsEnabled)
                return;

            if (item.Children is not { Count: > 0 })
                return;

            foreach (var child in item.Children)
            {
                SetIsEnabled(child);
            }

            item.IsEnabled = item.Children.Any(i => i.IsEnabled);
        }

        private void SetChildrenIsChecked(IList<FirmwareModel> items, bool isChecked)
        {
            foreach (var item in items)
            {
                if (!item.IsEnabled || item.IsChecked == isChecked)
                    continue;

                item.IsChecked = isChecked;

                if (item.Children is { Count: > 0 })
                {
                    SetChildrenIsChecked(item.Children, isChecked);
                }
            }
        }

        private void SetParentIsChecked(FirmwareModel parentItem)
        {
            var groups = parentItem.Children!.Where(i => i.IsEnabled).GroupBy(i => i.IsChecked).ToList();
            var isChecked = groups.Count == 1 ? groups[0].Key : null;

            if (!isChecked.HasValue && parentItem.FirmwareType != FirmwareType.Node)
            {
                isChecked = true;
            }

            if (parentItem.IsChecked == isChecked)
                return;

            parentItem.IsChecked = isChecked;

            if (parentItem.Parent != null)
            {
                SetParentIsChecked(parentItem.Parent);
            }
        }

        private void UpdateFirmwareTypeIsChecked()
        {
            foreach (var firmwareType in FirmwareTypes)
            {
                if (!firmwareType.IsEnabled)
                    continue;

                var groups = FirmwareList.Where(i => i.FirmwareType == firmwareType.FirmwareType && i.IsEnabled).GroupBy(i => i.IsChecked).ToList();
                firmwareType.IsChecked = groups.Count == 1 ? groups[0].Key : null;
            }
        }

        private void UpdateAllIsChecked(bool isChecked)
        {
            SetChildrenIsChecked(FirmwareTree, isChecked);

            foreach (var item in FirmwareTypes)
            {
                if (!item.IsEnabled)
                    continue;

                item.IsChecked = isChecked;
            }

            UpdateCheckedCountAndIsAllChecked();
        }

        private void UpdateCheckedCountAndIsAllChecked()
        {
            CheckedCount = (uint)FirmwareList.Count(i => i is { IsEnabled: true, IsChecked: true });
            IsAllChecked = FirmwareList.Where(i => i.IsEnabled).All(i => i.IsChecked == true);
        }

        #endregion

        #region Method

        private async Task ConfirmCanUpgrade(IEnumerable<FirmwareModel> items)
        {
            var firmwares = items.Select(i => i.ToProto()).ToList();
            await UpgradeProxy.Instance.CanUpgrade(firmwares);
        }

        private async Task GetFirmwaresVersion(IEnumerable<FirmwareModel> items)
        {
            var firmwares = items.Select(i => i.ToProto()).ToList();
            await UpgradeProxy.Instance.GetFirmwaresVersion(firmwares);
        }

        private void UpdateUpgradeCount()
        {
            var succeededCount = 0u;
            var failedCount = 0u;

            foreach (var item in FirmwareList)
            {
                if (item.IsChecked != true)
                {
                    continue;
                }

                switch (item.UpgradeStatus)
                {
                    case UpgradeStatusType.Success:
                    {
                        succeededCount += 1;
                        break;
                    }
                    case UpgradeStatusType.Fail:
                    {
                        failedCount += 1;
                        break;
                    }
                }
            }

            SucceededCount = succeededCount;
            FailedCount = failedCount;
        }

        private string GetFailedStr()
        {
            var failedList = FirmwareList.Where(i => i is { IsChecked: true, UpgradeStatus: UpgradeStatusType.Fail }).Select(i => i.DisplayName).ToArray();
            return failedList.Length > 0 ? $"{Environment.NewLine}{string.Format(Upgrade_Lang.Upgrade_Upgrade_FailedItems, string.Join(Environment.NewLine, failedList))}" : string.Empty;
        }

        #endregion
    }
}