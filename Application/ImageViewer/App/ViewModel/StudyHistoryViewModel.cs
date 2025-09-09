using NV.CT.ImageViewer.View;

namespace NV.CT.ImageViewer.ViewModel;

public class StudyHistoryViewModel : StudyViewModel
{
	private StudyFilterWindow? _studyFilterWindow;
	private readonly IMapper _mapper;
	private readonly IViewerService _viewerService;

	public StudyHistoryViewModel(IViewerService viewerService, IMapper mapper) : base(viewerService, mapper)
	{
		_viewerService = viewerService;
		_mapper = mapper;

		Commands.Add("FilterCommand", new DelegateCommand<object>(OnFiltrationWindowDialog));

		_viewerService.SelectHistoryStudyChanged += SelectHistoryStudyChanged;
	}

	private void SelectHistoryStudyChanged(object? sender, string studyId)
	{
		if (string.IsNullOrEmpty(studyId))
			return;

		GetVStudyModelsByStudyId(studyId);
	}

	public override bool NeedInit()
	{
		return false;
	}

	private void OnFiltrationWindowDialog(object element)
	{
		if (_studyFilterWindow is null)
		{
			_studyFilterWindow = new StudyFilterWindow
			{
				WindowStartupLocation = WindowStartupLocation.Manual
			};
			var button = element as Button;
			if (button != null)
			{
				var positionOfButton = button.PointFromScreen(new Point(0, 0));
				_studyFilterWindow.Left = Math.Abs(positionOfButton.X) - 1200;
				_studyFilterWindow.Top = Math.Abs(positionOfButton.Y) + 40;
			}
		}
		_studyFilterWindow.ShowWindowDialog(true);
	}


}