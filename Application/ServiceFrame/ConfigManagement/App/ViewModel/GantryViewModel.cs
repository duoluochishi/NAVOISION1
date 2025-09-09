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

public class GantryViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<GantryViewModel> _logger;

    private double _minAngle = 60;
    public double MinAngle
    {
        get => _minAngle;
        set => SetProperty(ref _minAngle, value);
    }

    private double _maxAngle = 540;
    public double MaxAngle
    {
        get => _maxAngle;
        set => SetProperty(ref _maxAngle, value);
    }

    private double _tube1Offset = 0;
    public double Tube1Offset
    {
        get => _tube1Offset;
        set => SetProperty(ref _tube1Offset, value);
    }

    private double _maxRotationSpeed = 0;
    public double MaxRotationSpeed
    {
        get => _maxRotationSpeed;
        set => SetProperty(ref _maxRotationSpeed, value);
    }

    private double _rotationAcceleration = 10;
    public double RotationAcceleration
    {
        get => _rotationAcceleration;
        set => SetProperty(ref _rotationAcceleration, value);
    }

    private double _rotationSpeedAccuracy = 1;
    public double RotationSpeedAccuracy
    {
        get => _rotationSpeedAccuracy;
        set => SetProperty(ref _rotationSpeedAccuracy, value);
    }

    private double _rotationPositioningAccuracy = 0.5;
    public double RotationPositioningAccuracy
    {
        get => _rotationPositioningAccuracy;
        set => SetProperty(ref _rotationPositioningAccuracy, value);
    }

    private double _resetSpeed = 0;
    public double ResetSpeed
    {
        get => _resetSpeed;
        set => SetProperty(ref _resetSpeed, value);
    }

    private double _manualRotationSpeed = 0;
    public double ManualRotationSpeed
    {
        get => _manualRotationSpeed;
        set => SetProperty(ref _manualRotationSpeed, value);
    }

    public GantryViewModel(
        IDialogService dialogService,
        ILogger<GantryViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        GantryInfo node = SystemConfig.GantryConfig.Gantry;
        MinAngle = UnitConvert.ReduceHundred((double)node.Angle.Min);
        MaxAngle = UnitConvert.ReduceHundred((double)node.Angle.Max);
        Tube1Offset = UnitConvert.ReduceHundred((double)node.FirstTubeOffset.Value);
        MaxRotationSpeed = UnitConvert.ReduceHundred((double)node.MaxRotationSpeed.Value);
        RotationAcceleration = UnitConvert.ReduceHundred((double)node.RotationAcceleration.Value);
        RotationSpeedAccuracy = UnitConvert.ReduceHundred((double)node.RotationSpeedAccuracy.Value);
        ResetSpeed = UnitConvert.ReduceHundred((double)node.ResetSpeed.Value);
        ManualRotationSpeed = UnitConvert.ReduceHundred((double)node.ManualRotationSpeed.Value);
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        GantryInfo node = SystemConfig.GantryConfig.Gantry;
        node.Angle.Min = (int)UnitConvert.ExpandHundred(MinAngle);
        node.Angle.Max = (int)UnitConvert.ExpandHundred(MaxAngle);
        node.FirstTubeOffset.Value = (int)UnitConvert.ExpandHundred(Tube1Offset);
        node.MaxRotationSpeed.Value = (int)UnitConvert.ExpandHundred(MaxRotationSpeed);
        node.RotationAcceleration.Value = (int)UnitConvert.ExpandHundred(RotationAcceleration);
        node.RotationSpeedAccuracy.Value = (int)UnitConvert.ExpandHundred(RotationSpeedAccuracy);
        node.ResetSpeed.Value = (int)UnitConvert.ExpandHundred(ResetSpeed);
        node.ManualRotationSpeed.Value = (int)UnitConvert.ExpandHundred(ManualRotationSpeed);
        SystemConfig.GantryConfig.Gantry = node;
        bool saveFlag = SystemConfig.SaveGantryConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (MaxAngle < MinAngle)
        {
            sb.Append("Angle max cannot be smaller than Angle min!");
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