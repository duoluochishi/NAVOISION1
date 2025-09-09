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
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.Extensions;
using NV.CT.ConfigManagement.View;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.ConfigService.Models.UserConfig;
using System.Collections.Generic;
using System.Windows.Controls;
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class PrintProtocolListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IPrintProtocolApplicationService _printProtocolApplicationService;
    private ILogger<PrintProtocolListViewModel> _logger;
    private PrintProtocolWindow? _editWindow;

    private ObservableCollection<BasePrintProtocolViewModel> _dataList = new ObservableCollection<BasePrintProtocolViewModel>();

    public ObservableCollection<BasePrintProtocolViewModel> DataList
    {
        get => _dataList;
        set => SetProperty(ref _dataList, value);
    }

    private ObservableCollection<KeyValuePair<string, string>> _bodyPartlist = new ObservableCollection<KeyValuePair<string, string>>();
    public ObservableCollection<KeyValuePair<string, string>> BodyPartList
    {
        get => _bodyPartlist;
        set => SetProperty(ref _bodyPartlist, value);
    }

    private KeyValuePair<string, string> _selectedBody = new KeyValuePair<string, string>();
    public KeyValuePair<string, string> SelectedBody
    {
        get => _selectedBody;
        set
        {
            if (SetProperty(ref _selectedBody, value))
            {
                Search(value.Value);
            }
        }
    }

    private BasePrintProtocolViewModel _selectedNode = new BasePrintProtocolViewModel();
    public BasePrintProtocolViewModel SelectedNode
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

    public PrintProtocolListViewModel(IPrintProtocolApplicationService printProtocolApplicationService,
        IDialogService dialogService,
        ILogger<PrintProtocolListViewModel> logger)
    {
        _dialogService = dialogService;
        _printProtocolApplicationService = printProtocolApplicationService;
        _logger = logger;
        InitBodyPartList();
        Commands.Add("EditCommand", new DelegateCommand(EditCommand));
        Commands.Add("AddCommand", new DelegateCommand(AddCommand));
        Commands.Add("DeleteCommand", new DelegateCommand(DeleteCommand));

        if (BodyPartList.Count > 0)
        {
            SelectedBody = BodyPartList[0];
        }
        Search(string.Empty);
        _printProtocolApplicationService.Reloaded += PrintProtocolApplicationService_Reloaded;
    }

    [UIRoute]
    private void PrintProtocolApplicationService_Reloaded(object? sender, EventArgs e)
    {
        Search(SelectedBody.Value);
    }

    private void InitBodyPartList()
    {
        BodyPartList.Add(new KeyValuePair<string, string>(" ", "All"));
        foreach (var enumItem in Enum.GetValues(typeof(BodyPart)))
        {
            if (enumItem is not null)
            {
                BodyPartList.Add(new KeyValuePair<string, string>(enumItem.ToString(), enumItem.ToString()));
            }
        }
    }

    public void Search(string bodyPart)
    {
        DataList.Clear();
        var list = _printProtocolApplicationService.Get();
        if (!string.IsNullOrEmpty(bodyPart) && !bodyPart.Equals("All"))
        {
            list = list.FindAll(t => t.BodyPart.Equals(bodyPart));
        }
        foreach (var node in list)
        {
            BasePrintProtocolViewModel bNode = new BasePrintProtocolViewModel()
            {
                ID = node.Id,
                Name = node.Name,
                IsSystem = node.IsSystem,
                BodyPart = Enum.Parse<BodyPart>(node.BodyPart),
                Row = node.Row,
                Column = node.Column,
                IsDefault = node.IsDefault
            };
            DataList.Add(bNode);
        }
        if (DataList.Count > 0)
        {
            SelectedNode = DataList[0];
        }
    }

    private void AddCommand()
    {
        var info = new PrintProtocol();
        info.Id = Guid.NewGuid().ToString();
        info.IsDefault = false;
        info.IsSystem = false;
        info.BodyPart = BodyPart.Abdomen.ToString();
        info.Row = 5;
        info.Column = 5;

        _printProtocolApplicationService.Set(OperationType.Add, info);
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
        PrintProtocol info = new PrintProtocol()
        {
            Id = SelectedNode.ID,
            Name = SelectedNode.Name,
            IsSystem = SelectedNode.IsSystem,
            BodyPart = SelectedNode.BodyPart.ToString(),
            Row = SelectedNode.Row,
            Column = SelectedNode.Column,
            IsDefault = SelectedNode.IsDefault
        };
        _printProtocolApplicationService.Set(OperationType.Edit, info);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<PrintProtocolWindow>();
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
              , "Please select a node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (SelectedNode.IsDefault || SelectedNode.IsSystem)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current print protocol node because there are default or system! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the print protocol node? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _printProtocolApplicationService.Delete(SelectedNode.ID))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete print protocol node({SelectedNode.Name})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    Search(SelectedBody.Value);
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}