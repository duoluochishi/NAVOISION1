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
using NV.MPS.Environment;

namespace NV.CT.ConfigManagement.ViewModel;

public class TableViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<TableViewModel> _logger;

    private double _xMax = 200;
    public double XMax
    {
        get => _xMax;
        set => SetProperty(ref _xMax, value);
    }

    private double _xMin = 0;
    public double XMin
    {
        get => _xMin;
        set => SetProperty(ref _xMin, value);
    }

    private double _yMax = 980;
    public double YMax
    {
        get => _yMax;
        set => SetProperty(ref _yMax, value);
    }

    private double _yMin = 480;
    public double YMin
    {
        get => _yMin;
        set => SetProperty(ref _yMin, value);
    }

    private double _zMax = 10;
    public double ZMax
    {
        get => _zMax;
        set => SetProperty(ref _zMax, value);
    }

    private double _zMin = -1960;
    public double ZMin
    {
        get => _zMin;
        set => SetProperty(ref _zMin, value);
    }

    private double _zSpeedMin = 250;
    public double ZSpeedMin
    {
        get => _zSpeedMin;
        set => SetProperty(ref _zSpeedMin, value);
    }

    private double _zSpeedMax = 10;
    public double ZSpeedMax
    {
        get => _zSpeedMax;
        set => SetProperty(ref _zSpeedMax, value);
    }

    private double _zAccMax = 200;
    public double ZAccMax
    {
        get => _zAccMax;
        set => SetProperty(ref _zAccMax, value);
    }

    private double _minLoad = 230;
    public double MinLoad
    {
        get => _minLoad;
        set => SetProperty(ref _minLoad, value);
    }

    private double _maxLoad = 230;
    public double MaxLoad
    {
        get => _maxLoad;
        set => SetProperty(ref _maxLoad, value);
    }

    private double _yFreeMoveZThreshold = 280;
    public double YFreeMoveZThreshold
    {
        get => _yFreeMoveZThreshold;
        set => SetProperty(ref _yFreeMoveZThreshold, value);
    }

    private double _zFreeMoveYThreshold = 750;
    public double ZFreeMoveYThreshold
    {
        get => _zFreeMoveYThreshold;
        set => SetProperty(ref _zFreeMoveYThreshold, value);
    }

    private double _xAccuracy = 1;
    public double XAccuracy
    {
        get => _xAccuracy;
        set => SetProperty(ref _xAccuracy, value);
    }

    private double _yAccuracy = 1;
    public double YAccuracy
    {
        get => _yAccuracy;
        set => SetProperty(ref _yAccuracy, value);
    }

    private double _zAccuracy = 1;
    public double ZAccuracy
    {
        get => _zAccuracy;
        set => SetProperty(ref _zAccuracy, value);
    }

    private double _yResetAccuracy = 0.5;
    public double YResetAccuracy
    {
        get => _yResetAccuracy;
        set => SetProperty(ref _yResetAccuracy, value);
    }

    public TableViewModel(
        IDialogService dialogService,
        ILogger<TableViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        TableInfo node = SystemConfig.TableConfig.Table;
        XMax = UnitConvert.Micron2Millimeter((double)node.MaxX.Value);
        XMin = UnitConvert.Micron2Millimeter((double)node.MinX.Value);
        YMax = UnitConvert.Micron2Millimeter((double)node.MaxY.Value);
        YMin = UnitConvert.Micron2Millimeter((double)node.MinY.Value);
        ZMax = UnitConvert.Micron2Millimeter((double)node.MaxZ.Value);
        ZMin = UnitConvert.Micron2Millimeter((double)node.MinZ.Value);
        ZSpeedMin = UnitConvert.Micron2Millimeter((double)node.MinZSpeed.Value);
        ZSpeedMax = UnitConvert.Micron2Millimeter((double)node.MaxZSpeed.Value);
        ZAccMax = UnitConvert.Micron2Millimeter((double)node.MaxZAcc.Value);
        MinLoad = node.MaxLoad.Min;
        MaxLoad = node.MaxLoad.Max;
        YFreeMoveZThreshold = UnitConvert.Micron2Millimeter((double)node.YFreeMoveZThreshold.Value);
        ZFreeMoveYThreshold = UnitConvert.Micron2Millimeter((double)node.ZFreeMoveYThreshold.Value);
        XAccuracy = node.XPositionAccuracy.Value;
        YAccuracy = node.YPositionAccuracy.Value;
        ZAccuracy = node.ZPositionAccuracy.Value;
        YResetAccuracy = node.YResetAccuracy.Value;
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        TableInfo node = SystemConfig.TableConfig.Table;
        node.MaxX.Value = UnitConvert.Millimeter2Micron(XMax);
        node.MinX.Value = UnitConvert.Millimeter2Micron(XMin);
        node.MaxY.Value = UnitConvert.Millimeter2Micron(YMax);
        node.MinY.Value = UnitConvert.Millimeter2Micron(YMin);
        node.MaxZ.Value = UnitConvert.Millimeter2Micron(ZMax);
        node.MinZ.Value = UnitConvert.Millimeter2Micron(ZMin);
        node.MinZSpeed.Value = UnitConvert.Millimeter2Micron(ZSpeedMin);
        node.MaxZSpeed.Value = UnitConvert.Millimeter2Micron(ZSpeedMax);
        node.MaxZAcc.Value = UnitConvert.Millimeter2Micron(ZAccMax);
        node.MaxLoad.Max = MaxLoad;
        node.MaxLoad.Min = MinLoad;
        node.YFreeMoveZThreshold.Value = UnitConvert.Millimeter2Micron(YFreeMoveZThreshold);
        node.ZFreeMoveYThreshold.Value = UnitConvert.Millimeter2Micron(ZFreeMoveYThreshold);
        node.XPositionAccuracy.Value = XAccuracy;
        node.YPositionAccuracy.Value = YAccuracy;
        node.ZPositionAccuracy.Value = ZAccuracy;
        node.YResetAccuracy.Value = YResetAccuracy;
        SystemConfig.TableConfig.Table = node;
        bool saveFlag = SystemConfig.SaveTableConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
         arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (XMax < XMin)
        {
            sb.Append("X max cannot be smaller than X min!");
            flag = false;
        }
        if (YMax < YMin)
        {
            sb.Append("Y max cannot be smaller than Y min!");
            flag = false;
        }
        if (ZMax < ZMin)
        {
            sb.Append("Z max cannot be smaller than Z min!");
            flag = false;
        }
        if (ZSpeedMax < ZSpeedMin)
        {
            sb.Append("Z speed max cannot be smaller than Z speed min!");
            flag = false;
        }
        if (MaxLoad < MinLoad)
        {
            sb.Append("Load max cannot be smaller than Load min!");
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