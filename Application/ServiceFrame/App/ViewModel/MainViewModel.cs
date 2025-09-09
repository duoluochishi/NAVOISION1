using Microsoft.Extensions.Logging;
using NV.CT.AppService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.ServiceFrame.ApplicationService.Contract.Interfaces;
using NV.CT.ServiceFramework.Contract;
using NV.CT.ServiceFramework.Model;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NV.CT.ServiceFrame.ViewModel;

public class MainViewModel : UI.ViewModel.BaseViewModel
{
	private readonly IServiceAppControlManager _serviceAppControlManager;
	private readonly IDialogService _dialogService;
	//private readonly IApplicationCommunicationService _applicationCommunicationService;
	private readonly ILogger<MainViewModel> _logger;

	private object _uiContent = null;
	public object UiContent
	{
		get => _uiContent;
		set => SetProperty(ref _uiContent, value);
	}

	public MainViewModel(IServiceAppControlManager serviceAppControlManager, IDialogService dialogService,
		ILogger<MainViewModel> logger
		//IApplicationCommunicationService applicationCommunicationService,
		)
	{
		_logger = logger;
		_serviceAppControlManager = serviceAppControlManager;
		_dialogService = dialogService;
		//_applicationCommunicationService = applicationCommunicationService;
		_serviceAppControlManager.ApplicationClosing += ServiceAppControlManager_ApplicationClosing;
		_serviceAppControlManager.OnChildrenSet += ServiceAppControlManager_OnChildrenSet;

		ComponentService.ComponentDataExchanged += ComponentService_ComponentDataExchanged;
	}

	private void ComponentService_ComponentDataExchanged(object? sender, System.Collections.Generic.List<ComponentExchange> e)
	{
		_logger.LogDebug($"ComponentService_ComponentDataExchanged with data: {e.ToJson()}");
	}

	private void ServiceAppControlManager_ApplicationClosing(object? sender, ApplicationResponse e)
	{
		if (!(e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME &&
			  e.Parameters == Global.Instance.ModelName))
			return;
		string message = string.Empty;
		if (!IsClosing(ref message))
		{
			_dialogService.ShowDialog(false, MessageLeveles.Info, "Info"
					  , "There are still tasks outstanding: " + message + "!", arg =>
					  { }, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		if (e.NeedConfirm)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(() =>
			{
				_dialogService.ShowDialog(true, MessageLeveles.Info, "Confirm"
						, "Are you sure you want to close the " + e.Parameters + "?", arg =>
						{
							if (arg.Result == ButtonResult.OK)
							{
								Process.GetProcessById(e.ProcessId).Kill();
							}
						}, ConsoleSystemHelper.WindowHwnd);
			});
		}
		else
		{
			Process.GetProcessById(e.ProcessId).Kill();
		}
	}

	private void ServiceAppControlManager_OnChildrenSet(object? sender, object e)
	{
		UiContent = e;
	}

	private bool IsClosing(ref string message)
	{
		var types = AppDomain.CurrentDomain.GetAssemblies()
					 .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IServiceControl))))
					 .ToArray();

		StringBuilder stringBuilder = new StringBuilder();
		foreach (var v in types)
		{
			if (v is IServiceControl service && !string.IsNullOrEmpty(service.GetTipOnClosing()))
			{
				stringBuilder.AppendLine(service.GetServiceAppID() + "," + service.GetServiceAppName() + "," + service.GetTipOnClosing());
			}
		}
		bool flag = true;
		if (stringBuilder.Length > 0)
		{
			flag = false;
			message = stringBuilder.ToString();
		}
		return flag;
	}
}