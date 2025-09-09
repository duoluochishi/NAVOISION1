using System.Threading.Tasks;
using StudyModel = NV.CT.DatabaseService.Contract.Models.StudyModel;

namespace NV.CT.RGT.ViewModel;

public class PatientBrowserViewModel : BaseViewModel
{
	private readonly IDataSync _dataSyncService;
	private readonly ISelectionManager _selectionManager;

	private ObservableCollection<StudyViewModel> _studies = new();
	public ObservableCollection<StudyViewModel> Studies
	{
		get => _studies;
		set => SetProperty(ref _studies, value);
	}

	private StudyViewModel? _selectedStudy;

	public StudyViewModel? SelectedStudy
	{
		get => _selectedStudy;
		set
		{
			if (value == _selectedStudy)
				return;

			SetProperty(ref _selectedStudy, value);

			if (_selectedStudy is not null && _selectedStudy.PatientId is not null)
			{
				var target = _studyDic[_selectedStudy.PatientId];

				//将选择扫描事件发送到本地通知
				_selectionManager.SelectScan(target.Item1, target.Item2);

				//某一个平板触发选中Study事件,最终由mcs发送事件通知过来
				_dataSyncService.SelectStudy(_selectedStudy.StudyId);
			}
		}
	}

	private readonly Dictionary<string, (StudyModel, PatientModel)> _studyDic = new();

	public PatientBrowserViewModel(IDataSync dataSyncService, ILayoutManager layoutManager, ISelectionManager selectionManager)
	{
		_dataSyncService = dataSyncService;
		_selectionManager = selectionManager;

		_dataSyncService.WorkListChanged += WorkListChanged;
		_dataSyncService.ExamCloseFinished += DataSyncService_ExamCloseFinished;
		_dataSyncService.SelectStudyChanged += DataSync_SelectStudyChanged;

		RefreshPatientList();
	}

	/// <summary>
	/// 被动接收work list 数据变化
	/// </summary>
	private void WorkListChanged(object? sender, List<(PatientModel, StudyModel)> e)
	{
		ConstructWorkList(e);
	}

	/// <summary>
	/// 主动找mcs要数据
	/// </summary>
	//    [UIRoute]
	public void RefreshPatientList()
	{
		Task.Run(() =>
		{
			var result = _dataSyncService.GetPatientStudyList();

			ConstructWorkList(result);
		});
	}

	[UIRoute]
	private void ConstructWorkList(List<(PatientModel, StudyModel)> result)
	{
		Studies.Clear();
		_studyDic.Clear();
		foreach (var (patientModel, studyModel) in result)
		{
			FillWithModel(patientModel, studyModel);

			_studyDic.Add(patientModel.PatientId, (studyModel, patientModel));
		}

		Studies = Studies.OrderBy(n => n.CreateTime).ToList().ToObservableCollection();
	}

	/// <summary>
	/// 同步选中病人变化
	/// </summary>
	[UIRoute]
	private void DataSync_SelectStudyChanged(object? sender, string e)
	{
		var findStudy = Studies.FirstOrDefault(n => n.StudyId == e);
		if (findStudy is null)
			return;

		//一定要有当前选中值和参数之间进行比较,因为同一个事件会发来多次,多台平板的情况下
		if (e == SelectedStudy?.StudyId)
			return;

		SelectedStudy = findStudy;
	}

	private void DataSyncService_ExamCloseFinished(object? sender, EventArgs e)
	{
		//RefreshPatientList();
	}

	[UIRoute]
	private void FillWithModel(PatientModel patientModel, StudyModel studyModel)
	{
		var studyViewModel = new StudyViewModel
		{
			StudyId = studyModel.Id,
			StudyDate = patientModel.CreateTime,
			PatientName = patientModel.PatientName,
			PatientId = patientModel.PatientId,
			Birthday = studyModel.Birthday,
			Sex = patientModel.PatientSex.ToString(),
			Age = studyModel.Age,
			CreateTime = patientModel.CreateTime
		};

		Studies.Add(studyViewModel);
	}


}