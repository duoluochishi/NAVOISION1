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

public class WindowingListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IWindowingApplicationService _windowingApplicationService;
    private ILogger<WindowingListViewModel> _logger;
    private WindowingWindow? _editWindow;

    private ObservableCollection<BaseWindowingViewModel> _windowingList = new ObservableCollection<BaseWindowingViewModel>();

    public ObservableCollection<BaseWindowingViewModel> WindowingList
    {
        get => _windowingList;
        set => SetProperty(ref _windowingList, value);
    }

    private BaseWindowingViewModel _selectedWindowing = new BaseWindowingViewModel();
    public BaseWindowingViewModel SelectedWindowing
    {
        get => _selectedWindowing;
        set
        {
            if (SetProperty(ref _selectedWindowing, value) && value is not null)
            {
                IsFactory = !value.IsFactory;
            }
        }
    }

    private bool _isFactory = false;
    public bool IsFactory
    {
        get => _isFactory;
        set => SetProperty(ref _isFactory, value);
    }

    public WindowingListViewModel(IWindowingApplicationService windowingApplicationService,
        IDialogService dialogService,
        ILogger<WindowingListViewModel> logger)
    {
        _dialogService = dialogService;
        _windowingApplicationService = windowingApplicationService;
        _logger = logger;
        Commands.Add("EditCommand", new DelegateCommand(EditCommand));
        Commands.Add("AddCommand", new DelegateCommand(AddCommand));
        Commands.Add("DeleteCommand", new DelegateCommand(DeleteCommand));

        SearchWindowing();
        _windowingApplicationService.WindowingReload += WindowingApplicationService_WindowingReload;
    }

    [UIRoute]
    private void WindowingApplicationService_WindowingReload(object? sender, EventArgs e)
    {
        SearchWindowing();
    }

    public void SearchWindowing()
    {
        WindowingList.Clear();
        foreach (var node in _windowingApplicationService.GetWindowings())
        {
            BaseWindowingViewModel windowing = new BaseWindowingViewModel()
            {
                ID = node.Id,
                BodyPart = node.BodyPart,
                IsFactory = node.IsFactory,
                Description = node.Description,
                Shortcut = node.Shortcut,
            };
            if (node.Width is not null)
            {
                windowing.WindowWidth = node.Width.Value;
            }
            if (node.Level is not null)
            {
                windowing.WindowLevel = node.Level.Value;
            }
            WindowingList.Add(windowing);
        }
        if (WindowingList.Count > 0)
        {
            SelectedWindowing = WindowingList[0];
        }
    }

    private void AddCommand()
    {
        var windowing = new WindowingInfo();
        windowing.Id = Guid.NewGuid().ToString();
        windowing.IsFactory = false;
        _windowingApplicationService.SetWindowing(OperationType.Add, windowing);
        ShowWindow();
    }

    private void EditCommand()
    {
        if (SelectedWindowing is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a windowing from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        WindowingInfo windowing = new WindowingInfo()
        {
            Id = SelectedWindowing.ID,
            BodyPart = SelectedWindowing.BodyPart,
            IsFactory = SelectedWindowing.IsFactory,
            Description = SelectedWindowing.Description,
            Shortcut = SelectedWindowing.Shortcut,
        };
        windowing.Width = new ItemField<int>();
        windowing.Level = new ItemField<int>();
        windowing.Width.Value = SelectedWindowing.WindowWidth;
        windowing.Level.Value = SelectedWindowing.WindowLevel;
        _windowingApplicationService.SetWindowing(OperationType.Edit, windowing);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<WindowingWindow>();
        }
        if (_editWindow is not null)
        {
            //_editWindow.ShowWindowDialog();
            _editWindow.ShowPopWindowDialog();
        }
    }

    private void DeleteCommand()
    {
        if (SelectedWindowing is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a windowing from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (SelectedWindowing.IsFactory)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current windowing because there are factory! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the windowing? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _windowingApplicationService.Delete(SelectedWindowing.ID))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete windowing({SelectedWindowing.BodyPart})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchWindowing();
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}