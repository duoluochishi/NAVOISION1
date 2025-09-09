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

using MaterialDesignThemes.Wpf;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.DicomUtility.Transfer.CEchoSCU;
using NV.CT.Language;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Net;
using System.Threading.Tasks;

namespace NV.CT.ConfigManagement.ViewModel;

public class ArchiveNodeViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IArchiveNodeApplicationService _archiveNodeApplicationService;
    private readonly ILogger<ArchiveNodeViewModel> _logger;
    private readonly IEchoVerificationHandler _echoVerificationHandler;
    private BaseArchiveNodeViewModel _currentNode = new BaseArchiveNodeViewModel();
    public BaseArchiveNodeViewModel CurrentNode
    {
        get => _currentNode;
        set => SetProperty(ref _currentNode, value);
    }

    private bool _isEdit = true;
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

    public ArchiveNodeViewModel(IArchiveNodeApplicationService archiveNodeApplicationService,
        IDialogService dialogService,
        ILogger<ArchiveNodeViewModel> logger)
    {
        _dialogService = dialogService;
        _archiveNodeApplicationService = archiveNodeApplicationService;
        _logger = logger;

        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => IsShowLoading == false));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => IsShowLoading == false));
        Commands.Add("ServerTestCommand", new DelegateCommand<object>(CheckServerConnection, _ => IsShowLoading == false));
        _archiveNodeApplicationService.ArchiveNodeChanged += ArchiveNodeApplicationService_ArchiveNodeChanged;
        _echoVerificationHandler = new EchoVerificationHandler();
    }

    [UIRoute]
    private void ArchiveNodeApplicationService_ArchiveNodeChanged(object? sender, EventArgs<(OperationType operation, ArchiveInfo archiveInfo)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetNodeInfo(e.Data.archiveInfo);
        if (OperationType == OperationType.Edit)
        {
            IsEdit = false;
        }
        if (OperationType == OperationType.Add)
        {
            IsEdit = true;
        }
    }

    private void SetNodeInfo(ArchiveInfo archiveInfo)
    {
        CurrentNode = new BaseArchiveNodeViewModel();
        CurrentNode.ID = archiveInfo.Id;
        CurrentNode.ServerAE = archiveInfo.ServerAETitle;
        CurrentNode.IP = archiveInfo.IP;
        CurrentNode.Port = archiveInfo.Port;
        CurrentNode.ClientAE = archiveInfo.ClientAETitle;
        CurrentNode.Remark = archiveInfo.Remark;
        CurrentNode.IsDefault = archiveInfo.IsDefault;
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty() || !CheckIpPort() || !CheckNumAndEnChForm())
        {
            return;
        }
        IPAddress ip = IPAddress.None;
        IPAddress.TryParse(CurrentNode.IP, out ip);
        CurrentNode.IP = ip.ToString();

        ArchiveInfo archiveInfo = new ArchiveInfo();
        archiveInfo.Id = CurrentNode.ID;
        archiveInfo.ServerAETitle = CurrentNode.ServerAE;
        archiveInfo.IP = CurrentNode.IP;
        archiveInfo.Port = CurrentNode.Port;
        archiveInfo.ClientAETitle = CurrentNode.ClientAE;
        archiveInfo.Remark = CurrentNode.Remark;
        archiveInfo.IsDefault = CurrentNode.IsDefault;

        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _archiveNodeApplicationService.Add(archiveInfo);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _archiveNodeApplicationService.Update(archiveInfo);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  ConnectionResult = default;
                  _archiveNodeApplicationService.ReloadArchiveNodes();
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
            _archiveNodeApplicationService.ReloadArchiveNodes();
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
                echoResult = _echoVerificationHandler.VerifyEcho(CurrentNode.IP, CurrentNode.Port, CurrentNode.ClientAE, CurrentNode.ServerAE);
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