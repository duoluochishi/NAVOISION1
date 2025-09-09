using NV.CT.DatabaseService.Contract;
using NV.CT.JobService.Contract;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ImageViewer.ViewModel;

public class HistorySeriesViewModel : SeriesViewModel
{
	private readonly IViewerService _viewerService;

	public HistorySeriesViewModel(IViewerService viewerService, IMapper mapper, ILogger<SeriesViewModel> logger, IStudyService studyService, ISeriesService seriesService, IRawDataService rawDataService, IDialogService dialogService, IJobRequestService jobRequestService, IAuthorization authorization, IOfflineTaskService offlineService, IApplicationCommunicationService applicationCommunicationService, IPrint printService, IOfflineConnection offlineConnection) : base(viewerService, mapper, logger, studyService, seriesService, rawDataService, dialogService, jobRequestService, authorization, offlineService, applicationCommunicationService, printService, offlineConnection)
	{
		_viewerService = viewerService;

		_viewerService.SelectHistoryStudyChanged += SelectHistoryStudyChanged;

		_viewerService.ViewerChanged -= OnSeriesChanged;
	}

	private void SelectHistoryStudyChanged(object? sender, string studyId)
	{
		if (string.IsNullOrEmpty(studyId))
			return;

		GetSeriesModelsByStudyId(studyId);
	}

	public override bool NeedInit()
	{
		return false;
	}
}