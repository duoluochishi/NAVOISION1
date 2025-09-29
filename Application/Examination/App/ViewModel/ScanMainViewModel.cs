//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Language;

namespace NV.CT.Examination.ViewModel;

public class ScanMainViewModel : BaseViewModel, IDisposable
{
    private readonly ISelectionManager _selectionManager;
    private readonly IUIRelatedStatusService _uiRelatedStatusService;

    private bool _scanMainShow;
    public bool ScanMainShow
    {
        get => _scanMainShow;
        set => SetProperty(ref _scanMainShow, value);
    }

    private bool _reconMainShow = true;
    public bool SelectProtocolMainShow
    {
        get => _reconMainShow;
        set => SetProperty(ref _reconMainShow, value);
    }

    private bool _isScanMainShow;
    public bool IsScanMainShow
    {
        get => _isScanMainShow;
        set
        {
            if (SetProperty(ref _isScanMainShow, value))
            {
                var vm = CTS.Global.ServiceProvider?.GetRequiredService<ScanControlsViewModel>();

                if (value)
                {
                    //扫描主界面
                    ScanMainShow = true;
                    SelectProtocolMainShow = false;
                }
                else
                {
                    // 跳转到协议选择页面
                    ScanMainShow = false;
                    SelectProtocolMainShow = true;
                }
            }
        }
    }

    private bool _popShow = false;

    public bool InfoPopShow
    {
        get => _popShow;
        set => SetProperty(ref _popShow, value);
    }

    private bool _scanParameterShow = true;
    public bool ScanParameterShow
    {
        get => _scanParameterShow;
        set => SetProperty(ref _scanParameterShow, value);
    }

    private bool _reconParameterShow;

    public bool ReconParameterShow
    {
        get => _reconParameterShow;
        set => SetProperty(ref _reconParameterShow, value);
    }

    private bool _isShowScanPara = true;
    public bool IsShowScanPara
    {
        get => _isShowScanPara;
        set
        {
            if (SetProperty(ref _isShowScanPara, value))
            {
                if (value)
                {
                    ScanParameterShow = true;
                    ReconParameterShow = false;
                }
                else
                {
                    ScanParameterShow = false;
                    ReconParameterShow = true;
                }
            }
        }
    }

    public ScanMainViewModel(ISelectionManager selectionManager, IUIRelatedStatusService uiRelatedStatusService)
    {
        Commands.Add("ShowParameterDetail", new DelegateCommand<object>(ShowParameterDetail, _ => true));
        _selectionManager = selectionManager;
        _selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;


        try
        {
            //2025.09.18 增加Pop居中提示控制
            _uiRelatedStatusService = uiRelatedStatusService;
            
            _uiRelatedStatusService.RealtimeStatusChanged -= ExamStatusChanged;
            _uiRelatedStatusService.RealtimeStatusChanged += ExamStatusChanged;
            _uiRelatedStatusService.EmergencyStopped += UIRelatedStatusService_EmergencyStopped;
            _uiRelatedStatusService.ErrorStopped += UIRelatedStatusService_ErrorStopped;
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }

    private void SelectionManager_SelectionReconChanged(object? sender, CTS.EventArgs<ReconModel> e)
    {
        IsShowScanPara = false;
    }

    public void ShowParameterDetail(object parameter)
    {
        if (CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailViewModel>() is ParameterDetailViewModel parameterDetailViewModel)
        {
            parameterDetailViewModel.IsShowScan = IsShowScanPara;
        }
    }

    private ObservableCollection<int> _scanTaskList = new ObservableCollection<int>() { 1, 2, };
    public ObservableCollection<int> ScanTaskList
    {
        get => _scanTaskList;
        set => SetProperty(ref _scanTaskList, value);
    }
    [UIRoute]
    private void ExamStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
    {
        var realtimeInfo = e.Data;
        if (realtimeInfo is null) return;
        switch (realtimeInfo.Status)
        {
            case RealtimeStatus.None:
            case RealtimeStatus.Init:
            case RealtimeStatus.Standby:
            case RealtimeStatus.ExposureFinished:
            case RealtimeStatus.NormalScanStopped:
            case RealtimeStatus.EmergencyScanStopped:
            case RealtimeStatus.ExposureStarted:
            case RealtimeStatus.Error:
                InfoPopShow = false;
                break;
            case RealtimeStatus.MovingPartEnable:
            case RealtimeStatus.ExposureEnable:
            case RealtimeStatus.ExposureSpoting:
            case RealtimeStatus.ExposureSpotingIdle:
                InfoPopShow = true;
                break;
            default:
                InfoPopShow = false;
                break;
        }
    }
    [UIRoute]
    private void UIRelatedStatusService_ErrorStopped(object? sender, EventArgs<RealtimeInfo> e)
    {
        InfoPopShow = false;
    }

    [UIRoute]
    private void UIRelatedStatusService_EmergencyStopped(object? sender, EventArgs<RealtimeInfo> e)
    {
        InfoPopShow = false;
    }
    public void Dispose()
    {
        // 显式解除事件订阅，并确保弹窗关闭
        if (_uiRelatedStatusService != null)
        {
            _uiRelatedStatusService.RealtimeStatusChanged -= ExamStatusChanged;
            _uiRelatedStatusService.EmergencyStopped -= UIRelatedStatusService_EmergencyStopped;
            _uiRelatedStatusService.ErrorStopped -= UIRelatedStatusService_ErrorStopped;
        }

        // 强制关闭所有弹窗
        InfoPopShow = false;
    }
}