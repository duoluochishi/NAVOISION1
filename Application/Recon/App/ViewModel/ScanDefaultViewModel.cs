//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Recon.ViewModel;

public class ScanDefaultViewModel : BaseViewModel
{
	private readonly ISelectionManager _selectionManager;
	private readonly ILayoutManager _layoutManager;
	private ParameterDetailWindow? _parameterDetailWindow;
	public ScanDefaultViewModel(ISelectionManager selectionManager, ILayoutManager layoutManager)
	{
		Commands.Add("ShowParameterDetail", new DelegateCommand<object>(ShowParameterDetail, _ => true));

		_selectionManager = selectionManager;
		_layoutManager = layoutManager;
		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;
		_selectionManager.SelectionScanChanged += SelectionScanChanged;
	}

	private void SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		IsShowScanPara = true;
		if (_layoutManager.CurrentLayout != ScanTaskAvailableLayout.ScanDefault)
		{
			_layoutManager.SwitchToView(ScanTaskAvailableLayout.ScanDefault);
		}
	}

	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
		IsShowScanPara = false;
	}

	/// <summary>
	/// 显示参数详细弹窗，扫描参数和重建参数
	/// </summary>
	public void ShowParameterDetail(object parameter)
	{
		var parameterDetailViewModel = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailViewModel>();
		if (parameterDetailViewModel != null)
		{
			parameterDetailViewModel.IsShowScan = IsShowScanPara;
		}
		//同一个进程内部高级重建的参数详情页面可以弹出无限多个
		if (_parameterDetailWindow is null)
		{
			_parameterDetailWindow = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailWindow>();
		}
		//不要在上面定义，因为这个ViewModel会预加载，会导致Xaml找不到资源而引发Exam启动不起来
		//ParameterDetailWindow _parameterDetailWindow = new ParameterDetailWindow();

		var wih = new WindowInteropHelper(_parameterDetailWindow);
		if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
		{
			if (wih.Owner == IntPtr.Zero)
				wih.Owner = ConsoleSystemHelper.WindowHwnd;

			if (!_parameterDetailWindow.IsVisible)
			{
				//隐藏底部状态栏
				_parameterDetailWindow.Topmost = true;
				_parameterDetailWindow.ShowDialog();
			}
		}
		else
		{
			if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
			{
				wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			}

			if (!_parameterDetailWindow.IsVisible)
			{
				_parameterDetailWindow.Show();
				_parameterDetailWindow.Activate();
			}
		}
	}

	private bool _isShowScanPara = true;
	public bool IsShowScanPara
	{
		get => _isShowScanPara;
		set
		{
			if (SetProperty(ref _isShowScanPara, value))
			{
				if (value)
				{
					ActiveParameterPanelIndex = 0;
				}
				else
				{
					ActiveParameterPanelIndex = 1;
				}
			}
		}
	}

	private int _activeParameterPanelIndex;
	public int ActiveParameterPanelIndex
	{
		get => _activeParameterPanelIndex;
		set
		{
			SetProperty(ref _activeParameterPanelIndex, value);
			if (value == 0)
			{
				//scan
				var currentScan = _selectionManager.CurrentSelection.Scan;

				if (currentScan is not null && currentScan.Descriptor.Id is not null)
				{
					var (frame, measurement, scan) = _selectionManager.CurrentSelection;
					_selectionManager.SelectScan(frame.Descriptor.Id, measurement.Descriptor.Id, scan.Descriptor.Id);
				}
			}
			else if (value == 1)
			{
				//recon
				var currentRecon = _selectionManager.CurrentSelectionRecon;
				if (currentRecon == null)
				{
					currentRecon = _selectionManager.CurrentSelection.Scan.Children.FirstOrDefault();
				}

				if (currentRecon != null)
				{
					_selectionManager.SelectRecon(currentRecon);
				}
			}
		}
	}
}