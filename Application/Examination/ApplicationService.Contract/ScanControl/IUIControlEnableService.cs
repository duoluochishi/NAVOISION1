namespace NV.CT.Examination.ApplicationService.Contract.ScanControl;

public interface IUIControlEnableService
{
    void RegisterRule(IUIControlEnableRule rule);
    bool IsEnabled();
    string GetFirstFailReason();

    event EventHandler<(bool, string)>? UIStatusChanged;
}