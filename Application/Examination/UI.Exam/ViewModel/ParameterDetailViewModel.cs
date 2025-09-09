//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.UI.Exam.DynamicParameters.Interfaces;
namespace NV.CT.UI.Exam.ViewModel;

public class ParameterDetailViewModel : BaseViewModel
{
    private IDynamicTemplateService _dynamicTemplateService;
    private IAuthorization _authorization;
    private string _detailTitle = LanguageResource.Content_ScanParametersWindowTitle;

    public string DetailTitle
    {
        get => _detailTitle;
        set => SetProperty(ref _detailTitle, value);
    }

    private bool _scanDetailShow = true;
    public bool ScanDetailShow
    {
        get => _scanDetailShow;
        set => SetProperty(ref _scanDetailShow, value);
    }

    private bool _reconDetailShow;
    public bool ReconDetailShow
    {
        get => _reconDetailShow;
        set => SetProperty(ref _reconDetailShow, value);
    }

    private bool _isShowScan = true;
    public bool IsShowScan
    {
        get => _isShowScan;
        set
        {
            if (SetProperty(ref _isShowScan, value))
            {
                if (value)
                {
                    ScanDetailShow = true;
                    ReconDetailShow = false;

                    ActiveParameterPanelIndex = 0;
                    DetailTitle = LanguageResource.Content_ScanParametersWindowTitle;
                }
                else
                {
                    ScanDetailShow = false;
                    ReconDetailShow = true;

                    ActiveParameterPanelIndex = 1;
                    DetailTitle = LanguageResource.Content_ReconParametersWindowTitle;
                }
            }
            IsReconAdvancedChecked = false;
        }
    }

    private bool _isReconAdvancedBottonShow = false;
    public bool IsReconAdvancedButtonShow
    {
        get => _isReconAdvancedBottonShow;
        set => SetProperty(ref _isReconAdvancedBottonShow, value & IsDevelopment);
    }

    private bool _isReconAdvancedChecked = false;
    public bool IsReconAdvancedChecked
    {
        get => _isReconAdvancedChecked;
        set
        {
            if (SetProperty(ref _isReconAdvancedChecked, value) && value)
            {
                _dynamicTemplateService.SetTemplate(DynamicTemplates.AdvancedParamterDetail);
            }
            else
            {
                _dynamicTemplateService.SetTemplate(DynamicTemplates.SourceParamterDetail);
            }
        }
    }

    public ParameterDetailViewModel(ISelectionManager selectionManager,
        IDynamicTemplateService dynamicTemplateService,
        IAuthorization authorization)
    {
        _dynamicTemplateService = dynamicTemplateService;
        _authorization = authorization;

        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Close, _ => true));

        selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
        selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;

        selectionManager.SelectionReconChanged -= SelectionManager_SelectionReconChanged;
        selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;

        _authorization.CurrentUserChanged -= Authorization_CurrentUserChanged;
        _authorization.CurrentUserChanged += Authorization_CurrentUserChanged;

        InitIsReconAdvancedButton(_authorization.GetCurrentUser());
    }

    [UIRoute]
    private void Authorization_CurrentUserChanged(object? sender, Models.UserModel? e)
    {
        InitIsReconAdvancedButton(e);
    }

    private void InitIsReconAdvancedButton(Models.UserModel? userModel)
    {
        if (userModel is null || userModel.RoleList is null)
        {
            IsReconAdvancedButtonShow = false;
            return;
        }
        if (userModel.RoleList.Any(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(SystemPermissionNames.ROLENAME_ADMINISTRATOR))
            || userModel.RoleList.Any(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(SystemPermissionNames.ROLENAME_SERVICEENGINEER))
            || userModel.RoleList.Any(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(SystemPermissionNames.ROLENAME_DEVICEMANAGER))
            || userModel.RoleList.Any(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(SystemPermissionNames.ROLENAME_SENIOR)))
        {
            IsReconAdvancedButtonShow = IsDevelopment & true;
        }
        else
        {
            IsReconAdvancedButtonShow = false;
        }
    }

    [UIRoute]
    private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
    {
        IsShowScan = false;
    }

    [UIRoute]
    private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        IsShowScan = true;
    }

    private int _activeParameterPanelIndex;
    public int ActiveParameterPanelIndex
    {
        get => _activeParameterPanelIndex;
        set => SetProperty(ref _activeParameterPanelIndex, value);
    }

    public void Close(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}