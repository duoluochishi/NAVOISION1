namespace NV.CT.Examination.ApplicationService.Contract.ScanControl;

public interface IUIControlEnableRule
{
    bool IsEnabled();
    string GetFailReason();

    event EventHandler? UIStatusChanged;
}