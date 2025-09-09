//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.CommonAttributeUI.AOPAttribute;

namespace NV.CT.Examination.ViewModel;

public class ScanDefaultViewMode : BaseViewModel
{
    private readonly ISelectionManager _selectionManager;
    private ParameterDetailWindow? _parameterDetailWindow;
    private TimeDensityWindow? _timeDensityWindow;
    private bool IsUIChange = false;

    private bool _isTimeDensityShowEnable = false;
    public bool IsTimeDensityShowEnable
    {
        get => _isTimeDensityShowEnable;
        set
        {
            SetProperty(ref _isTimeDensityShowEnable, value);
        }
    }

    public ScanDefaultViewMode(ISelectionManager selectionManager)
    {
        Commands.Add("ShowParameterDetail", new DelegateCommand(ShowParameterDetail, () => true));
        Commands.Add("ShowTimeDensityWindow", new DelegateCommand(ShowTimeDensityWindow, () => true));

        _selectionManager = selectionManager;
        _selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;
        _selectionManager.SelectionScanChanged += SelectionScanChanged;
    }

    [UIRoute]
    private void SelectionScanChanged(object? sender, CTS.EventArgs<ScanModel> e)
    {
        IsUIChange = false;
        IsShowScanPara = true;
        IsUIChange = true;

        if (e is not null
            && e.Data is not null
            && (e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.NVTestBolus
            || e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.NVTestBolusBase
            || e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.TestBolus
            || e.Data.ScanOption == FacadeProxy.Common.Enums.ScanOption.BolusTracking))
        {
            IsTimeDensityShowEnable = true;
        }
        else
        {
            IsTimeDensityShowEnable = false;
        }
    }

    [UIRoute]
    private void SelectionManager_SelectionReconChanged(object? sender, CTS.EventArgs<ReconModel> e)
    {
        IsUIChange = false;
        IsShowScanPara = false;
        IsUIChange = true;
    }

    /// <summary>
    /// 显示参数详细弹窗，扫描参数和重建参数
    /// </summary>
    public void ShowParameterDetail()
    {
        var parameterDetailViewModel = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailViewModel>();
        if (parameterDetailViewModel is not null)
        {
            parameterDetailViewModel.IsShowScan = IsShowScanPara;
        }

        if (_parameterDetailWindow is null)
        {
            _parameterDetailWindow = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailWindow>();
        }
        WindowDialogShow.DialogShow(_parameterDetailWindow);
    }

    private bool _isShowScanPara = true;
    public bool IsShowScanPara
    {
        get => _isShowScanPara;
        set
        {
            if (SetProperty(ref _isShowScanPara, value) && !IsUIChange)
            {
                if (value)
                {
                    ActiveParameterPanelIndex = 0;
                }
                else
                {
                    ActiveParameterPanelIndex = 1;
                }
            }
        }
    }

    private int _activeParameterPanelIndex;
    public int ActiveParameterPanelIndex
    {
        get => _activeParameterPanelIndex;
        set
        {
            SetProperty(ref _activeParameterPanelIndex, value);
            if (value == 0)
            {
                //scan
                var currentScan = _selectionManager.CurrentSelection.Scan;

                if (currentScan is not null && currentScan.Descriptor.Id is not null)
                {
                    var (frame, measurement, scan) = _selectionManager.CurrentSelection;
                    _selectionManager.SelectScan(frame.Descriptor.Id, measurement.Descriptor.Id, scan.Descriptor.Id);
                }
            }
            else if (value == 1)
            {
                //recon
                var currentRecon = _selectionManager.CurrentSelectionRecon;
                if (currentRecon is null)
                {
                    currentRecon = _selectionManager.CurrentSelection.Scan?.Children.FirstOrDefault();
                }

                if (currentRecon is not null)
                {
                    _selectionManager.SelectRecon(currentRecon);
                }
            }

            if (IsUIChange)
            {
                var parameterDetailViewModel = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailViewModel>();
                if (parameterDetailViewModel is not null)
                {
                    parameterDetailViewModel.IsShowScan = (value == 0);
                }
                IsShowScanPara = (value == 0);
            }
        }
    }

    public void ShowTimeDensityWindow()
    {
        if (_timeDensityWindow is null)
        {
            _timeDensityWindow = CTS.Global.ServiceProvider?.GetRequiredService<TimeDensityWindow>();
        }
        if (_timeDensityWindow is not null)
        {          
            WindowDialogShow.DialogShow(_timeDensityWindow,false);
        }
    }
}