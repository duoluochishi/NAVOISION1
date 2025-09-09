using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;

namespace NV.CT.NanoConsole.ViewModel;

public class CalibrationTemplateViewModel : BaseViewModel
{
	public CalibrationTemplateViewModel()
	{

		FakeList();
	}

	private void FakeList()
	{
		CalibrationList.Add(new CalibrationInfo()
		{
			Name = "AfterGlowCal",
			CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			ValidDays = 7,
			State = SelfCheckStatus.Success,
		});
		CalibrationList.Add(new CalibrationInfo()
		{
			Name = "AirCalTwoSpot",
			CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			ValidDays = 1,
			State = SelfCheckStatus.Error,
		});
		CalibrationList.Add(new CalibrationInfo()
		{
			Name = "NonLinearCal",
			CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
			ValidDays = 3,
			State = SelfCheckStatus.Success,
		});
	}

	private ObservableCollection<CalibrationInfo> _calibrationList = new();
	public ObservableCollection<CalibrationInfo> CalibrationList
	{
		get => _calibrationList;
		set => SetProperty(ref _calibrationList, value);
	}
}

public class CalibrationInfo
{
	public string Name { get; set; } = string.Empty;
	public string CreateTime { get; set; } = string.Empty;
	public string CurrentTime { get; set; } = string.Empty;
	public int ValidDays { get; set; }
	public SelfCheckStatus State { get; set; }
	public string StatusString => State.ToString();

}
