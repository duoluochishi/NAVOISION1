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

public class WorklistNodesViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IWorklistNodeApplicationService _worklistNodeApplicationService;
    private ILogger<WorklistNodesViewModel> _logger;
    private WorklistNodeWindow? _editWindow;

    private ObservableCollection<BaseWorklistNodeViewModel> _worklistNodes = new ObservableCollection<BaseWorklistNodeViewModel>();

    public ObservableCollection<BaseWorklistNodeViewModel> WorklistNodes
    {
        get => _worklistNodes;
        set => SetProperty(ref _worklistNodes, value);
    }

    private BaseWorklistNodeViewModel _selectedWorkNode = new BaseWorklistNodeViewModel();
    public BaseWorklistNodeViewModel SelectedWorkNode
    {
        get => _selectedWorkNode;
        set
        {
            if (SetProperty(ref _selectedWorkNode, value) && value is not null)
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

    public WorklistNodesViewModel(IWorklistNodeApplicationService worklistNodeApplicationService,
        IDialogService dialogService,
        ILogger<WorklistNodesViewModel> logger)
    {
        _dialogService = dialogService;
        _worklistNodeApplicationService = worklistNodeApplicationService;
        _logger = logger;
        Commands.Add("EditCommand", new DelegateCommand(EditCommand));
        Commands.Add("AddCommand", new DelegateCommand(AddCommand));
        Commands.Add("DeleteCommand", new DelegateCommand(DeleteCommand));

        SearchWorklistNodes();
        _worklistNodeApplicationService.WorklistReload += WorklistNodeApplicationService_WorklistReload;
    }

    [UIRoute]
    private void WorklistNodeApplicationService_WorklistReload(object? sender, EventArgs e)
    {
        SearchWorklistNodes();
    }

    public void SearchWorklistNodes()
    {
        WorklistNodes.Clear();
        foreach (var node in _worklistNodeApplicationService.GetWorklist())
        {
            BaseWorklistNodeViewModel worklistNode = new BaseWorklistNodeViewModel()
            {
                ID = node.Id,
                ServerName = node.Name,
                IP = node.IP,
                Port = node.Port,
                ServerAE = node.AETitle,
                Remark = node.Remark,
                IsDefault = node.IsDefault,
                EnableMPPS = node.IsMppsEnabled,
            };
            WorklistNodes.Add(worklistNode);
        }
        if (WorklistNodes.Count > 0)
        {
            SelectedWorkNode = WorklistNodes[0];
        }
    }

    private void AddCommand()
    {
        var worklistInfo = new WorklistInfo();
        worklistInfo.Id = Guid.NewGuid().ToString();
        worklistInfo.IsDefault = false;
        _worklistNodeApplicationService.SetWorklist(OperationType.Add, worklistInfo);
        ShowWindow();
    }

    private void EditCommand()
    {
        if (SelectedWorkNode is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        WorklistInfo worklistInfo = new WorklistInfo()
        {
            Id = SelectedWorkNode.ID,
            AETitle = SelectedWorkNode.ServerAE,
            IP = SelectedWorkNode.IP,
            Port = SelectedWorkNode.Port,
            Name = SelectedWorkNode.ServerName,
            Remark = SelectedWorkNode.Remark,
            IsDefault = SelectedWorkNode.IsDefault,
        };
        _worklistNodeApplicationService.SetWorklist(OperationType.Edit, worklistInfo);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<WorklistNodeWindow>();
        }
        if (_editWindow is not null)
        {
			//_editWindow.ShowWindowDialog();
			_editWindow.ShowPopWindowDialog();
		}
    }

    private void DeleteCommand()
    {
        if (SelectedWorkNode is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a worklist node from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (SelectedWorkNode.IsDefault)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current worklist node because there are  default! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the worklist node? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _worklistNodeApplicationService.Delete(SelectedWorkNode.ID))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete worklist node({SelectedWorkNode.ServerName})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchWorklistNodes();
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}