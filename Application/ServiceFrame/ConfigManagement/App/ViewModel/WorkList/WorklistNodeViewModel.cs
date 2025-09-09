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
using MaterialDesignThemes.Wpf;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.DicomUtility.Transfer.CEchoSCU;
using NV.CT.Language;
using NV.MPS.Configuration;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Net;
using System.Threading.Tasks;

namespace NV.CT.ConfigManagement.ViewModel;

public class WorklistNodeViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IWorklistNodeApplicationService _worklistNodeApplicationService;
    private readonly ILogger<WorklistNodeViewModel> _logger;
    private readonly IEchoVerificationHandler _echoVerificationHandler;

    private BaseWorklistNodeViewModel _currentNode = new BaseWorklistNodeViewModel();
    public BaseWorklistNodeViewModel CurrentNode
    {
        get => _currentNode;
        set => SetProperty(ref _currentNode, value);
    }
    private bool _isEdit = false;
    public bool IsEdit
    {
        get => _isEdit;
        set => SetProperty(ref _isEdit, value);
    }

    private bool _isShowLoading = false;
    public bool IsShowLoading
    {
        get => _isShowLoading;
        set => SetProperty(ref _isShowLoading, value);
    }

    private PackIconKind _connectionResult = default;
    public PackIconKind ConnectionResult
    {
        get => _connectionResult;
        set => SetProperty(ref _connectionResult, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public WorklistNodeViewModel(IWorklistNodeApplicationService worklistNodeApplicationService,
        IDialogService dialogService,
        ILogger<WorklistNodeViewModel> logger)
    {
        _dialogService = dialogService;
        _worklistNodeApplicationService = worklistNodeApplicationService;
        _logger = logger;

        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => IsShowLoading == false));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => IsShowLoading == false));
        Commands.Add("ServerTestCommand", new DelegateCommand<object>(CheckServerConnection, _ => IsShowLoading == false));

        _worklistNodeApplicationService.WorklistChanged += WorklistNodeApplicationService_WorklistChanged;
        _echoVerificationHandler = new EchoVerificationHandler();
    }

    [UIRoute]
    private void WorklistNodeApplicationService_WorklistChanged(object? sender, EventArgs<(OperationType operation, WorklistInfo worklistInfo)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetNodeInfo(e.Data.worklistInfo);
        if (OperationType == OperationType.Edit)
        {
            IsEdit = false;
        }
        if (OperationType == OperationType.Add)
        {
            IsEdit = true;
        }
    }

    private void SetNodeInfo(WorklistInfo worklistInfo)
    {
        CurrentNode = new BaseWorklistNodeViewModel();
        CurrentNode.ID = worklistInfo.Id;
        CurrentNode.ServerName = worklistInfo.Name;
        CurrentNode.IP = worklistInfo.IP;
        CurrentNode.Port = worklistInfo.Port;
        CurrentNode.ServerAE = worklistInfo.AETitle;
        CurrentNode.Remark = worklistInfo.Remark;
        CurrentNode.IsDefault = worklistInfo.IsDefault;
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty() || !CheckIpPort() || !UniquenessCheck()|| !CheckNumAndEnChForm())
		{
            return;
        }
		IPAddress ip = IPAddress.None;
		IPAddress.TryParse(CurrentNode.IP, out ip);
		CurrentNode.IP = ip.ToString();

		WorklistInfo worklistInfo = new WorklistInfo();
        worklistInfo.Id = CurrentNode.ID;
        worklistInfo.Name = CurrentNode.ServerName;
        worklistInfo.IP = CurrentNode.IP;
        worklistInfo.Port = CurrentNode.Port;
        worklistInfo.AETitle = CurrentNode.ServerAE;
        worklistInfo.Remark = CurrentNode.Remark;
        worklistInfo.IsDefault = CurrentNode.IsDefault;

        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _worklistNodeApplicationService.Add(worklistInfo);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _worklistNodeApplicationService.Update(worklistInfo);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  ConnectionResult = default;
                  _worklistNodeApplicationService.ReloadWorklist();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentNode.ServerAE))
        {
            sb.Append(string.Format(message, "ServerAE"));
            flag = false;
        }
        if (string.IsNullOrEmpty(CurrentNode.IP))
        {
            sb.Append(string.Format(message, "IP"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckIpPort()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} Incorrect input!";
        if (!VerificationExtension.IpFormatVerification(CurrentNode.IP))
        {
            sb.Append(string.Format(message, "IP"));
            flag = false;
        }
        if (!VerificationExtension.PortFormatVerification(CurrentNode.Port.ToString()))
        {
            sb.Append(string.Format(message, "Port"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool UniquenessCheck()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} non-repeatable!";
        switch (OperationType)
        {
            case OperationType.Add:
                var addm = _worklistNodeApplicationService.GetWorklist().FirstOrDefault(t => t.AETitle.Equals(CurrentNode.ServerAE) || t.IP.Equals(CurrentNode.IP));
                if (addm is not null)
                {
                    sb.Append(string.Format(message, "ServerAE or IP"));
                    flag = false;
                }
                break;
            case OperationType.Edit:
                var editm = _worklistNodeApplicationService.GetWorklist().FirstOrDefault(t => !t.Id.Equals(CurrentNode.ID) && (t.AETitle.Equals(CurrentNode.ServerAE) || t.IP.Equals(CurrentNode.IP)));
                if (editm is not null)
                {
                    sb.Append(string.Format(message, "ServerAE or IP"));
                    flag = false;
                }
                break;
            default:
                break;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

	private bool CheckNumAndEnChForm()
	{
		bool flag = true;
		string message = "";
		if (VerificationExtension.IsSpecialCharacters(CurrentNode.ServerAE))
		{
			flag = false;
			message += $"ServerAE:Special characters are not allowed!";
		}
		if (!flag)
		{
			_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
				arg => { }, ConsoleSystemHelper.WindowHwnd);
		}
		return flag;
	}

	public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            ConnectionResult = default;
            _worklistNodeApplicationService.ReloadWorklist();
            window.Hide();
        }
    }
    public async void CheckServerConnection(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty() || !CheckIpPort() || !CheckNumAndEnChForm())
        {
            return;
        }
        try
        {
            IsShowLoading = true;
            (bool, string) echoResult = default;
            await Task.Run(() =>
            {
                echoResult = _echoVerificationHandler.VerifyEcho(CurrentNode.IP, CurrentNode.Port, CurrentNode.ServerName, CurrentNode.ServerAE);
            });

            if (echoResult.Item1 == false)
                ConnectionResult = PackIconKind.Close;
            else
                ConnectionResult = PackIconKind.Check;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Show execption in CheckServerConnection: {ex}");
        }
        finally
        {
            IsShowLoading = false;
        }
    }
}