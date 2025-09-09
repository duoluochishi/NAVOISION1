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
using System.Collections.Generic;
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class CoefficientsViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IKvMaCoefficientApplicationService _kvMaCoefficientApplicationService;
    private ILogger<CoefficientsViewModel> _logger;
    private KvMaCoefficientWindow? _editWindow;

    private ObservableCollection<BaseCoefficientViewModel> _nodes = new ObservableCollection<BaseCoefficientViewModel>();

    public ObservableCollection<BaseCoefficientViewModel> Nodes
    {
        get => _nodes;
        set => SetProperty(ref _nodes, value);
    }

    private BaseCoefficientViewModel _selectedNode = new BaseCoefficientViewModel();
    public BaseCoefficientViewModel SelectedNode
    {
        get => _selectedNode;
        set
        {
            SetProperty(ref _selectedNode, value);
        }
    }

    private string lastSearchText = string.Empty;
    public string LastSearchText
    {
        get => lastSearchText;
        set => SetProperty(ref lastSearchText, value);
    }

    public CoefficientsViewModel(IKvMaCoefficientApplicationService kvMaCoefficientApplicationService,
        IDialogService dialogService,
        ILogger<CoefficientsViewModel> logger)
    {
        _dialogService = dialogService;
        _kvMaCoefficientApplicationService = kvMaCoefficientApplicationService;
        _logger = logger;
        Commands.Add("SearchCommand", new DelegateCommand<string>(SearchNodes));
        Commands.Add("EditCommand", new DelegateCommand(EditCommand));
        Commands.Add("AddCommand", new DelegateCommand(AddCommand));
        Commands.Add("DeleteCommand", new DelegateCommand(DeleteCommand));

        SearchNodes(LastSearchText);
        _kvMaCoefficientApplicationService.ReloadHandler += KvMaCoefficientApplicationService_ReloadHandler;
    }

    [UIRoute]
    private void KvMaCoefficientApplicationService_ReloadHandler(object? sender, EventArgs e)
    {
        SearchNodes(LastSearchText);
    }

    public void SearchNodes(string searchText)
    {
        LastSearchText = searchText;
        Nodes.Clear();
        List<CategoryCoefficientInfo> li = _kvMaCoefficientApplicationService.Get();
        var list = li.ToObservableCollection();
        if (!string.IsNullOrEmpty(searchText))
        {
            list = li.FindAll(t => t.KV.ToString().Contains(searchText.ToLower()) || t.MA.ToString().Contains(searchText.ToLower())).ToObservableCollection();
        }
        foreach (var node in list)
        {
            BaseCoefficientViewModel cnode = new BaseCoefficientViewModel()
            {
                MA = node.MA,
                KV = node.KV,
            };
            foreach (var model in node.Sources)
            {
                SourceViewModel sourceViewModel = new SourceViewModel()
                {
                    Id = model.Id,
                    KVFactor = model.KVFactor,
                    MAFactor = model.MAFactor,
                };
                cnode.Sources.Add(sourceViewModel);
            }
            Nodes.Add(cnode);
        }
        if (Nodes.Count > 0)
        {
            SelectedNode = Nodes[0];
        }
    }

    private void AddCommand()
    {
        var info = new CategoryCoefficientInfo();
        info.Sources = new List<SourceCoefficientInfo>();
        for (int i = 1; i <= 24; i++)    //默认24型号
        {
            info.Sources.Add(new SourceCoefficientInfo
            {
                Id = i,
                KVFactor = 10000,
                MAFactor = 10000,
            });
        }
        _kvMaCoefficientApplicationService.Set(OperationType.Add, info);
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
        CategoryCoefficientInfo info = new CategoryCoefficientInfo()
        {
            KV = SelectedNode.KV,
            MA = SelectedNode.MA,
        };
        info.Sources = new List<SourceCoefficientInfo>();
        foreach (var model in SelectedNode.Sources)
        {
            info.Sources.Add(new SourceCoefficientInfo
            {
                Id = model.Id,
                KVFactor = model.KVFactor,
                MAFactor = model.MAFactor,
            });
        }
        _kvMaCoefficientApplicationService.Set(OperationType.Edit, info);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<KvMaCoefficientWindow>();
        }
        if (_editWindow is not null)
        {
            // _editWindow.ShowWindowDialog();
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

        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the node? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _kvMaCoefficientApplicationService.Delete(SelectedNode.KV, SelectedNode.MA))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete the node({SelectedNode.KV})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchNodes(LastSearchText);
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}