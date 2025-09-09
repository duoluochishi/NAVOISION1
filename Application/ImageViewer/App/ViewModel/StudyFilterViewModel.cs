using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.ImageViewer.Model;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ImageViewer.ViewModel;

public class StudyFilterViewModel : BaseViewModel
{
	private readonly IViewerService _viewerService;
    private readonly IViewer _viewer;
    private readonly ILogger<StudyFilterViewModel> _logger;
	public StudyFilterViewModel(IViewerService viewerService, IViewer viewer, ILogger<StudyFilterViewModel> logger)
	{
		_logger = logger;
		_viewerService = viewerService;
        _viewer= viewer;

        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
		Commands.Add("SearchCommand", new DelegateCommand(QueryStudyListByFilter));
		Commands.Add("ConfirmCommand",new DelegateCommand<object>(ConfirmCommand,_=>true));

		BodyParts = EnumHelper.GetAllItems<BodyPart>().Select(n=>n.ToString()).ToList().ToObservableCollection();
	}

	public void ConfirmCommand(object parameter)
	{
		if (SelectedItem is null)
		{
			_errorMsg = "Please choose a study and then confirm!";
			return;
		}

		_errorMsg = string.Empty;
		_viewerService.SelectHistoryStudy(SelectedItem.StudyId);
        _viewer.StartViewer(SelectedItem.StudyId);

        if (parameter is Window window)
		{
			window.Hide();
		}
	}

	/// <summary>
	/// 根据Filter配置获取Study列表记录
	/// </summary>
	public void QueryStudyListByFilter()
	{
		try
		{
			var queryModel = new StudyQueryModel()
			{
				BodyPart = BodyPart,
				PatientId = PatientId,
				StartDate = StartDate,
				EndDate = EndDate,
			};
			var result = _viewerService.GetPatientStudyListByFilterSimple(queryModel);

			var list = result.Select(r =>
			{
				var firstName = string.Empty;
				string lastName;
				if (r.Item1.PatientName.Contains("^"))
				{
					string[] arr = r.Item1.PatientName.Split('^');
					firstName = arr[0];
					lastName = arr[1];
				}
				else
				{
					lastName = r.Item1.PatientName;
				}

				return new VStudyModel
				{
					PatientName = r.Item1.PatientName,
					BodyPart = r.Item2.BodyPart,
					StudyId = r.Item2.Id,
					PatientId = r.Item1.PatientId,
					LastName = lastName,
					FirstName = firstName,
					PatientSex = r.Item1.PatientSex,
					ExamStartTime = r.Item2.ExamStartTime,
					ExamEndTime = r.Item2.ExamEndTime,
					CreateTime = r.Item2.CreateTime,
					Birthday = r.Item2.Birthday,
					StudyStatus = r.Item2.StudyStatus,
					PatientType = r.Item2.PatientType
				};
			}).ToList(); 
			VStudies =list.ToObservableCollection();
			//SelectedItem = _gVStudies.FirstOrDefault();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Failed in QueryStudyListByFilterConfig with error : {ex.Message}");
		}
	}

	private ObservableCollection<VStudyModel> _vStudies=new ();
	public ObservableCollection<VStudyModel> VStudies
	{
		get => _vStudies;
		set => SetProperty(ref _vStudies, value);
	}

	private VStudyModel? _selectedItem;
	public VStudyModel? SelectedItem
	{
		get => _selectedItem;
		set => SetProperty(ref _selectedItem, value);
	}

	private string _patientId = string.Empty;
	public string PatientId
	{
		get => _patientId;
		set => SetProperty(ref _patientId, value);
	}

	private string _bodyPart = string.Empty;
	public string BodyPart
	{
		get => _bodyPart;
		set => SetProperty(ref _bodyPart, value);
	}

	private DateTime? _startDate;
	public DateTime? StartDate
	{
		get => _startDate;
		set => SetProperty(ref _startDate, value);
	}

	private DateTime? _endDate;
	public DateTime? EndDate
	{
		get => _endDate;
		set => SetProperty(ref _endDate, value);
	}

	private ObservableCollection<string> _bodyParts = new();
	public ObservableCollection<string> BodyParts
	{
		get => _bodyParts;
		set => SetProperty(ref _bodyParts, value);
	}

	private string _errorMsg = string.Empty;
	public string ErrorMsg
	{
		get => _errorMsg;
		set => SetProperty(ref _errorMsg, value);
	}

	public void Closed(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}

}