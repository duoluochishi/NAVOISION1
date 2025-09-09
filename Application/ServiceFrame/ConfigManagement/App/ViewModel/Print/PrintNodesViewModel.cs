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
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.View;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class PrintNodesViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IPrintNodeApplicationService _printNodeApplicationService;
    private ILogger<PrintNodesViewModel> _logger;
    private PrintNodeWindow? _editWindow;

    private ObservableCollection<BasePrintNodeViewModel> _printNodes = new ObservableCollection<BasePrintNodeViewModel>();

    public ObservableCollection<BasePrintNodeViewModel> PrintNodes
    {
        get => _printNodes;
        set => SetProperty(ref _printNodes, value);
    }

    private BasePrintNodeViewModel _selectedNode = new BasePrintNodeViewModel();
    public BasePrintNodeViewModel SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (SetProperty(ref _selectedNode, value) && value is not null)
            {
                IsDefault = !value.IsDefault;
            }
        }
    }

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    public PrintNodesViewModel(IPrintNodeApplicationService printNodeApplicationService,
        IDialogService dialogService,
        ILogger<PrintNodesViewModel> logger)
    {
        _dialogService = dialogService;
        _printNodeApplicationService = printNodeApplicationService;
        _logger = logger;
        Commands.Add("EditCommand", new DelegateCommand(EditCommand));
        Commands.Add("AddCommand", new DelegateCommand(AddCommand));
        Commands.Add("DeleteCommand", new DelegateCommand(DeleteCommand));

        SearchNodes();
        _printNodeApplicationService.PrintNodeReload += PrintNodeApplicationService_PrintNodeReload;
    }

    [UIRoute]
    private void PrintNodeApplicationService_PrintNodeReload(object? sender, EventArgs e)
    {
        SearchNodes();
    }

    public void SearchNodes()
    {
        PrintNodes.Clear();
        foreach (var node in _printNodeApplicationService.GetPrintNodes())
        {
            BasePrintNodeViewModel printNode = new BasePrintNodeViewModel()
            {
                Id = node.Id,
                ServerName = node.Name,
                IP = node.IP,
                Port = node.Port,
                ServerAE = node.AETitle,
                Remark = node.Remark,
                IsDefault = node.IsDefault,
                Layout = node.Layout,
                CreateTime = node.CreateTime,
                AECaller = node.AECaller,
                Resolution = node.Resolution,
                Creator = node.Creator,
                IsUsing = node.IsUsing,
                PaperSize = node.PaperSize,
            };
            PrintNodes.Add(printNode);
        }
        if (PrintNodes.Count > 0)
        {
            SelectedNode = PrintNodes[0];
        }
    }

    private void AddCommand()
    {
        var printNode = new PrinterInfo();
        printNode.Id = Guid.NewGuid().ToString();
        printNode.CreateTime = DateTime.Now;
        printNode.IsDefault = false;
        _printNodeApplicationService.SetPrintNode(OperationType.Add, printNode);
        ShowWindow();
    }

    private void EditCommand()
    {
        if (SelectedNode is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        PrinterInfo printNode = new PrinterInfo()
        {
            Id = SelectedNode.Id,
            AETitle = SelectedNode.ServerAE,
            IP = SelectedNode.IP,
            Port = SelectedNode.Port,
            Name = SelectedNode.ServerName,
            Remark = SelectedNode.Remark,
            IsDefault = SelectedNode.IsDefault,
            CreateTime = SelectedNode.CreateTime,
            AECaller = SelectedNode.AECaller,
            Resolution = SelectedNode.Resolution,
            Creator = SelectedNode.Creator,
            IsUsing = SelectedNode.IsUsing,
            PaperSize = SelectedNode.PaperSize,
            Layout = SelectedNode.Layout
        };
        _printNodeApplicationService.SetPrintNode(OperationType.Edit, printNode);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<PrintNodeWindow>();
        }
        if (_editWindow is not null)
        {
            //_editWindow.ShowWindowDialog();
			_editWindow.ShowPopWindowDialog();
		}
    }

    private void DeleteCommand()
    {
        if (SelectedNode is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a print node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        if (SelectedNode.IsDefault)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current print node because there are  default! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the print node? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _printNodeApplicationService.Delete(SelectedNode.Id))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete print node({SelectedNode.ServerName})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchNodes();
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}