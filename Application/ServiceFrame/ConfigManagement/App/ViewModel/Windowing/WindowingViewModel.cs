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
using NV.CT.ConfigManagement.ApplicationService.Impl;

namespace NV.CT.ConfigManagement.ViewModel;

public class WindowingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IWindowingApplicationService _windowingApplicationService;
    private readonly ILogger<WindowingViewModel> _logger;
    private BaseWindowingViewModel _currentWindowing = new BaseWindowingViewModel();
    public BaseWindowingViewModel CurrentWindowing
    {
        get => _currentWindowing;
        set => SetProperty(ref _currentWindowing, value);
    }

    private ObservableCollection<KeyValuePair<string, string>> _bodyPartlist = new ObservableCollection<KeyValuePair<string, string>>();
    public ObservableCollection<KeyValuePair<string, string>> BodyPartList
    {
        get => _bodyPartlist;
        set => SetProperty(ref _bodyPartlist, value);
    }

    private bool _isEdit = false;
    public bool IsEdit
    {
        get => _isEdit;
        set => SetProperty(ref _isEdit, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public WindowingViewModel(IWindowingApplicationService windowingApplicationService,
        IDialogService dialogService,
        ILogger<WindowingViewModel> logger)
    {
        _dialogService = dialogService;
        _windowingApplicationService = windowingApplicationService;
        _logger = logger;
        InitBodyPartList();
        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
        _windowingApplicationService.WindowingChanged += WindowingApplicationService_WindowingChanged;
    }

    [UIRoute]
    private void WindowingApplicationService_WindowingChanged(object? sender, EventArgs<(OperationType operation, WindowingInfo windowingInfo)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetWindowingInfo(e.Data.windowingInfo);
        if (OperationType == OperationType.Edit)
        {
            IsEdit = false;
        }
        if (OperationType == OperationType.Add)
        {
            IsEdit = true;
            CurrentWindowing.BodyPart = BodyPartList[0].Value;
        }
    }

    private void SetWindowingInfo(WindowingInfo windowing)
    {
        CurrentWindowing = new BaseWindowingViewModel();
        CurrentWindowing.ID = windowing.Id;
        CurrentWindowing.BodyPart = windowing.BodyPart;
        CurrentWindowing.WindowWidth = 0;
        if (windowing.Width is not null)
        {
            CurrentWindowing.WindowWidth = windowing.Width.Value;
        }
        CurrentWindowing.WindowLevel = 0;
        if (windowing.Level is not null)
        {
            CurrentWindowing.WindowLevel = windowing.Level.Value;
        }
        CurrentWindowing.IsFactory = windowing.IsFactory;
        CurrentWindowing.Description = windowing.Description;
        CurrentWindowing.Shortcut = windowing.Shortcut;
    }

    private void InitBodyPartList()
    {
        foreach (var enumItem in Enum.GetValues(typeof(WindowLevelCenter)))
        {
            if (enumItem is not null)
            {
                BodyPartList.Add(new KeyValuePair<string, string>(enumItem.ToString(), enumItem.ToString()));
            }
        }
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (CheckAccountRepeat() || !CheckFormEmpty())
        {
            return;
        }
        WindowingInfo windowing = new WindowingInfo()
        {
            Id = CurrentWindowing.ID,
            BodyPart = CurrentWindowing.BodyPart,
            IsFactory = CurrentWindowing.IsFactory,
            Description = CurrentWindowing.Description,
            Shortcut = CurrentWindowing.Shortcut,
        };
        windowing.Width = new ItemField<int>();
        windowing.Level = new ItemField<int>();
        windowing.Width.Value = CurrentWindowing.WindowWidth;
        windowing.Level.Value = CurrentWindowing.WindowLevel;

        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _windowingApplicationService.Add(windowing);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _windowingApplicationService.Update(windowing);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _windowingApplicationService.ReloadWindowing();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentWindowing.BodyPart))
        {
            sb.Append(string.Format(message, "Bodypart"));
            flag = false;
        }
        if (string.IsNullOrEmpty(CurrentWindowing.Shortcut))
        {
            sb.Append(string.Format(message, "Shortcut"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }


    private bool CheckAccountRepeat()
    {
        bool flag = false;
        var currentWindowings = _windowingApplicationService.GetWindowings().FindAll(t => !t.BodyPart.Equals(WindowLevelCenter.Custom.ToString())).ToList();
        switch (OperationType)
        {
            case OperationType.Add:
                flag = currentWindowings.Any(t => t.BodyPart == CurrentWindowing.BodyPart);
                break;
            case OperationType.Edit:
                flag = currentWindowings.Any(t => t.Id != CurrentWindowing.ID && t.BodyPart == CurrentWindowing.BodyPart);
                break;
            default: break;
        }
        if (flag)
        {
            var message = "The BodyPart is duplicated!";
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _windowingApplicationService.ReloadWindowing();
            window.Hide();
        }
    }
}