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
using System.Net;

namespace NV.CT.ConfigManagement.ViewModel;

public class LocalDicomNodeViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<LocalDicomNodeViewModel> _logger;

    private string _serverEAE = string.Empty;
    public string ServiceAE
    {
        get => _serverEAE;
        set => SetProperty(ref _serverEAE, value);
    }

    private string _ip = string.Empty;
    public string IP
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }

    private int _port = 0;
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    private string _userAE = string.Empty;
    public string UserAE
    {
        get => _userAE;
        set => SetProperty(ref _userAE, value);
    }

    private string _implVersion = string.Empty;
    public string ImplVersion
    {
        get => _implVersion;
        set => SetProperty(ref _implVersion, value);
    }

    private string _implUID = string.Empty;
    public string ImplUID
    {
        get => _implUID;
        set => SetProperty(ref _implUID, value);
    }

    private string _supportServiceType = string.Empty;
    public string SupportServiceType
    {
        get => _supportServiceType;
        set => SetProperty(ref _supportServiceType, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public LocalDicomNodeViewModel(
        IDialogService dialogService,
        ILogger<LocalDicomNodeViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        LocalDicomSettingInfo node = UserConfig.LocalDicomSettingConfig.LocalDicomSetting;
        ServiceAE = node.SCPAETitle;
        IP = node.IP;
        Port = node.Port;
        UserAE = node.UserAETitle;
        ImplVersion = node.DicomVersion;
        ImplUID = node.ImplementationClassUID;
        SupportServiceType = node.SupportServiceType;
    }

    public void Saved()
    {
        if (!CheckFormEmpty() || !CheckIpPort() || !CheckNumAndEnChForm())
        {
            return;
        }
        IPAddress ip = IPAddress.None;
        IPAddress.TryParse(IP, out ip);
        IP = ip.ToString();

        LocalDicomSettingInfo node = UserConfig.LocalDicomSettingConfig.LocalDicomSetting;
        node.SCPAETitle = ServiceAE;
        node.IP = IP;
        node.Port = Port;
        node.UserAETitle = UserAE;
        node.DicomVersion = ImplUID;
        node.ImplementationClassUID = ImplUID;
        node.SupportServiceType = SupportServiceType;

        UserConfig.LocalDicomSettingConfig.LocalDicomSetting = node;
        bool saveFlag = UserConfig.SaveLocalDicomSetting();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
           arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(ServiceAE))
        {
            sb.Append(string.Format(message, "ServiceAE"));
            flag = false;
        }
        if (string.IsNullOrEmpty(IP))
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
        if (!VerificationExtension.IpFormatVerification(IP))
        {
            sb.Append(string.Format(message, "IP"));
            flag = false;
        }
        if (!VerificationExtension.PortFormatVerification(Port.ToString()))
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
        if (VerificationExtension.IsSpecialCharacters(ServiceAE))
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
}