namespace NV.CT.Recon.Model;

public class ReconRangeGroupModel : BaseViewModel
{
	/// <summary>
	/// 当前检查在这个检查列表里面的索引
	/// </summary>
	public int ScanIndex { get; set; }
	public string ImagePath { get; set; } = string.Empty;
	public string GroupName { get; set; } = string.Empty;
	public string ScanId { get; set; } = string.Empty;

	private ObservableCollection<ScanReconModel> _recon = new();
	public ObservableCollection<ScanReconModel> Recons
	{
		get => _recon;
		set => SetProperty(ref _recon, value);
	}

	private ScanReconModel? _selectedReconModel;
	public ScanReconModel? SelectedReconModel
	{
		get => _selectedReconModel;
		set
		{
			SetProperty(ref _selectedReconModel, value);

			if (value != null)
			{
				EventAggregator.Instance.GetEvent<SelectedReconRangeChangedEvent>()
					.Publish(value);
			}
		}
	}
}


public class PlanningBaseGroupModel : BaseViewModel
{
	public int ScanIndex { get; set; }
	public string ImagePath { get; set; } = string.Empty;
	public string GroupName { get; set; } = string.Empty;
	public string ScanId { get; set; } = string.Empty;

	private ObservableCollection<ScanReconModel> _recon = new();
	public ObservableCollection<ScanReconModel> Recons
	{
		get => _recon;
		set => SetProperty(ref _recon, value);
	}

	private ScanReconModel? _selectedReconModel;
	public ScanReconModel? SelectedReconModel
	{
		get => _selectedReconModel;
		set
		{
			SetProperty(ref _selectedReconModel, value);

			if (value != null)
			{
				EventAggregator.Instance.GetEvent<SelectedPlanningBaseChangedEvent>()
					.Publish(value);
			}
		}
	}
}
