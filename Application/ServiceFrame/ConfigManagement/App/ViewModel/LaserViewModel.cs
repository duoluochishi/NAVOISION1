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

public class LaserViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<LaserViewModel> _logger;

    private int _offset = 1;
    public int Offset
    {
        get => _offset;
        set => SetProperty(ref _offset, value);
    }

    public LaserViewModel(
        IDialogService dialogService,
        ILogger<LaserViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        LaserInfo node = SystemConfig.LaserConfig.Laser;
        Offset = UnitConvert.Micron2Millimeter(node.Offset.Value);
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }
        LaserInfo node = SystemConfig.LaserConfig.Laser;
        node.Offset.Value = UnitConvert.Millimeter2Micron(Offset);
        SystemConfig.LaserConfig.Laser = node;
        bool saveFlag = SystemConfig.SaveLaserConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (Offset < 0)
        {
            sb.Append("Offset cannot be smaller than 0!");
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