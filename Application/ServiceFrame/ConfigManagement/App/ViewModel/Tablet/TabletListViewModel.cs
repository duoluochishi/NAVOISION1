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
using NV.CT.ConfigManagement.Extensions;
using NV.CT.ConfigManagement.View;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class TabletListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ITabletApplicationService _tabletApplicationService;
    private ILogger<TabletListViewModel> _logger;
    private TabletWindow? _tabletWindow;

    private ObservableCollection<BaseTabletViewModel> _tabletList = new ObservableCollection<BaseTabletViewModel>();
    public ObservableCollection<BaseTabletViewModel> TabletList
    {
        get => _tabletList;
        set => SetProperty(ref _tabletList, value);
    }

    private BaseTabletViewModel _selectedTablet = new BaseTabletViewModel();
    public BaseTabletViewModel SelectedTablet
    {
        get => _selectedTablet;
        set => SetProperty(ref _selectedTablet, value);
    }

    public TabletListViewModel(ITabletApplicationService tabletApplicationService,
        IDialogService dialogService,
        ILogger<TabletListViewModel> logger)
    {
        _dialogService = dialogService;
        _tabletApplicationService = tabletApplicationService;
        _logger = logger;

        Commands.Add("TabletEditCommand", new DelegateCommand(TabletEditCommand));
        Commands.Add("TabletAddCommand", new DelegateCommand(TabletAddCommand));
        Commands.Add("TabletDeleteCommand", new DelegateCommand(TabletDeleteCommand));

        SearchTabletList(string.Empty);
        _tabletApplicationService.TabletListReload += TabletApplicationService_RoleListReload;
    }

    [UIRoute]
    private void TabletApplicationService_RoleListReload(object? sender, EventArgs e)
    {
        SearchTabletList(string.Empty);
    }

    public void SearchTabletList(string searchText)
    {
        TabletList.Clear();
        foreach (var tablet in _tabletApplicationService.GetAllTabletInfo())
        {
            BaseTabletViewModel tabletViewModel = new BaseTabletViewModel()
            {
                ID = tablet.Id,
                IP = tablet.IP,
                SN = tablet.SerialNumber,
            };
            TabletList.Add(tabletViewModel);
        }
        if (TabletList.Count > 0)
        {
            SelectedTablet = TabletList[0];
        }
    }

    private void TabletAddCommand()
    {
        var tablet = new TabletInfo();
        tablet.Id = Guid.NewGuid().ToString();
        _tabletApplicationService.SetTabletInfo(OperationType.Add, tablet);
        ShowTabletWindow();
    }

    private void TabletEditCommand()
    {
        //出厂角色不可编辑
        if (SelectedTablet is null || string.IsNullOrEmpty(SelectedTablet.IP))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a tablet from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        var tablet = new TabletInfo();
        tablet.Id = SelectedTablet.ID;
        tablet.IP = SelectedTablet.IP;
        tablet.SerialNumber = SelectedTablet.SN;

        _tabletApplicationService.SetTabletInfo(OperationType.Edit, tablet);
        ShowTabletWindow();
    }

    public void ShowTabletWindow()
    {
        if (_tabletWindow is null)
        {
            _tabletWindow = CTS.Global.ServiceProvider?.GetRequiredService<TabletWindow>();
        }
        if (_tabletWindow != null)
        {
			//_tabletWindow.ShowWindowDialog();
			_tabletWindow.ShowPopWindowDialog();			
		}
    }

    private void TabletDeleteCommand()
    {
        if (SelectedTablet is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a tablet from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the tablet? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _tabletApplicationService.Delete(SelectedTablet.ID))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete tablet({SelectedTablet.IP})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchTabletList(string.Empty);
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}