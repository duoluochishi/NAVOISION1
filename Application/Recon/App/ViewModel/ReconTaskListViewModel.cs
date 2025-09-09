//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Extensions;

namespace NV.CT.Recon.ViewModel;

public class ReconTaskListViewModel : TaskListViewModel
{
	private readonly ILogger<TaskListViewModel> _logger;
	private readonly IDialogService _dialogService;
	private readonly IDataSync _dataSync;
	private readonly IApplicationCommunicationService _appService;
	public ReconTaskListViewModel(ILogger<TaskListViewModel> logger,
		ISelectionManager scanSelectManager,
		IProtocolHostService protocolHostService,
		IStudyHostService studyHostService,
		IMeasurementStatusService measurementStatusService,
		ILayoutManager layoutManager,
		IWorkflow workflow,
		IDialogService dialogService,
		IUIRelatedStatusService uiRelatedStatusService,
		IApplicationCommunicationService applicationCommunicationService,
		IPageService pageService, 
		IDataSync dataSync) : base(logger, scanSelectManager, protocolHostService, studyHostService, measurementStatusService, layoutManager, workflow, dialogService, uiRelatedStatusService, applicationCommunicationService, pageService, dataSync)
	{
		_logger = logger;
		_dataSync = dataSync;
		_dialogService = dialogService;
		_appService = applicationCommunicationService;
	}

	public override void AfterTaskListCreated()
	{
		//离线重建 只显示已完成的检查,没有完成的不需要显示出来
		TaskList = TaskList.Where(n => n.ScanTaskStatus == PerformStatus.Performed).ToList().ToObservableCollection();
	}

	public override void ScanClose(int? processId)
	{
		if (processId is not null && Process.GetCurrentProcess().Id != processId)
		{
			return;
		}

		var appName = "Recon";

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			_dialogService.ShowDialog(true, MessageLeveles.Info, "Confirm"
				, "Are you sure you want to close the " + appName + "?", arg =>
				{
					if (arg.Result == ButtonResult.OK)
					{
						try
						{
							_logger.LogInformation($"recon close");
						}
						catch (Exception ex)
						{
							_logger.LogError("TaskListViewModel ScanClose Error { 0}", ex.Message);
						}
						finally
						{
							Process.GetCurrentProcess().Kill();
						}

					}
				}, ConsoleSystemHelper.WindowHwnd);
		});

	}

	public override void ProtocolHostService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		//重建不改变扫描任务状态
	}

	public override void SetScanPerforming()
	{
		//重建不改变扫描任务状态
	}
}