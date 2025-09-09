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
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.Language;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.ConfigManagement.ViewModel;

public class CoefficientViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IKvMaCoefficientApplicationService _kvMaCoefficientApplicationService;
    private readonly ILogger<CoefficientViewModel> _logger;
    private BaseCoefficientViewModel _current = new BaseCoefficientViewModel();
    public BaseCoefficientViewModel Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public CoefficientViewModel(IKvMaCoefficientApplicationService kvMaCoefficientApplicationService,
        IDialogService dialogService,
        ILogger<CoefficientViewModel> logger)
    {
        _dialogService = dialogService;
        _kvMaCoefficientApplicationService = kvMaCoefficientApplicationService;
        _logger = logger;

        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
        _kvMaCoefficientApplicationService.ChangedHandler += KvMaCoefficentApplicationService_ChangedHandler;
    }

    [UIRoute]
    private void KvMaCoefficentApplicationService_ChangedHandler(object? sender, EventArgs<(OperationType operation, CategoryCoefficientInfo categoryCoefficentInfo)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetInfo(e.Data.categoryCoefficentInfo);
    }

    private void SetInfo(CategoryCoefficientInfo categoryCoefficentInfo)
    {
        //Current = new BaseCoefficentViewModel();
        Current.KV = categoryCoefficentInfo.KV;
        Current.MA = categoryCoefficentInfo.MA;
        Current.Sources.Clear();
        foreach (var node in categoryCoefficentInfo.Sources)
        {
            Current.Sources.Add(new SourceViewModel
            {
                Id = node.Id,
                KVFactor = node.KVFactor,
                MAFactor = node.MAFactor,
            });
        }
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty())
        {
            return;
        }
        CategoryCoefficientInfo windowing = new CategoryCoefficientInfo()
        {
            KV = Current.KV,
            MA = Current.MA,
        };
        foreach (var node in Current.Sources)
        {
            windowing.Sources.Add(new SourceCoefficientInfo
            {
                Id = node.Id,
                KVFactor = node.KVFactor,
                MAFactor = node.MAFactor,
            });
        }
        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _kvMaCoefficientApplicationService.Add(windowing);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _kvMaCoefficientApplicationService.Update(windowing);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _kvMaCoefficientApplicationService.Reload();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be 0!";
        if (Current.KV <= 0)
        {
            sb.Append(string.Format(message, "KV"));
            flag = false;
        }
        if (Current.MA <= 0)
        {
            sb.Append(string.Format(message, "MA"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _kvMaCoefficientApplicationService.Reload();
            window.Hide();
        }
    }
}