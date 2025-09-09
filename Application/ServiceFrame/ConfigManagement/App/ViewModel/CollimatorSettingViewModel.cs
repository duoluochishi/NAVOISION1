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

public class CollimatorSettingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<CollimatorSettingViewModel> _logger;

    private int _minCollimator;
    public int MinCollimator
    {
        get => _minCollimator;
        set => SetProperty(ref _minCollimator, value);
    }

    private int _maxCollimator;
    public int MaxCollimator
    {
        get => _maxCollimator;
        set => SetProperty(ref _maxCollimator, value);
    }

    private int _moduleCount = 24;
    public int ModuleCount
    {
        get => _moduleCount;
        set => SetProperty(ref _moduleCount, value);
    }

    private int _maxFrontBladeMoveStept = 4000;
    public int MaxFrontBladeMoveStep
    {
        get => _maxFrontBladeMoveStept;
        set => SetProperty(ref _maxFrontBladeMoveStept, value);
    }

    private int _maxRearBladeMoveStep = 4533;
    public int MaxRearBladeMoveStep
    {
        get => _maxRearBladeMoveStep;
        set => SetProperty(ref _maxRearBladeMoveStep, value);
    }

    private int _maxBowtieMoveStep = 3733;
    public int MaxBowtieMoveStep
    {
        get => _maxBowtieMoveStep;
        set => SetProperty(ref _maxBowtieMoveStep, value);
    }

    private ObservableCollection<BaseItemViewModel<string>> _collimatorModes = new ObservableCollection<BaseItemViewModel<string>>();
    public ObservableCollection<BaseItemViewModel<string>> CollimatorModes
    {
        get => _collimatorModes;
        set => SetProperty(ref _collimatorModes, value);
    }

    public CollimatorSettingViewModel(
        IDialogService dialogService,
        ILogger<CollimatorSettingViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        InitCollimatorModes();
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void InitCollimatorModes()
    {
        CollimatorModes.Add(new BaseItemViewModel<string> { IsChecked = false, Display = "CenterOpening", Key = "CenterOpening" });
        //CollimatorModes.Add(new BaseItemViewModel<string> { IsChecked = false, Display = "180", Key = 180 });
        //CollimatorModes.Add(new BaseItemViewModel<string> { IsChecked = false, Display = "360", Key = 360 });
        //CollimatorModes.Add(new BaseItemViewModel<string> { IsChecked = false, Display = "720", Key = 720 });     
    }

    private void GetNodeInfo()
    {
        CollimatorSettingInfo node = SystemConfig.CollimatorConfig.CollimatorSetting;
        MinCollimator = node.Collimator.Min;
        MaxCollimator = node.Collimator.Max;
        ModuleCount = node.ModuleCount.Value;
        foreach (var dItem in CollimatorModes)
        {
            dItem.IsChecked = false;
            foreach (var item in node.CollimatorMode.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }
        MaxFrontBladeMoveStep = node.MaxFrontBladeMoveStep.Value;
        MaxRearBladeMoveStep = node.MaxRearBladeMoveStep.Value;
        MaxBowtieMoveStep = node.MaxBowtieMoveStep.Value;
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        CollimatorSettingInfo node = SystemConfig.CollimatorConfig.CollimatorSetting;
        node.Collimator.Min = MinCollimator;
        node.Collimator.Max = MaxCollimator;
        node.ModuleCount.Value = ModuleCount;
        node.CollimatorMode.Ranges = new List<string>();
        foreach (var dItem in CollimatorModes)
        {
            if (dItem.IsChecked)
            {
                node.CollimatorMode.Ranges.Add(dItem.Key);
            }
        }
        node.MaxFrontBladeMoveStep.Value = MaxFrontBladeMoveStep;
        node.MaxRearBladeMoveStep.Value = MaxRearBladeMoveStep;
        node.MaxBowtieMoveStep.Value = MaxBowtieMoveStep;
        SystemConfig.CollimatorConfig.CollimatorSetting = node;

        bool saveFlag = SystemConfig.SaveCollimatorConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be < 0!";

        if (MinCollimator < 0)
        {
            sb.Append(string.Format(message, "Collimator min"));
            flag = false;
        }
        if (MaxCollimator < 0)
        {
            sb.Append(string.Format(message, "Collimator max"));
            flag = false;
        }
        if (MaxCollimator < MinCollimator)
        {
            sb.Append("Collimator max cannot be smaller than Collimator min!");
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