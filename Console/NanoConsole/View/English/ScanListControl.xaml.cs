//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.View.English;

public partial class ScanListControl
{
	public ScanListControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<ScanListViewModel>();
	}

	private void GrdScanPatient_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		ScanStudyModel study = (ScanStudyModel)grdScanPatient.SelectedItem;
		if (study != null)
		{
			string hintText = "";
			string currentStudyId = "";
			IWorkflow workflow = CTS.Global.ServiceProvider.GetRequiredService<IWorkflow>();
			currentStudyId = workflow.GetCurrentStudy();
			if (!string.IsNullOrEmpty(currentStudyId) && study.StudyId != currentStudyId)
			{
				hintText = "Are you sure want to close the current study and start a new one ? ";
			}
			if (string.IsNullOrEmpty(currentStudyId))
			{
				hintText = "Are you sure to start a new study ? ";
			}
			IDialogService dialogService = CTS.Global.ServiceProvider.GetRequiredService<IDialogService>();
			dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm", hintText, arg =>
			{
				if (arg.Result == ButtonResult.OK)
				{
					ConsoleApplicationService consoleAppService = CTS.Global.ServiceProvider.GetRequiredService<ConsoleApplicationService>();

					//Todo剂量报告没有生成，不能关闭检查

					if (study.StudyId != currentStudyId)
					{
						if (!string.IsNullOrEmpty(currentStudyId))//关闭当前病人
						{
							//FuncCommon.GetDoseReport(Global.Instance.StudyId);

							workflow.CloseWorkflow();
							consoleAppService.CloseApp(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);
							currentStudyId = string.Empty;
						}
						if (!string.IsNullOrEmpty(study.StudyId))
						{
							//开启新的病人检查
							workflow?.StartWorkflow(study.StudyId);
							currentStudyId = workflow.GetCurrentStudy();
							Global.Instance.StudyId = currentStudyId;
							Thread.Sleep(100);

							consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);
						}
					}

				}
			}, ConsoleSystemHelper.WindowHwnd);
		}
	}


	#region handle ScanPanel

	private bool _isMouseOverButton;
	private bool _isMouseOverPopup;

	private void ScanPanel_OnMouseEnter(object sender, MouseEventArgs e)
	{
		_isMouseOverButton = true;
		PopupScan.IsOpen = true;
	}

	private void ScanPanel_OnMouseLeave(object sender, MouseEventArgs e)
	{
		_isMouseOverButton = false;
		SchedulePopupClose();
	}
	private void PopupScan_OnMouseEnter(object sender, MouseEventArgs e)
	{
		_isMouseOverPopup = true;
	}
	private void PopupScan_OnMouseLeave(object sender, MouseEventArgs e)
	{
		_isMouseOverPopup = false;
		SchedulePopupClose();
	}
	private async void SchedulePopupClose()
	{
		await Task.Delay(100); // 防止鼠标快速移入移出时抖动
		if (!_isMouseOverButton && !_isMouseOverPopup)
		{
			PopupScan.IsOpen = false;
		}
	}
	#endregion
}