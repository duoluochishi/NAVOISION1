//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class SystemHomeViewModel : BaseViewModel
{
	private readonly ILayoutManager? _layoutManager;
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly IShutdownService _shutdownService;
	private readonly IWorkflow _workflow;
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	public SystemHomeViewModel(IConsoleApplicationService consoleAppService, IShutdownService shutdownService,IWorkflow workflow, IApplicationCommunicationService applicationCommunicationService)
	{
		_workflow = workflow;
		_shutdownService = shutdownService;
		_consoleAppService = consoleAppService;
		_applicationCommunicationService = applicationCommunicationService;

		_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();

		_workflow.LockScreenChanged += LockScreenChanged;
		_workflow.UnlockScreenChanged += UnlockScreenChanged;

		Commands.Add("PatientBrowser", new DelegateCommand(PatientBrowser));
		Commands.Add("JobViewer", new DelegateCommand(JobViewer));
		Commands.Add("Lock", new DelegateCommand(Lock));
		Commands.Add("Restart", new DelegateCommand(Restart));
		Commands.Add("Shutdown", new DelegateCommand(Shutdown));
		
	}

	private void UnlockScreenChanged(object? sender, string nextScreen)
	{
		_layoutManager?.Goto(nextScreen.IsEmpty() ? Screens.Home : Enum.Parse<Screens>(nextScreen));
	}

	private void LockScreenChanged(object? sender, EventArgs e)
	{
		_layoutManager?.Goto(Screens.LockScreen);
	}

	private void Lock()
	{
		_workflow.LockScreen();
	}

	private void Shutdown()
	{
		_layoutManager?.Goto(Screens.Shutdown);
	}

	private void Restart()
	{
		_shutdownService.Restart();
	}

	private void PatientBrowser()
	{
		_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER);

		//Global.Instance.StudyId = Global.Instance.WorkFlowClient.GetCurrentStudy(new Empty()).StudyId;

		var workflow = Program.ServiceProvider?.GetRequiredService<IWorkflow>();

		Global.Instance.StudyId = workflow.GetCurrentStudy();
	}

	private void JobViewer()
	{
		//在主控台这边玩是错的
		//_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_JOBVIEWER);

		//直接通知grpc去启进程,不要在主控台这边玩
		_applicationCommunicationService.Start(
			new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_JOBVIEWER));
	}
}