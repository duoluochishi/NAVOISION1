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

using NV.CT.DatabaseService.Contract;
using NV.CT.Logging;

namespace NV.CT.UI.Exam.ViewModel;

public class DoseNotificationViewModel : BaseViewModel
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IGoService _goService;
    private readonly IDialogService _dialogService;
    private readonly IStudyHostService _studyHostService;
    private readonly IDoseCheckService _doseCheckService;
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

    private List<DoseCheckModel> _doseCheckModels = new List<DoseCheckModel>();

    public DoseNotificationViewModel(IGoValidateDialogService goValidateDialogService,
        IGoService goService,
        IDialogService dialogService,
        IStudyHostService studyHostService,
        IDoseCheckService doseCheckService)
    {
        _goValidateDialogService = goValidateDialogService;
        _goService = goService;
        _dialogService = dialogService;
        _studyHostService = studyHostService;
        _doseCheckService = doseCheckService;
        Commands.Add(CommandParameters.COMMAND_LOAD, new DelegateCommand<object>(Loaded, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        _goValidateDialogService.PopValidateMessage += ValidateDialogService_PopValidateMessage;
    }

    [UIRoute]
    private void ValidateDialogService_PopValidateMessage(object? sender, EventArgs<(string message, RuleDialogType ruleDialogType, object? extendModel)> e)
    {
        if (e is null || e.Data.ruleDialogType != RuleDialogType.DoseNotificationDialog)
        {
            return;
        }
        Message = e.Data.message;
        DiagnosticReason = string.Empty;

        if (e.Data.extendModel is List<DoseCheckModel> doseCheckModel)
        {
            _doseCheckModels = doseCheckModel;
        }
        Task.Run(() =>
        {
            SetAuditLog($"{this.GetType().Name}", $"Load dose notification message to windows :{JsonConvert.SerializeObject(_doseCheckModels)}");
        });
    }

    private void Loaded(object parameter)
    {
        if (string.IsNullOrEmpty(DiagnosticReason))
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_EnterDiagnosticInstructions, arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        foreach (DoseCheckModel doseCheckModel in _doseCheckModels)
        {
            doseCheckModel.Reason = DiagnosticReason;
            doseCheckModel.InternalPatientId = _studyHostService.Instance.InternalPatientId;
            doseCheckModel.InternalStudyId = _studyHostService.Instance.ID;
            doseCheckModel.CreateTime = DateTime.Now;
        }
        Task.Run(() =>
        {
            _doseCheckService.AddList(_doseCheckModels);
            SetAuditLog($"{this.GetType().Name}", $"Save dose notification message to db:{JsonConvert.SerializeObject(_doseCheckModels)}");
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