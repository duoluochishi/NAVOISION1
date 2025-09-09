//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class GantryViewModel : BaseViewModel
{
	private readonly ILogger<GantryViewModel> _logger;
	private readonly ITablePositionService _tablePositionService;

	public GantryViewModel(ILogger<GantryViewModel> logger, ITablePositionService tablePositionService)
	{
		_logger = logger;
		_tablePositionService = tablePositionService;

		_tablePositionService.GantryPositionChanged += TablePositionService_GantryPositionChanged;

        UpdateGantryPositionInitially();
	}

	private void TablePositionService_GantryPositionChanged(object? sender, EventArgs<GantryPositionInfo> e)
	{
		if (e.Data is null)
			return;

		//_logger.LogInformation($"gantry position changed to {e.Data.Position}");
		Application.Current?.Dispatcher.BeginInvoke(() =>
		{
			GantryPosition = Math.Round((double)e.Data.Position / 100, 1);
		});
	}

	private void UpdateGantryPositionInitially()
	{
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            GantryPosition = Math.Round((double)_tablePositionService.CurrentGantryPosition.Position / 100, 1);
        });
    }

    private double gantryPosition;
	private double gantryPalstance;

	public double GantryPosition
	{
		get => gantryPosition;
		set => SetProperty(ref gantryPosition, value);
	}

	public double GantryPalstance
	{
		get => gantryPalstance;
		set => SetProperty(ref gantryPalstance, value);
	}
}