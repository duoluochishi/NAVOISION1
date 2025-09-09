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

public class AutoPositioningViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<AutoPositioningViewModel> _logger;

    private string _resolution = (1280 * 768).ToString();
    public string Resolution
    {
        get => _resolution;
        set => SetProperty(ref _resolution, value);
    }

    private double _minPixelSize = 1.85;
    public double MinPixelSize
    {
        get => _minPixelSize;
        set => SetProperty(ref _minPixelSize, value);
    }

    private double _maxPixelSize = 1.85;
    public double MaxPixelSize
    {
        get => _maxPixelSize;
        set => SetProperty(ref _maxPixelSize, value);
    }

    private double _leftImageOffset = 0;
    public double LeftImageOffset
    {
        get => _leftImageOffset;
        set => SetProperty(ref _leftImageOffset, value);
    }

    private double _iSOCenterOffset = 0;
    public double ISOCenterOffset
    {
        get => _iSOCenterOffset;
        set => SetProperty(ref _iSOCenterOffset, value);
    }

    public AutoPositioningViewModel(
        IDialogService dialogService,
        ILogger<AutoPositioningViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;

        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        AutoPositioningInfo node = SystemConfig.AutoPositioningConfig.AutoPositioning;
        Resolution = node.Resolution.Value;
        MaxPixelSize = node.PixelSize.Max;
        MinPixelSize = node.PixelSize.Min;
        LeftImageOffset = node.LeftImageOffset.Value;
        ISOCenterOffset = node.ISOCenterOffset.Value;
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        AutoPositioningInfo node = SystemConfig.AutoPositioningConfig.AutoPositioning;
        node.Resolution.Value = Resolution;
        node.PixelSize.Max = MaxPixelSize;
        node.PixelSize.Min = MinPixelSize;
        node.LeftImageOffset.Value = LeftImageOffset;
        node.ISOCenterOffset.Value = ISOCenterOffset;

        SystemConfig.AutoPositioningConfig.AutoPositioning = node;
        bool saveFlag = SystemConfig.SaveAutoPositioningConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (MaxPixelSize < MinPixelSize)
        {
            sb.Append("Pixel size max cannot be smaller than pixel size min!");
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