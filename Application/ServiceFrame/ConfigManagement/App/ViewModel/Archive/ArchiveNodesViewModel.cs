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
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class ArchiveNodesViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IArchiveNodeApplicationService _archiveNodeApplicationService;
    private ILogger<VoiceListViewModel> _logger;
    private ArchiveNodeWindow? _editWindow;

    private ObservableCollection<BaseArchiveNodeViewModel> _archiveNodes = new ObservableCollection<BaseArchiveNodeViewModel>();

    public ObservableCollection<BaseArchiveNodeViewModel> ArchiveNodes
    {
        get => _archiveNodes;
        set => SetProperty(ref _archiveNodes, value);
    }

    private BaseArchiveNodeViewModel _selectedArchiveNode = new BaseArchiveNodeViewModel();
    public BaseArchiveNodeViewModel SelectedArchiveNode
    {
        get => _selectedArchiveNode;
        set
        {
            if (SetProperty(ref _selectedArchiveNode, value) && value is not null)
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

    public ArchiveNodesViewModel(IArchiveNodeApplicationService archiveNodeApplicationService,
        IDialogService dialogService,
        ILogger<VoiceListViewModel> logger)
    {
        _dialogService = dialogService;
        _archiveNodeApplicationService = archiveNodeApplicationService;
        _logger = logger;
        Commands.Add("EditCommand", new DelegateCommand(EditCommand));
        Commands.Add("AddCommand", new DelegateCommand(AddCommand));
        Commands.Add("DeleteCommand", new DelegateCommand(DeleteCommand));

        SearchArchiveNodes();
        _archiveNodeApplicationService.ArchiveNodeReload += ArchiveNodeApplicationService_ArchiveNodeReload;
    }

    [UIRoute]
    private void ArchiveNodeApplicationService_ArchiveNodeReload(object? sender, EventArgs e)
    {
        SearchArchiveNodes();
    }

    public void SearchArchiveNodes()
    {
        ArchiveNodes.Clear();
        foreach (var node in _archiveNodeApplicationService.GetArchiveNodes())
        {
            BaseArchiveNodeViewModel archiveNode = new BaseArchiveNodeViewModel()
            {
                ID = node.Id,
                ServerAE = node.ServerAETitle,
                IP = node.IP,
                Port = node.Port,
                ClientAE = node.ClientAETitle,
                Remark = node.Remark,
                IsDefault = node.IsDefault,
            };
            ArchiveNodes.Add(archiveNode);
        }
        if (ArchiveNodes.Count > 0)
        {
            SelectedArchiveNode = ArchiveNodes[0];
        }
    }

    private void AddCommand()
    {
        var archiveInfo = new ArchiveInfo();
        archiveInfo.Id = Guid.NewGuid().ToString();
        archiveInfo.IsDefault = false;
        _archiveNodeApplicationService.SetArchiveNode(OperationType.Add, archiveInfo);
        ShowWindow();
    }

    private void EditCommand()
    {
        if (SelectedArchiveNode is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        ArchiveInfo archiveNode = new ArchiveInfo()
        {
            Id = SelectedArchiveNode.ID,
            ServerAETitle = SelectedArchiveNode.ServerAE,
            IP = SelectedArchiveNode.IP,
            Port = SelectedArchiveNode.Port,
            ClientAETitle = SelectedArchiveNode.ClientAE,
            Remark = SelectedArchiveNode.Remark,
            IsDefault = SelectedArchiveNode.IsDefault,
        };
        _archiveNodeApplicationService.SetArchiveNode(OperationType.Edit, archiveNode);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<ArchiveNodeWindow>();
        }
        if (_editWindow is not null)
        {
			//_editWindow.ShowWindowDialog();
			_editWindow.ShowPopWindowDialog();
		}
    }

    private void DeleteCommand()
    {
        if (SelectedArchiveNode is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (SelectedArchiveNode.IsDefault)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current node because there are  default! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the node? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _archiveNodeApplicationService.Delete(SelectedArchiveNode.ID))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete node({SelectedArchiveNode.ServerAE})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchArchiveNodes();
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}