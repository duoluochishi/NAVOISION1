//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/31 14:45:22           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Encryptions;
using NV.CT.DatabaseService.Contract;
using NV.CT.Logging;
using NV.CT.Models;

namespace NV.CT.UI.Exam.ViewModel;

public class DoseAlertViewModel : BaseViewModel
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IGoService _goService;
    private readonly IDialogService _dialogService;
    private readonly IStudyHostService _studyHostService;
    private readonly IDoseCheckService _doseCheckService;
    private readonly IAuthorization _authorization;
    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private string _diagnosticReason = string.Empty;
    public string DiagnosticReason
    {
        get => _diagnosticReason;
        set => SetProperty(ref _diagnosticReason, value);
    }

    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private List<DoseCheckModel> _doseCheckModels = new List<DoseCheckModel>();
    public DoseAlertViewModel(IGoValidateDialogService goValidateDialogService,
        IGoService goService,
        IDialogService dialogService,
        IStudyHostService studyHostService,
        IDoseCheckService doseCheckService,
        IAuthorization authorization)
    {
        _goValidateDialogService = goValidateDialogService;
        _goService = goService;
        _dialogService = dialogService;
        _studyHostService = studyHostService;
        _doseCheckService = doseCheckService;
        Commands.Add(CommandParameters.COMMAND_LOAD, new DelegateCommand<object>(Loaded, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        Commands.Add(CommandParameters.COMMAND_PASSWORDCHANGED, new DelegateCommand<object>(PasswordChanged, _ => true));
        _goValidateDialogService.PopValidateMessage += ValidateDialogService_PopValidateMessage;
        _authorization = authorization;
    }

    [UIRoute]
    private void ValidateDialogService_PopValidateMessage(object? sender, EventArgs<(string message, RuleDialogType ruleDialogType, object? extendModel)> e)
    {
        if (e is null || e.Data.ruleDialogType != RuleDialogType.DoseAlertDialog)
        {
            return;
        }
        Message = e.Data.message;
        UserName = string.Empty;
        DiagnosticReason = string.Empty;
        Password = string.Empty;

        if (e.Data.extendModel is List<DoseCheckModel> doseCheckModel)
        {
            _doseCheckModels = doseCheckModel;
        }
        Task.Run(() =>
        {
            SetAuditLog($"{this.GetType().Name}", $"Load dose alert message to windows :{JsonConvert.SerializeObject(_doseCheckModels)}");
        });
    }

    private void PasswordChanged(object password)
    {
        if (password is PasswordBox passwordBox)
        {
            Password = passwordBox.Password;
        }
    }

    private void Loaded(object parameter)
    {
        if (string.IsNullOrEmpty(DiagnosticReason))
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_EnterDiagnosticInstructions, arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (string.IsNullOrEmpty(Password))
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_EnterCorrectPassword, arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        AuthorizationRequest authorizationRequest = new AuthorizationRequest(UserName, MD5Helper.Encrypt(Password), SystemPermissionNames.EXAM_EXCEED_ALERT_SCAN);
        AuthorizationResult result = _authorization.AuthenticationOtherUser(authorizationRequest);

        if (!(result is not null && result.IsSuccess && result.HasPermissions))
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_EnterCorrectPassword, arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        foreach (DoseCheckModel doseCheckModel in _doseCheckModels)
        {
            doseCheckModel.Reason = DiagnosticReason;
            doseCheckModel.Operator = UserName;
            doseCheckModel.InternalPatientId = _studyHostService.Instance.InternalPatientId;
            doseCheckModel.InternalStudyId = _studyHostService.Instance.ID;
            doseCheckModel.CreateTime = DateTime.Now;
        }

        Task.Run(() =>
        {
            _doseCheckService.AddList(_doseCheckModels);
            SetAuditLog($"{this.GetType().Name}", $"Save dose alert message to db:{JsonConvert.SerializeObject(_doseCheckModels)}");
        });
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

    private void SetAuditLog(string eventName, string message)
    {
        AuditLogger.Log(new AuditLogInfo
        {
            CreateTime = DateTime.Now,
            EventType = "Action",
            EntryPoint = eventName,
            UserName = Environment.UserName,
            Description = message,
            OriginalValues = JsonConvert.SerializeObject(null),
            ReturnValues = JsonConvert.SerializeObject(null)
        });
    }
}