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
using System.Collections.Generic;
using NV.CT.Language;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.ConfigManagement.ViewModel;

public class AcquisitionViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<AcquisitionViewModel> _logger;

    private ObservableCollection<BaseItemViewModel<string>> _exposureModes = new ObservableCollection<BaseItemViewModel<string>>();
    public ObservableCollection<BaseItemViewModel<string>> ExposureModes
    {
        get => _exposureModes;
        set => SetProperty(ref _exposureModes, value);
    }

    private double _minExposureTime = 5;
    public double MinExposureTime
    {
        get => _minExposureTime;
        set => SetProperty(ref _minExposureTime, value);
    }

    private double _maxExposureTime = 5;
    public double MaxExposureTime
    {
        get => _maxExposureTime;
        set => SetProperty(ref _maxExposureTime, value);
    }

    private double _minFrameTime = 10;
    public double MinFrameTime
    {
        get => _minFrameTime;
        set => SetProperty(ref _minFrameTime, value);
    }

    private double _maxFrameTime = 10;
    public double MaxFrameTime
    {
        get => _maxFrameTime;
        set => SetProperty(ref _maxFrameTime, value);
    }

    private int _exposureInterval = 1;
    public int ExposureInterval
    {
        get => _exposureInterval;
        set => SetProperty(ref _exposureInterval, value);
    }

    private ObservableCollection<BaseItemViewModel<int>> _framesPerCycles = new ObservableCollection<BaseItemViewModel<int>>();
    public ObservableCollection<BaseItemViewModel<int>> FramesPerCycles
    {
        get => _framesPerCycles;
        set => SetProperty(ref _framesPerCycles, value);
    }

    private double _minExposureIntervalTime = 1;
    public double MinExposureIntervalTime
    {
        get => _minExposureIntervalTime;
        set => SetProperty(ref _minExposureIntervalTime, value);
    }

    private double _maxExposureIntervalTime = 10;
    public double MaxExposureIntervalTime
    {
        get => _maxExposureIntervalTime;
        set => SetProperty(ref _maxExposureIntervalTime, value);
    }

    private double _postPreVoiceDelayTime = 2;
    public double PostPreVoiceDelayTime
    {
        get => _postPreVoiceDelayTime;
        set => SetProperty(ref _postPreVoiceDelayTime, value);
    }

    public AcquisitionViewModel(
        IDialogService dialogService,
        ILogger<AcquisitionViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        InitExposureModes();
        InitFramesPerCycles();
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void InitExposureModes()
    {
        foreach (var enumItem in Enum.GetValues(typeof(ExposureMode)))
        {
            if (enumItem is not null)
            {
                ExposureModes.Add(new BaseItemViewModel<string> { IsChecked = false, Display = enumItem.ToString(), Key = enumItem.ToString() });
            }
        }
    }

    private void InitFramesPerCycles()
    {
        FramesPerCycles.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "1", Key = 1 });
        FramesPerCycles.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "180", Key = 180 });
        FramesPerCycles.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "360", Key = 360 });
        FramesPerCycles.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "540", Key = 540 });
        FramesPerCycles.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "720", Key = 720 });
        FramesPerCycles.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "1080", Key = 1080 });
    }

    private void GetNodeInfo()
    {
        AcquisitionInfo node = SystemConfig.AcquisitionConfig.Acquisition;
        foreach (var dItem in ExposureModes)
        {
            dItem.IsChecked = false;
            foreach (var item in node.ExposureMode.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }
        ExposureInterval = node.ExposureInterval.Value;
        MaxExposureTime = UnitConvert.Microsecond2Millisecond((double)node.ExposureTime.Max);
        MinExposureTime = UnitConvert.Microsecond2Millisecond((double)node.ExposureTime.Min);
        foreach (var dItem in FramesPerCycles)
        {
            dItem.IsChecked = false;
            foreach (var item in node.FramesPerCycle.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }
        MinFrameTime = UnitConvert.Microsecond2Millisecond((double)node.FrameTime.Min);
        MaxFrameTime = UnitConvert.Microsecond2Millisecond((double)node.FrameTime.Max);

        MinExposureIntervalTime = UnitConvert.Microsecond2Millisecond((double)node.ExposureIntervalTime.Min);
        MaxExposureIntervalTime = UnitConvert.Microsecond2Millisecond((double)node.ExposureIntervalTime.Max);
        PostPreVoiceDelayTime = UnitConvert.Microsecond2Second((double)node.PostPreVoiceDelayTime.Value);
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        AcquisitionInfo node = SystemConfig.AcquisitionConfig.Acquisition;
        node.ExposureInterval.Value = ExposureInterval;
        node.ExposureTime.Max = UnitConvert.Millisecond2Microsecond(MaxExposureTime);
        node.ExposureTime.Min = UnitConvert.Millisecond2Microsecond(MinExposureTime);
        node.FrameTime.Min = UnitConvert.Millisecond2Microsecond(MinFrameTime);
        node.FrameTime.Max = UnitConvert.Millisecond2Microsecond(MaxFrameTime);
        node.ExposureMode.Ranges = new List<string>();
        foreach (var dItem in ExposureModes)
        {
            if (dItem.IsChecked)
            {
                node.ExposureMode.Ranges.Add(dItem.Key);
            }
        }
        node.FramesPerCycle.Ranges = new List<int>();
        foreach (var dItem in FramesPerCycles)
        {
            if (dItem.IsChecked)
            {
                node.FramesPerCycle.Ranges.Add(dItem.Key);
            }
        }
        node.ExposureIntervalTime.Min = UnitConvert.Millisecond2Microsecond(MinExposureIntervalTime);
        node.ExposureIntervalTime.Max = UnitConvert.Millisecond2Microsecond(MaxExposureIntervalTime);
        node.PostPreVoiceDelayTime.Value = UnitConvert.Second2Microsecond(PostPreVoiceDelayTime);
        SystemConfig.AcquisitionConfig.Acquisition = node;
        bool saveFlag = SystemConfig.SaveAcquisitionConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (MaxExposureTime < MinExposureTime)
        {
            sb.Append("ExposureTime max cannot be smaller than ExposureTime min!");
            flag = false;
        }
        if (MaxFrameTime < MinFrameTime)
        {
            sb.Append("FrameTime max cannot be smaller than FrameTime min!");
            flag = false;
        }
        if (MaxExposureIntervalTime < MinExposureIntervalTime)
        {
            sb.Append("ExposureIntervalTime max cannot be smaller than ExposureIntervalTime min!");
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