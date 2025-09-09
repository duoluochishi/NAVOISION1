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

namespace NV.CT.ConfigManagement.ViewModel;

public class SourceComponentViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<SourceComponentViewModel> _logger;

    private uint _sourceCount = 24;
    public uint SourceCount
    {
        get => _sourceCount;
        set => SetProperty(ref _sourceCount, value);
    }

    private uint _tubeInterfaceCount = 6;
    public uint TubeInterfaceCount
    {
        get => _tubeInterfaceCount;
        set => SetProperty(ref _tubeInterfaceCount, value);
    }

    private double _maxHeatCapacity = 260;
    public double MaxHeatCapacity
    {
        get => _maxHeatCapacity;
        set => SetProperty(ref _maxHeatCapacity, value);
    }

    private double _maxDissipationPower = 450;
    public double MaxDissipationPower
    {
        get => _maxDissipationPower;
        set => SetProperty(ref _maxDissipationPower, value);
    }

    private double _preheatCapThreshold = 10;
    public double PreheatCapThreshold
    {
        get => _preheatCapThreshold;
        set => SetProperty(ref _preheatCapThreshold, value);
    }

    private double _notifyHeatCapThreshold = 60;
    public double NotifyHeatCapThreshold
    {
        get => _notifyHeatCapThreshold;
        set => SetProperty(ref _notifyHeatCapThreshold, value);
    }

    private double _alertHeatCapThreshold = 90;
    public double AlertHeatCapThreshold
    {
        get => _alertHeatCapThreshold;
        set => SetProperty(ref _alertHeatCapThreshold, value);
    }

    private double _oilTempUpperThreshold = 0;
    public double OilTempUpperThreshold
    {
        get => _oilTempUpperThreshold;
        set => SetProperty(ref _oilTempUpperThreshold, value);
    }

    private double _smallFocusMaxPower = 0;
    public double SmallFocusMaxPower
    {
        get => _smallFocusMaxPower;
        set => SetProperty(ref _smallFocusMaxPower, value);
    }

    public SourceComponentViewModel(
        IDialogService dialogService,
        ILogger<SourceComponentViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        SourceComponentInfo node = SystemConfig.SourceComponentConfig.SourceComponent;
        SourceCount = node.SourceCount;
        TubeInterfaceCount = node.TubeInterfaceCount;
        MaxHeatCapacity = node.MaxHeatCapacity.Value;
        MaxDissipationPower = node.MaxDissipationPower.Value;
        PreheatCapThreshold = node.PreheatCapThreshold.Value;
        NotifyHeatCapThreshold = node.NotifyHeatCapThreshold.Value;
        AlertHeatCapThreshold = node.AlertHeatCapThreshold.Value;
        OilTempUpperThreshold = node.OilTempUpperThreshold.Value;
    }

    public void Saved()
    {
        SourceComponentInfo node = SystemConfig.SourceComponentConfig.SourceComponent;
        node.SourceCount = SourceCount;
        node.TubeInterfaceCount = TubeInterfaceCount;
        node.MaxHeatCapacity.Value = MaxHeatCapacity;
        node.MaxDissipationPower.Value = MaxDissipationPower;
        node.PreheatCapThreshold.Value = PreheatCapThreshold;
        node.NotifyHeatCapThreshold.Value = NotifyHeatCapThreshold;
        node.AlertHeatCapThreshold.Value = AlertHeatCapThreshold;
        node.OilTempUpperThreshold.Value = OilTempUpperThreshold;
        SystemConfig.SourceComponentConfig.SourceComponent = node;
        bool saveFlag = SystemConfig.SaveSourceComponentConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
}