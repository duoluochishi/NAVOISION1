//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.UI.Exam.ViewModel;

public class CommonWarningViewModel : BaseViewModel
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private string _message = $"Abdomen:common exceeds the notification value(mGy).";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public CommonWarningViewModel(IGoValidateDialogService goValidateDialogService)
    {
        _goValidateDialogService = goValidateDialogService;
        Commands.Add(CommandParameters.COMMAND_LOAD, new DelegateCommand<object>(Loaded, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        _goValidateDialogService.PopValidateMessage += ValidateDialogService_PopValidateMessage;
    }

    [UIRoute]
    private void ValidateDialogService_PopValidateMessage(object? sender, EventArgs<(string message, RuleDialogType ruleDialogType, object? extendModel)> e)
    {
        if (e is null || e.Data.ruleDialogType != RuleDialogType.WarningDialog)
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
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}