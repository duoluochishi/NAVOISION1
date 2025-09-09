namespace NV.CT.RGT.ViewModel;

public class HeaderViewModel : BaseViewModel
{
	private const double DOUBLE_DIVIDER = 1000.0;
	private readonly IDataSync _dataSync;

	private PatientModel? _patientModel;
	public PatientModel? PatientModel
	{
		get => _patientModel;
		set => SetProperty(ref _patientModel, value);
	}

	private StudyModel? _studyModel;
	public StudyModel? StudyModel
	{
		get => _studyModel;
		set => SetProperty(ref _studyModel, value);
	}

	private double? _bmi = 0;

	// ReSharper disable once InconsistentNaming
	public double? BMI
	{
		get => _bmi;
		set => SetProperty(ref _bmi, value);
	}

	private double _horizontalPosition;
	public double HorizontalPosition
	{
		get => _horizontalPosition;
		set => SetProperty(ref _horizontalPosition, value);
	}

	private double _verticalPosition;
	public double VerticalPosition
	{
		get => _verticalPosition;
		set => SetProperty(ref _verticalPosition, value);
	}

	public HeaderViewModel( IDataSync dataSync)
	{
		_dataSync = dataSync;

		UpdateTablePosition();

		_dataSync.TablePositionChanged += DataSync_TablePositionChanged;
		_dataSync.ExamCloseFinished += DataSync_ExamCloseFinished;
		_dataSync.NormalExamFinished += DataSync_NormalExamFinished;
	}

	private void UpdateTablePosition()
	{
		var tablePositionInfo = _dataSync.CurrentTablePosition();
		if (tablePositionInfo == null)
		{
			return;
		}

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			HorizontalPosition = OneDot(tablePositionInfo.HorizontalPosition / DOUBLE_DIVIDER);
			VerticalPosition = OneDot(tablePositionInfo.VerticalPosition / DOUBLE_DIVIDER);
		});
	}

	/// <summary>
	/// 正常检查启动完成
	/// </summary>
	private void DataSync_NormalExamFinished(object? sender, EventArgs e)
	{
		var (studyInfo, patientInfo) = _dataSync.GetCurrentStudyInfo();

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			StudyModel = studyInfo;
			PatientModel = patientInfo;

			UpdateIndicator();
		});
	}

	private void DataSync_ExamCloseFinished(object? sender, EventArgs e)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			BMI = 0;
		});
	}

	private void DataSync_TablePositionChanged(object? sender, EventArgs<CTS.Models.TablePositionInfo> e)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			HorizontalPosition = OneDot(e.Data.HorizontalPosition / DOUBLE_DIVIDER);
			VerticalPosition = OneDot(e.Data.VerticalPosition / DOUBLE_DIVIDER);
		});
	}

	private void UpdateIndicator()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			var height = StudyModel?.PatientSize;
			var weight = StudyModel?.PatientWeight;
			if (height is not null && weight is not null)
			{
				var formatHeight = height / 100.0;
				BMI = weight / (formatHeight * formatHeight);
			}
			else
			{
				BMI = 0;
			}
		});
	}

	private double OneDot(double val)
	{
		return Math.Truncate(val * 10) / 10;
	}
}