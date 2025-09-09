//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.UI.Exam.ViewModel;

public class CommonNotificationViewModel : BaseViewModel
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IGoService _goService;
    private string _message = $"Abdomen:common exceeds the notification value(mGy).";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public CommonNotificationViewModel(IGoValidateDialogService goValidateDialogService, IGoService goService)
    {
        _goValidateDialogService = goValidateDialogService;
        Commands.Add(CommandParameters.COMMAND_LOAD, new DelegateCommand<object>(Loaded, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        _goValidateDialogService.PopValidateMessage += ValidateDialogService_PopValidateMessage;
        _goService = goService;
    }

    [UIRoute]
    private void ValidateDialogService_PopValidateMessage(object? sender, EventArgs<(string message, RuleDialogType ruleDialogType,object? extendModel)> e)
    {
        if (e is null || e.Data.ruleDialogType != RuleDialogType.CommonNotificationDialog)
        {
            return;
        }
        Message = e.Data.message;
    }

    private void Loaded(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    public void Closed(object parameter)
    {
        _goService.StopValidated(true);
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}