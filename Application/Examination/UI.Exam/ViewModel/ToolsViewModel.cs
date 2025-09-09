//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//----------------------------------------------------------------------

using NV.CT.Protocol.Models;

namespace NV.CT.UI.Exam.ViewModel;

public class ToolsViewModel : BaseViewModel
{
    private string _lastCommandStr = string.Empty;
    private readonly IImageOperationService _imageOperationService;
    private readonly ISelectionManager _selectionManager;
    private readonly IProtocolHostService _protocolHostService;
    private bool _isReverse;
    public bool IsReverse
    {
        get => _isReverse;
        set => SetProperty(ref _isReverse, value);
    }

    private bool _isHUShow;
    public bool IsHUShow
    {
        get => _isHUShow;
        set => SetProperty(ref _isHUShow, value);
    }

    private bool _isSelected = true;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isSwitchViewsEnabled = false;
    public bool IsSwitchViewsEnabled
    {
        get => _isSwitchViewsEnabled;
        set => SetProperty(ref _isSwitchViewsEnabled, value);
    }

    private bool _isSetCenterPosition;
    public bool IsSetCenterPosition
    {
        get => _isSetCenterPosition;
        set => SetProperty(ref _isSetCenterPosition, value);
    }

    private bool _isSetCenterPositionEnabled = false;
    public bool IsSetCenterPositionEnabled
    {
        get => _isSetCenterPositionEnabled;
        set => SetProperty(ref _isSetCenterPositionEnabled, value);
    }

    private ScanModel? CurrentScanModel { get; set; }

    public ToolsViewModel(IImageOperationService imageOperationService,
        ISelectionManager selectionManager,
        IProtocolHostService protocolHostService)
    {
        _imageOperationService = imageOperationService;
        _selectionManager = selectionManager;
        _protocolHostService = protocolHostService;
        Commands.Add(CommandParameters.COMMAND_TOOLS, new DelegateCommand<string>(ToolsCommand));
        Commands.Add(CommandParameters.COMMAND_SWITCH_VIEWS, new DelegateCommand(SwitchViews));
        Commands.Add("SetCenterPosition", new DelegateCommand(SetCenterPosition));

        _selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
        _selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;

        _protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
        _protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;

        _protocolHostService.ParameterChanged -= ProtocolHostService_ParameterChanged;
        _protocolHostService.ParameterChanged += ProtocolHostService_ParameterChanged;
    }

    private void ProtocolHostService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
    {
        if (e is null || e.Data.baseModel is null || e.Data.list is null)
        {
            return;
        }
        if (e.Data.baseModel is ScanModel scanModel
            && CurrentScanModel is not null
            && scanModel.Descriptor.Id == CurrentScanModel.Descriptor.Id
            && scanModel.Status == PerformStatus.Unperform
            && e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_OPTION)) is not null)
        {
            if (scanModel.ScanOption == FacadeProxy.Common.Enums.ScanOption.DualScout)
            {
                IsSwitchViewsEnabled = true;
            }
            else
            {
                IsSwitchViewsEnabled = false;
            }
        }
    }

    private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
    {
        if (e is null)
        {
            return;
        }
        IsSetCenterPositionEnabled = true;
        // IsSetCenterPositionEnabled = _protocolHostService.Models.Any(t => t.Scan.IsIntervention);
    }

    private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        CurrentScanModel = e.Data;
        if (e.Data is ScanModel scan && (scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.Surview || scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.DualScout))
        {
            if (scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.DualScout)
            {
                IsSwitchViewsEnabled = true;
            }
            else
            {
                IsSwitchViewsEnabled = false;
            }
        }
    }

    public void ToolsCommand(string commandStr)
    {
        if (!(!string.IsNullOrEmpty(commandStr) && !_lastCommandStr.Equals(commandStr)))
        {
            return;
        }

        _imageOperationService.DoToolsBarCommand(commandStr);
        _lastCommandStr = commandStr;
        if (CommandParameters.IMAGE_OPERATE_REVERSE.Equals(commandStr))
        {
            _lastCommandStr = "rrrcdd";  //这就是记录一个乱码，没有实际意义
        }
        if (CommandParameters.IMAGE_OPERATE_REWORK == commandStr)
        {
            IsSelected = true;
            _imageOperationService.DoToolsBarCommand(CommandParameters.IMAGE_OPERATE_SELECT);
        }
        if (CommandParameters.IMAGE_OPERATE_HU.Equals(commandStr))
        {
            IsHUShow = true;
        }
        if (!CommandParameters.IMAGE_OPERATE_HU.Equals(commandStr) && IsHUShow)
        {
            IsHUShow = false;
            _imageOperationService.DoToolsBarCommand(CommandParameters.IMAGE_OPERATE_HU);
        }

        if (CommandParameters.IMAGE_OPERATE_REVERSE.Equals(commandStr))
        {
            IsReverse = false;
            _imageOperationService.SetInverted();

            IsSelected = true;
            _imageOperationService.DoToolsBarCommand(CommandParameters.IMAGE_OPERATE_SELECT);
        }
    }

    private void SwitchViews()
    {
        _imageOperationService.SwitchViews();
    }

    public void SetCenterPosition()
    {
        IsSetCenterPosition = true;
        _imageOperationService.SetCurrentToCenterPositon();
    }
}