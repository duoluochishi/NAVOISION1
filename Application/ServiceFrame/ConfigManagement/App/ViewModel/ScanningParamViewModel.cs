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
using NV.MPS.Environment;

namespace NV.CT.ConfigManagement.ViewModel;

public class ScanningParamViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<ScanningParamViewModel> _logger;

    private int _minAvailableVoltage = 70;
    public int MinAvailableVoltage
    {
        get => _minAvailableVoltage;
        set => SetProperty(ref _minAvailableVoltage, value);
    }

    private int _maxavailableVoltage = 140;
    public int MaxAvailableVoltage
    {
        get => _maxavailableVoltage;
        set => SetProperty(ref _maxavailableVoltage, value);
    }

    private double _maxTubeCurrent = 200;
    public double MaxTubeCurrent
    {
        get => _maxTubeCurrent;
        set => SetProperty(ref _maxTubeCurrent, value);
    }

    private double _minTubeCurrent = 20;
    public double MinTubeCurrent
    {
        get => _minTubeCurrent;
        set => SetProperty(ref _minTubeCurrent, value);
    }

    private double _topoMinLength = 50;
    public double TopoMinLength
    {
        get => _topoMinLength;
        set => SetProperty(ref _topoMinLength, value);
    }

    private double _topoMaxLength = 1000;
    public double TopoMaxLength
    {
        get => _topoMaxLength;
        set => SetProperty(ref _topoMaxLength, value);
    }

    private double _axialMinLength = 50;
    public double AxialMinLength
    {
        get => _axialMinLength;
        set => SetProperty(ref _axialMinLength, value);
    }

    private double _axialMaxLength = 1000;
    public double AxialMaxLength
    {
        get => _axialMaxLength;
        set => SetProperty(ref _axialMaxLength, value);
    }

    private double _spiralMinLength = 50;
    public double SpiralMinLength
    {
        get => _spiralMinLength;
        set => SetProperty(ref _spiralMinLength, value);
    }

    private double _spiralMaxLength = 1000;
    public double SpiralMaxLength
    {
        get => _spiralMaxLength;
        set => SetProperty(ref _spiralMaxLength, value);
    }

    private double _axialScanFeed = 42;
    public double AxialScanFeed
    {
        get => _axialScanFeed;
        set => SetProperty(ref _axialScanFeed, value);
    }

    private double _axialScanDetectorWidth = 47.2;
    public double AxialScanDetectorWidth
    {
        get => _axialScanDetectorWidth;
        set => SetProperty(ref _axialScanDetectorWidth, value);
    }

    private double _pitchMin = 0.33;
    public double PitchMin
    {
        get => _pitchMin;
        set => SetProperty(ref _pitchMin, value);
    }

    private double _pitchMax = 2.0;
    public double PitchMax
    {
        get => _pitchMax;
        set => SetProperty(ref _pitchMax, value);
    }

    private double _minCombinedScanIntervalTime = 200;
    public double MinCombinedScanIntervalTime
    {
        get => _minCombinedScanIntervalTime;
        set => SetProperty(ref _minCombinedScanIntervalTime, value);
    }

    private double _maxCombinedScanIntervalTime = 200;
    public double MaxCombinedScanIntervalTime
    {
        get => _maxCombinedScanIntervalTime;
        set => SetProperty(ref _maxCombinedScanIntervalTime, value);
    }

    private double _minExposureDelayTime = 3.0;
	public double MinExposureDelayTime
	{
		get => _minExposureDelayTime;
		set => SetProperty(ref _minExposureDelayTime, value);
	}

	private ObservableCollection<BaseItemViewModel<double>> _scanFOVs = new ObservableCollection<BaseItemViewModel<double>>();
    public ObservableCollection<BaseItemViewModel<double>> ScanFOVs
    {
        get => _scanFOVs;
        set => SetProperty(ref _scanFOVs, value);
    }

    public ScanningParamViewModel(
        IDialogService dialogService,
        ILogger<ScanningParamViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        InitScanFOVs();
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void InitScanFOVs()
    {
        ScanFOVs.Add(new BaseItemViewModel<double> { IsChecked = false, Display = "253.44", Key = 253440 });
        ScanFOVs.Add(new BaseItemViewModel<double> { IsChecked = false, Display = "337.92", Key = 337920 });
        ScanFOVs.Add(new BaseItemViewModel<double> { IsChecked = false, Display = "506.88", Key = 506880 });
    }

    private void GetNodeInfo()
    {
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;
        MinAvailableVoltage = node.AvailableVoltages.Min;
        MaxAvailableVoltage = node.AvailableVoltages.Max;
        MaxTubeCurrent = UnitConvert.ReduceThousand((double)node.TubeCurrent.Max);
        MinTubeCurrent = UnitConvert.ReduceThousand((double)node.TubeCurrent.Min);
        TopoMinLength = UnitConvert.Micron2Millimeter((double)node.TopoLength.Min);
        TopoMaxLength = UnitConvert.Micron2Millimeter((double)node.TopoLength.Max);
        AxialMinLength = UnitConvert.Micron2Millimeter((double)node.AxialLength.Min);
        AxialMaxLength = UnitConvert.Micron2Millimeter((double)node.AxialLength.Max);
        SpiralMinLength = UnitConvert.Micron2Millimeter((double)node.SpiralLength.Min);
        SpiralMaxLength = UnitConvert.Micron2Millimeter((double)node.SpiralLength.Max);
        AxialScanFeed = UnitConvert.Micron2Millimeter((double)node.AxialScanFeed.Value);
        AxialScanDetectorWidth = UnitConvert.Micron2Millimeter((double)node.AxialScanDetectorWidth.Value);
        PitchMin = UnitConvert.ReduceHundred((double)node.Pitch.Min);
        PitchMax = UnitConvert.ReduceHundred((double)node.Pitch.Max);
        MinCombinedScanIntervalTime = UnitConvert.Microsecond2Second((double)node.CombinedScanIntervalTime.Min);
        MaxCombinedScanIntervalTime = UnitConvert.Microsecond2Second((double)node.CombinedScanIntervalTime.Max);
        MinExposureDelayTime = UnitConvert.Microsecond2Second((double)node.MinExposureDelayTime.Value);

		foreach (var dItem in ScanFOVs)
        {
            dItem.IsChecked = false;
            foreach (var item in node.ScanFOV.Ranges)
            {
                if (item == dItem.Key)
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
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;
        node.AvailableVoltages.Min = MinAvailableVoltage;
        node.AvailableVoltages.Max = MaxAvailableVoltage;
        node.TubeCurrent.Max = (int)UnitConvert.ExpandThousand(MaxTubeCurrent);
        node.TubeCurrent.Min = (int)UnitConvert.ExpandThousand(MinTubeCurrent);
        node.TopoLength.Min = UnitConvert.Millimeter2Micron(TopoMinLength);
        node.TopoLength.Max = UnitConvert.Millimeter2Micron(TopoMaxLength);
        node.AxialLength.Min = UnitConvert.Millimeter2Micron(AxialMinLength);
        node.AxialLength.Max = UnitConvert.Millimeter2Micron(AxialMaxLength);
        node.SpiralLength.Min = UnitConvert.Millimeter2Micron(SpiralMinLength);
        node.SpiralLength.Max = UnitConvert.Millimeter2Micron(SpiralMaxLength);
        node.AxialScanFeed.Value = UnitConvert.Millimeter2Micron(AxialScanFeed);
        node.AxialScanDetectorWidth.Value = UnitConvert.Millimeter2Micron(AxialScanDetectorWidth);
        node.Pitch.Min = (int)UnitConvert.ExpandHundred(PitchMin);
        node.Pitch.Max = (int)UnitConvert.ExpandHundred(PitchMax);
        node.CombinedScanIntervalTime.Min = UnitConvert.Second2Microsecond((int)MinCombinedScanIntervalTime);
        node.CombinedScanIntervalTime.Max = UnitConvert.Second2Microsecond((int)MaxCombinedScanIntervalTime);
        node.MinExposureDelayTime.Value = UnitConvert.Second2Microsecond((int)MinExposureDelayTime);

		node.ScanFOV.Ranges = new List<int>();
        foreach (var dItem in ScanFOVs)
        {
            if (dItem.IsChecked)
            {
                node.ScanFOV.Ranges.Add((int)dItem.Key);
            }
        }
        SystemConfig.ScanningParamConfig.ScanningParam = node;
        bool saveFlag = SystemConfig.SaveScanningParamConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (MaxAvailableVoltage < MinAvailableVoltage)
        {
            sb.Append("Available voltage max cannot be smaller than available voltage min!");
            flag = false;
        }
        if (MaxTubeCurrent < MinTubeCurrent)
        {
            sb.Append("Tube current max cannot be smaller than tube current min!");
            flag = false;
        }
        if (TopoMaxLength < TopoMinLength)
        {
            sb.Append("Topo length max cannot be smaller than topo length min!");
            flag = false;
        }
        if (AxialMaxLength < AxialMinLength)
        {
            sb.Append("Axial Length max cannot be smaller than axial length min!");
            flag = false;
        }
        if (SpiralMaxLength < SpiralMinLength)
        {
            sb.Append("Spiral length max cannot be smaller than spiral length min!");
            flag = false;
        }
        if (PitchMax < PitchMin)
        {
            sb.Append("Pitch max cannot be smaller than pitch min!");
            flag = false;
        }
        if (MaxCombinedScanIntervalTime < MinCombinedScanIntervalTime)
        {
            sb.Append("Combined Scan Interval Time max cannot be smaller than Combined Scan Interval Time min!");
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