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

public class DetectorViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<DetectorViewModel> _logger;

    private int _xChannelCount = 10240;
    public int XChannelCount
    {
        get => _xChannelCount;
        set => SetProperty(ref _xChannelCount, value);
    }

    private int _zChannelCount = 288;
    public int ZChannelCount
    {
        get => _zChannelCount;
        set => SetProperty(ref _zChannelCount, value);
    }

    private int _xCutChannelCount = 3072;
    public int XCutChannelCount
    {
        get => _xCutChannelCount;
        set => SetProperty(ref _xCutChannelCount, value);
    }

    private int _zCutChannelCount = 288;
    public int ZCutChannelCount
    {
        get => _zCutChannelCount;
        set => SetProperty(ref _zCutChannelCount, value);
    }

    private int _xSingleModuleChannelCount = 640;
    public int XSingleModuleChannelCount
    {
        get => _xSingleModuleChannelCount;
        set => SetProperty(ref _xSingleModuleChannelCount, value);
    }

    private int _zSingleModuleChannelCount = 288;
    public int ZSingleModuleChannelCount
    {
        get => _zSingleModuleChannelCount;
        set => SetProperty(ref _zSingleModuleChannelCount, value);
    }

    private int _xModuleCount = 16;
    public int XModuleCount
    {
        get => _xModuleCount;
        set => SetProperty(ref _xModuleCount, value);
    }

    private int _zModuleCount = 1;
    public int ZModuleCount
    {
        get => _zModuleCount;
        set => SetProperty(ref _zModuleCount, value);
    }

    private double _detectorWidth = 47.2;
    public double DetectorWidth
    {
        get => _detectorWidth;
        set => SetProperty(ref _detectorWidth, value);
    }

    private double _detectorPhysicalWidth = 75.5;
    public double DetectorPhysicalWidth
    {
        get => _detectorPhysicalWidth;
        set => SetProperty(ref _detectorPhysicalWidth, value);
    }

    private double _xOffset = 0;
    public double XOffset
    {
        get => _xOffset;
        set => SetProperty(ref _xOffset, value);
    }

    private double _zOffset = 0;
    public double ZOffset
    {
        get => _zOffset;
        set => SetProperty(ref _zOffset, value);
    }

    private ObservableCollection<BaseItemViewModel<int>> _scanFOVs = new ObservableCollection<BaseItemViewModel<int>>();
    public ObservableCollection<BaseItemViewModel<int>> ScanFOVs
    {
        get => _scanFOVs;
        set => SetProperty(ref _scanFOVs, value);
    }

    private double _sID = 1;
    public double SID
    {
        get => _sID;
        set => SetProperty(ref _sID, value);
    }

    private double _sOD = 1;
    public double SOD
    {
        get => _sOD;
        set => SetProperty(ref _sOD, value);
    }

    private double _resolution = 0.265;
    public double Resolution
    {
        get => _resolution;
        set => SetProperty(ref _resolution, value);
    }

    private double _slantedAngle = 0.5;
    public double SlantedAngle
    {
        get => _slantedAngle;
        set => SetProperty(ref _slantedAngle, value);
    }

    private int _xSingleModuleChipCount = 4;
    public int XSingleModuleChipCount
    {
        get => _xSingleModuleChipCount;
        set => SetProperty(ref _xSingleModuleChipCount, value);
    }

    private int _zSingleModuleChipCount = 2;
    public int ZSingleModuleChipCount
    {
        get => _zSingleModuleChipCount;
        set => SetProperty(ref _zSingleModuleChipCount, value);
    }

    public DetectorViewModel(
        IDialogService dialogService,
        ILogger<DetectorViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        InitScanFOVs();
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void InitScanFOVs()
    {
        ScanFOVs.Add(new BaseItemViewModel<int> { IsChecked = false, Display = " 506.88 ", Key = 506880 });
        ScanFOVs.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "337.92", Key = 337920 });
        ScanFOVs.Add(new BaseItemViewModel<int> { IsChecked = false, Display = "253.44", Key = 253440 });
    }

    private void GetNodeInfo()
    {
        DetectorInfo node = SystemConfig.DetectorConfig.Detector;
        XChannelCount = node.XChannelCount.Value;
        ZChannelCount = node.ZChannelCount.Value;
        XCutChannelCount = node.XCutChannelCount.Value;
        ZCutChannelCount = node.ZCutChannelCount.Value;
        XSingleModuleChannelCount = node.XSingleModuleChannelCount.Value;
        ZSingleModuleChannelCount = node.ZSingleModuleChannelCount.Value;
        XModuleCount = node.XModuleCount.Value;
        ZModuleCount = node.ZModuleCount.Value;
        DetectorWidth = UnitConvert.Micron2Millimeter((double)node.Width.Value);
        DetectorPhysicalWidth = UnitConvert.Micron2Millimeter((double)node.PhysicalWidth.Value);
        XOffset = node.XOffset.Value;
        ZOffset = node.ZOffset.Value;
        foreach (var dItem in ScanFOVs)
        {
            dItem.IsChecked = false;
            foreach (var item in node.ScanFOV.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }
        SID = node.SID.Value / 1000;
        SOD = node.SOD.Value / 1000;
        Resolution = UnitConvert.Micron2Millimeter((double)node.Resolution.Value);
        SlantedAngle = node.SlantedAngle.Value;
        XSingleModuleChipCount = node.XSingleModuleChipCount.Value;
        ZSingleModuleChipCount = node.ZSingleModuleChipCount.Value;
    }

    public void Saved()
    {
        DetectorInfo node = SystemConfig.DetectorConfig.Detector;
        node.XChannelCount.Value = XChannelCount;
        node.ZChannelCount.Value = ZChannelCount;
        node.XCutChannelCount.Value = XCutChannelCount;
        node.ZCutChannelCount.Value = ZCutChannelCount;
        node.XSingleModuleChannelCount.Value = XSingleModuleChannelCount;
        node.ZSingleModuleChannelCount.Value = ZSingleModuleChannelCount;
        node.XModuleCount.Value = XModuleCount;
        node.ZModuleCount.Value = ZModuleCount;
        node.Width.Value = UnitConvert.Millimeter2Micron(DetectorWidth);
        node.PhysicalWidth.Value = UnitConvert.Millimeter2Micron(DetectorPhysicalWidth);
        node.XOffset.Value = XOffset;
        node.ZOffset.Value = ZOffset;
        node.ScanFOV.Ranges = new List<int>();
        foreach (var dItem in ScanFOVs)
        {
            if (dItem.IsChecked)
            {
                node.ScanFOV.Ranges.Add(dItem.Key);
            }
        }
        node.SID.Value = SID * 1000;
        node.SOD.Value = SOD * 1000;
        node.Resolution.Value = UnitConvert.Millimeter2Micron(Resolution);
        node.SlantedAngle.Value = SlantedAngle;
        node.XSingleModuleChipCount.Value = XSingleModuleChipCount;
        node.ZSingleModuleChipCount.Value = ZSingleModuleChipCount;
        SystemConfig.DetectorConfig.Detector = node;
        bool saveFlag = SystemConfig.SaveDetectorConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
}