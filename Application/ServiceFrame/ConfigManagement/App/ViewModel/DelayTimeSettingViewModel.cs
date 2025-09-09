//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:59    V1.0.0       jianggang
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

public class DelayTimeSettingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<DelayTimeSettingViewModel> _logger;
    private double rDelay = 1000;
    public double RDelay
    {
        get => rDelay;
        set => SetProperty(ref rDelay, value);
    }

    private double tDelay = 0;
    public double TDelay
    {
        get => tDelay;
        set => SetProperty(ref tDelay, value);
    }

    private double collimatorSpotDelay = 1000;
    public double CollimatorSpotDelay
    {
        get => collimatorSpotDelay;
        set => SetProperty(ref collimatorSpotDelay, value);
    }

    private double spotDelay = 0;
    public double SpotDelay
    {
        get => spotDelay;
        set => SetProperty(ref spotDelay, value);
    }

    public DelayTimeSettingViewModel(IDialogService dialogService,
         ILogger<DelayTimeSettingViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;

        Commands.Add("SaveCommand", new DelegateCommand(SaveCommand));
    }
    private OperationType operationType;
    public OperationType OperationType
    {
        get => operationType;
        set => SetProperty(ref operationType, value);
    }
    public void SaveCommand()
    {
        try
        {
            //CurrentDoseConfig.Alerts.AlertChild.CTDI = int.Parse(CurrentChildCTDIVal);
            //CurrentDoseConfig.Alerts.AlertChild.DLP = int.Parse(CurrentChildDLPVal);
            //CurrentDoseConfig.Alerts.AlertAdult.CTDI = int.Parse(CurrentAdultCTDIVal);
            //CurrentDoseConfig.Alerts.AlertAdult.DLP = int.Parse(CurrentAdultDLPVal);
            //CurrentDoseConfig.Notices.IsNotice = CurrentIsNotice;

            //ConfigService.Models.DoseConfig doseConfig = _doseSettingService.GetConfigs();

            //doseConfig = CurrentDoseConfig;

            //_doseSettingService.Save(doseConfig);
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseInformationTitle, "Successfully saved!", arg =>
            {
            }, ConsoleSystemHelper.WindowHwnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            _dialogService.ShowDialog(false, MessageLeveles.Error, LanguageResource.Message_Info_CloseErrorTitle, "Save failed!", arg =>
            {
            }, ConsoleSystemHelper.WindowHwnd);
        }
    }
}