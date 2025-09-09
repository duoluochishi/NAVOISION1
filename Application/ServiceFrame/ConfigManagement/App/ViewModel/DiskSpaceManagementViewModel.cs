//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.Language;
using System.Collections.Generic;
namespace NV.CT.ConfigManagement.ViewModel;

public class DiskSpaceManagementViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
   // private readonly ILogger<DiskSpaceManagementViewModel> _logger;

    private int _rawDataWarningThreshold = 0;
    public int RawDataWarningThreshold
    {
        get => _rawDataWarningThreshold;
        set => SetProperty(ref _rawDataWarningThreshold, value);
    }

    private int _imageDataWarningThreshold = 0;
    public int ImageDataWarningThreshold
    {
        get => _imageDataWarningThreshold;
        set => SetProperty(ref _imageDataWarningThreshold, value);
    }

    private bool _isAutoDeletedEnabled = false;
    public bool IsAutoDeletedEnabled
    {
        get => _isAutoDeletedEnabled;
        set => SetProperty(ref _isAutoDeletedEnabled, value);
    }

    private ObservableCollection<BaseItemViewModel<int>> _autoDeletePeriods = new ObservableCollection<BaseItemViewModel<int>>();
    public ObservableCollection<BaseItemViewModel<int>> AutoDeletePeriods
    {
        get => _autoDeletePeriods;
        set => SetProperty(ref _autoDeletePeriods, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public DiskSpaceManagementViewModel(
        IDialogService dialogService,
        ILogger<DiskSpaceManagementViewModel> logger)
    {
        _dialogService = dialogService;
       // _logger = logger;
        InitAutoDeletePeriods();
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void InitAutoDeletePeriods()
    {
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "1", Key = 1 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "2", Key = 2 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "3", Key = 3 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "4", Key = 4 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "5", Key = 5 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "6", Key = 6 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "7", Key = 7 });
        AutoDeletePeriods.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "8", Key = 8 });
    }

    private void GetNodeInfo()
    {
        DiskspaceSettingInfo node = UserConfig.DiskspaceSettingConfig.DiskspaceSetting;
        if (node.RawDataWarningThreshold is not null)
        {
            RawDataWarningThreshold = node.RawDataWarningThreshold.Value;
        }
        if (node.RawDataWarningThreshold is not null)
        {
            ImageDataWarningThreshold = node.ImageDataWarningThreshold.Value;
        }
        if (node.IsAutoDeleted is not null)
        {
            IsAutoDeletedEnabled = node.IsAutoDeleted.Value;
        }

        foreach (var dItem in AutoDeletePeriods)
        {
            dItem.IsChecked = false;
            foreach (var item in node.AutoDeletePeriod.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        DiskspaceSettingInfo node = UserConfig.DiskspaceSettingConfig.DiskspaceSetting;
        node.RawDataWarningThreshold.Value = RawDataWarningThreshold;
        node.ImageDataWarningThreshold.Value =ImageDataWarningThreshold;

        node.IsAutoDeleted.Value = IsAutoDeletedEnabled;
        node.AutoDeletePeriod.Ranges = new List<int>();
        foreach (var dItem in AutoDeletePeriods)
        {
            if (dItem.IsChecked)
            {
                node.AutoDeletePeriod.Ranges.Add(dItem.Key);
            }
        }
        UserConfig.DiskspaceSettingConfig.DiskspaceSetting = node;
        bool saveFlag = UserConfig.SaveDiskspaceSetting();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
           arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(ImageDataWarningThreshold.ToString()))
        {
            sb.Append(string.Format(message, "ServiceAE"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }
}