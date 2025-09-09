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
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.CommonAttributeUI.AOPAttribute;
using System.Collections.Generic;
using NV.CT.Language;
using System.Net;

namespace NV.CT.ConfigManagement.ViewModel;

public class PrintNodeViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IPrintNodeApplicationService _printNodeApplicationService;
    private readonly ILogger<PrintNodeViewModel> _logger;

    private BasePrintNodeViewModel _currentNode = new BasePrintNodeViewModel();
    public BasePrintNodeViewModel CurrentNode
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

    private ObservableCollection<KeyValuePair<int, string>> _resolutions = new ObservableCollection<KeyValuePair<int, string>>();
    public ObservableCollection<KeyValuePair<int, string>> Resolutions
    {
        get => _resolutions;
        set => SetProperty(ref _resolutions, value);
    }

    private ObservableCollection<KeyValuePair<string, string>> _layouts = new ObservableCollection<KeyValuePair<string, string>>();
    public ObservableCollection<KeyValuePair<string, string>> Layouts
    {
        get => _layouts;
        set => SetProperty(ref _layouts, value);
    }

    private ObservableCollection<KeyValuePair<string, string>> _paperSizes = new ObservableCollection<KeyValuePair<string, string>>();
    public ObservableCollection<KeyValuePair<string, string>> PaperSizes
    {
        get => _paperSizes;
        set => SetProperty(ref _paperSizes, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public PrintNodeViewModel(IPrintNodeApplicationService printNodeApplicationService,
        IDialogService dialogService,
        ILogger<PrintNodeViewModel> logger)
    {
        _dialogService = dialogService;
        _printNodeApplicationService = printNodeApplicationService;
        _logger = logger;
        InitList();
        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));

        _printNodeApplicationService.PrintNodeChanged += PrintNodeApplicationService_PrintNodeChanged;
    }

    [UIRoute]
    private void PrintNodeApplicationService_PrintNodeChanged(object? sender, EventArgs<(OperationType operation, PrinterInfo printInfo)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetNodeInfo(e.Data.printInfo);
        if (OperationType == OperationType.Edit)
        {
            IsEdit = false;
        }
        if (OperationType == OperationType.Add)
        {
            IsEdit = true;
        }
    }

    private void InitList()
    {
        Resolutions = new ObservableCollection<KeyValuePair<int, string>>(new KeyValuePair<int, string>[] {
                new KeyValuePair<int,string>(254,"254"),
                new KeyValuePair<int,string>(320,"320"),
                new KeyValuePair<int,string>(508, "508"),
              });

        Layouts = new ObservableCollection<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("2x2","2x2"),
                new KeyValuePair<string,string>("3x2", "3x2"),
                new KeyValuePair<string,string>("3x3", "3x3"),
                new KeyValuePair<string,string>("4x4", "4x4"),
                new KeyValuePair<string,string>("4x5", "4x5"),
                new KeyValuePair<string,string>("5x5", "5x5"),
                new KeyValuePair<string,string>("5x6", "5x6"),
                new KeyValuePair<string,string>("5x7", "5x7"),
                new KeyValuePair<string,string>("6x6", "6x6"),
                new KeyValuePair<string,string>("6x7", "6x7"),
                new KeyValuePair<string,string>("6x8","6x8")});

        PaperSizes = new ObservableCollection<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("8INX10IN","8INX10IN"),
                new KeyValuePair<string,string>("10INX12IN", "10INX12IN"),
                new KeyValuePair<string,string>("11INX14IN", "11INX14IN"),
                new KeyValuePair<string,string>("14INX17IN", "14INX17IN")});
    }

    private void SetNodeInfo(PrinterInfo printInfo)
    {
        CurrentNode = new BasePrintNodeViewModel();
        CurrentNode.Id = printInfo.Id;
        CurrentNode.ServerName = printInfo.Name;
        CurrentNode.IP = printInfo.IP;
        CurrentNode.Port = printInfo.Port;
        CurrentNode.ServerAE = printInfo.AETitle;
        CurrentNode.Remark = printInfo.Remark;
        CurrentNode.IsDefault = printInfo.IsDefault;
        CurrentNode.CreateTime = printInfo.CreateTime;
        CurrentNode.AECaller = printInfo.AECaller;
        CurrentNode.Resolution = printInfo.Resolution;
        CurrentNode.Creator = printInfo.Creator;
        CurrentNode.IsUsing = printInfo.IsUsing;
        CurrentNode.PaperSize = printInfo.PaperSize;
        CurrentNode.Layout = printInfo.Layout;
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckNumAndEnChForm() || !CheckFormEmpty() || !CheckIpPort() || !UniquenessCheck())
        {
            return;
        }
        IPAddress ip = IPAddress.None;
        IPAddress.TryParse(CurrentNode.IP, out ip);
        CurrentNode.IP = ip.ToString();

        PrinterInfo printInfo = new PrinterInfo();
        printInfo.Id = CurrentNode.Id;
        printInfo.Name = CurrentNode.ServerName;
        printInfo.IP = CurrentNode.IP;
        printInfo.Port = CurrentNode.Port;
        printInfo.AETitle = CurrentNode.ServerAE;
        printInfo.Remark = CurrentNode.Remark;
        printInfo.IsDefault = CurrentNode.IsDefault;
        printInfo.Layout = CurrentNode.Layout;
        printInfo.CreateTime = CurrentNode.CreateTime;
        printInfo.AECaller = CurrentNode.AECaller;
        printInfo.Resolution = CurrentNode.Resolution;
        printInfo.Creator = CurrentNode.Creator;
        printInfo.IsUsing = CurrentNode.IsUsing;
        printInfo.PaperSize = CurrentNode.PaperSize;

        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _printNodeApplicationService.Add(printInfo);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _printNodeApplicationService.Update(printInfo);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _printNodeApplicationService.ReloadPrintNode();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentNode.ServerName))
        {
            sb.Append(string.Format(message, "ServerName"));
            flag = false;
        }
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

    private bool UniquenessCheck()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} non-repeatable!";
        switch (OperationType)
        {
            case OperationType.Add:
                var addm = _printNodeApplicationService.GetPrintNodes().FirstOrDefault(t => t.AETitle.Equals(CurrentNode.ServerAE) || t.IP.Equals(CurrentNode.IP));
                if (addm is not null)
                {
                    sb.Append(string.Format(message, "ServerAE or IP"));
                    flag = false;
                }
                break;
            case OperationType.Edit:
                var editm = _printNodeApplicationService.GetPrintNodes().FirstOrDefault(t => !t.Id.Equals(CurrentNode.Id) && (t.AETitle.Equals(CurrentNode.ServerAE) || t.IP.Equals(CurrentNode.IP)));
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
            _printNodeApplicationService.ReloadPrintNode();
            window.Hide();
        }
    }
}