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
using NV.CT.AppService.Contract;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using NV.CT.Language;

namespace NV.CT.ConfigManagement.ViewModel;

public class MainViewModel : BaseViewModel
{
    private readonly IApplicationCommunicationService _applicationCommunicationService;
    private readonly IDialogService _dialogService;
    public MainViewModel(IApplicationCommunicationService applicationCommunicationService,
        IDialogService dialogService)
    {
        _applicationCommunicationService = applicationCommunicationService;
        _dialogService = dialogService;

        _applicationCommunicationService.NotifyApplicationClosing -= ApplicationCommunicationService_NotifyApplicationClosing;
        _applicationCommunicationService.NotifyApplicationClosing += ApplicationCommunicationService_NotifyApplicationClosing;       
    }

    [UIRoute]
    private void ApplicationCommunicationService_NotifyApplicationClosing(object? sender, ApplicationResponse e)
    {
        if (!(e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME &&
              e.Parameters == Global.Instance.ModelName))
            return;

        if(e.NeedConfirm)
        {
            _dialogService.ShowDialog(true, MessageLeveles.Info, "Confirm"
                , "Are you sure you want to close the " + e.Parameters + "?", arg =>
                {
                    if (arg.Result == ButtonResult.OK)
                    {
                        Process.GetProcessById(e.ProcessId).Kill();
                    }
                }, ConsoleSystemHelper.WindowHwnd);
        }
        else
        {
            Process.GetProcessById(e.ProcessId).Kill();
		}
    }
}