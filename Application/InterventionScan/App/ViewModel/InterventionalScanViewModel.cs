//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.AppService.Contract;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using NV.CT.Language;

namespace NV.CT.InterventionScan.ViewModel;

public class InterventionScanViewModel : BaseViewModel
{
    private readonly IApplicationCommunicationService _applicationCommunicationService;
    private readonly IDialogService _dialogService;
    public InterventionScanViewModel(IApplicationCommunicationService applicationCommunicationService,
        IDialogService dialogService)
    {
        _applicationCommunicationService = applicationCommunicationService;
        _dialogService = dialogService;
        _applicationCommunicationService.NotifyApplicationClosing -= ApplicationCommunicationService_NotifyApplicationClosing;
        _applicationCommunicationService.NotifyApplicationClosing += ApplicationCommunicationService_NotifyApplicationClosing;
        _applicationCommunicationService.ApplicationStatusChanged -= ApplicationCommunicationService_ApplicationStatusChanged;
        _applicationCommunicationService.ApplicationStatusChanged += ApplicationCommunicationService_ApplicationStatusChanged;
    }

    [UIRoute]
    private void ApplicationCommunicationService_ApplicationStatusChanged(object? sender, ApplicationResponse e)
    {
        //检查进程关闭则介入扫描进程同步关闭?
        if (e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION && e.Status == ProcessStatus.Closed)
        {
            //Process.GetCurrentProcess().Kill();
        }
    }

    [UIRoute]
    private void ApplicationCommunicationService_NotifyApplicationClosing(object? sender, ApplicationResponse e)
    {
        if (!(e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_INTERVENTIONSCAN &&
              Process.GetCurrentProcess().Id == e.ProcessId))
            return;

        if(e.NeedConfirm)
        {
            _dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, LanguageResource.Message_Info_CloseMessage, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    Process.GetCurrentProcess().Kill();
                }
            }, ConsoleSystemHelper.WindowHwnd);
        }
        else
        {
            Process.GetCurrentProcess().Kill();
		}
    }
}