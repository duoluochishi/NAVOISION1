namespace NV.CT.Examination.ApplicationService.Contract.ScanControl;

/// <summary>
/// 控制状态服务接口
/// </summary>
public interface IUIControlStatusService
{
    bool IsGoButtonEnable { get; }
    bool IsCancelButtonEnable { get; }
    bool IsReconAllButtonEnable { get; }
    bool IsConfirmButtonEnable { get; }

    string GoFailReason { get; }
    string CancelFailReason { get; }
    string ReconAllFailReason { get; }
    string ConfirmFailReason { get; }

    event EventHandler? GoButtonStatusChanged;
    event EventHandler? CancelButtonStatusChanged;
    event EventHandler? ReconAllButtonStatusChanged;
    event EventHandler? ConfirmButtonStatusChanged;
}
