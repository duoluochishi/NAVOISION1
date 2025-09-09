//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;
using NV.CT.UI.Controls.Extensions;
using NV.MPS.Configuration;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;
using Label = System.Windows.Controls.Label;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace NV.CT.ImageViewer.View;

public partial class Control2D
{
	private List<TextBlock> textBlocks = new();
	private readonly Image2DViewModel _vm;
	private readonly ILogger<Control2D>? _logger;
	private CustomWWWLWindow? _customWwwlWindow;
    public Control2D()
	{
		InitializeComponent();

		_vm = CTS.Global.ServiceProvider.GetRequiredService<Image2DViewModel>();
		DataContext = _vm;
		_logger = CTS.Global.ServiceProvider.GetService<ILogger<Control2D>>();

		//禁用右键菜单
		labelLayout.ContextMenu = null;
		labelWwWl.ContextMenu = null;
		labelRotate.ContextMenu = null;
		labelKernel.ContextMenu = null;
		labelGrid.ContextMenu = null;
		labelSynchronization.ContextMenu = null;

		menuLayout.DataContext = DataContext;
		menuWwWl.DataContext = DataContext;
		menuRotate.DataContext = DataContext;
		menuKernel.DataContext = DataContext;
		menuGrid.DataContext = DataContext;
		menuSynchronization.DataContext = DataContext;

		//TextBlock点击事件与对应的菜单关联上
		txtLayout.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelLayout_OnMouseLeftButtonDown), true);
		//txtWWWL.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelWwWl_OnMouseLeftButtonDown), true);
		//txtRotate.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelRotate_OnMouseLeftButtonDown), true);
		txtKernel.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelKernel_OnMouseLeftButtonDown), true);
		txtGrid.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelGrid_OnMouseLeftButtonDown), true);
		//txtSynchronization.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelSynchronization_OnMouseLeftButtonDown), true);
		tbCine.MouseLeftButtonDown +=tbCine_MouseLeftButtonDown;
        Loaded += _2D_Loaded;
		EventAggregator.Instance.GetEvent<UpdateUnStateButtonEvent>().Subscribe(UpdateUnStateButtonHighLight);
        EventAggregator.Instance.GetEvent<Update2DStateButtonEvent>().Subscribe(UpdateStateButtonHighLight);
        EventAggregator.Instance.GetEvent<Update2DPreviousStateButtonEvent>().Subscribe(updatePreviousFocusedTextBlock);
        EventAggregator.Instance.GetEvent<Update2DSwitchButtonEvent>().Subscribe(UpdateSwitchButtonHighLight);
        EventAggregator.Instance.GetEvent<Update2DHotKeyEvent>().Subscribe(UpdateSwitchButtonState);
        EventAggregator.Instance.GetEvent<FilmPlayChangedEvent>().Subscribe(filmControlStatus =>
		{
			if (filmControlStatus)
			{
				//disable all other control's states

				var textblocks = grdIcon.FindVisualChild<TextBlock>();
				foreach (var childControl in textblocks)
				{
					childControl.IsEnabled = false;
				}

				var labels = grdIcon.FindVisualChild<Label>();
				foreach (var childControl in labels)
				{
					childControl.IsEnabled = false;
				}
			}
			else
			{
				//restore all other control's states

				var textblocks = grdIcon.FindVisualChild<TextBlock>();
				foreach (var childControl in textblocks)
				{
					childControl.IsEnabled = true;
				}

				var labels = grdIcon.FindVisualChild<Label>();
				foreach (var childControl in labels)
				{
					childControl.IsEnabled = true;
				}
			}
		});

	}
    private void UpdateSwitchButtonHighLight(string name)
    {
        TextBlock textBlock = (TextBlock)this.FindName(name);
        if (textBlock is not null)
        {
            var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
            var newStr = str.Replace("0.png", "1.png");
            ChangeBackgroundImage(newStr, textBlock);
        }
    }
    private void UpdateUnStateButtonHighLight(string name)
	{
		TextBlock textBlock = (TextBlock)this.FindName(name);
		if (textBlock is not null)
		{
			var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
			var newStr = str.Replace("1.png", "0.png");
			ChangeBackgroundImage(newStr, textBlock);
		}
	}
    private void UpdateStateButtonHighLight(StateButton statebutton)
	{
        TextBlock textBlock = (TextBlock)this.FindName(statebutton.ButtonName);
        if (textBlock is not null)
        {
            var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
			string? newStr;
            if (statebutton.ButtonState)
			{              
                 newStr = str.Replace("1.png", "0.png");               
            }
            else
			{
                 newStr = str.Replace("0.png", "1.png");
            }
            ChangeBackgroundImage(newStr, textBlock);
        }
    }
    private void FillWWWL()
	{
		var windowTypes = UserConfig.WindowingConfig.Windowings;
		if (windowTypes != null)
		{
			foreach (var windowType in windowTypes)
			{
				var menuItem = new MenuItem();
				menuItem.Header = $"{windowType.BodyPart} ({windowType.Shortcut})";
				menuItem.Tag = windowType;
				menuItem.Click += WWWL_MenuItem_Click;
				menuItem.Style = FindResource("BackendMenuItem") as Style;
				menuWwWl.Items.Add(menuItem);
				var separator = new Separator();
				separator.Style = FindResource("BackendSeparator") as Style;
				menuWwWl.Items.Add(separator);
			}
		}
	}
	private void tbCine_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
	{	
        Point screenPoint = tbCine.PointToScreen(e.GetPosition(this));
        Point cinePoint = tbCine.PointToScreen(new Point(0, 0));
        FollowingPoints followingPoints=new FollowingPoints();
		followingPoints.ScreenPoint = screenPoint;
		followingPoints.CinePoint = cinePoint;
        EventAggregator.Instance.GetEvent<MousePositionEvent>().Publish(followingPoints);		
    }

    private void WWWL_MenuItem_Click(object sender, RoutedEventArgs e)
	{

	}

	private void _2D_Loaded(object sender, RoutedEventArgs e)
	{
		textBlocks = Global.FindVisualChild<TextBlock>(grdIcon);

		//var total = textBlocks.Count;
		//var images = textBlocks.Where(n => n.Tag != null).Select(n => ((ImageBrush)n.Background).ImageSource.ToString()).ToList();
		//var emptyList = textBlocks.Except(textBlocks.Where(n => n.Tag != null)).ToList();

		foreach (var item in textBlocks)
		{
			//踢掉两个 Label标签，View和Mark文本标签
			if (item.Tag is not null)
			{
				item.MouseLeftButtonDown += SwitchSelfAndHasFocusBackgroundImage;

				////mark标记
				//if (item.Tag.ToString() == "1")
				//{
				//    item.MouseLeftButtonDown += SwitchSelfBackgroundImage;
				//}

				////view操作标记
				//else
				//{
				//    item.MouseLeftButtonDown += SwitchSelfAndHasFocusBackgroundImage;
				//}
			}

			//item.MouseEnter += Item_MouseEnter;
			//item.MouseLeave += Item_MouseLeave;
		}
	}

	private void Item_MouseLeave(object sender, MouseEventArgs _)
	{
		var textBlock = (TextBlock)sender;
		var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
		var newStr = str.Replace("1.png", "0.png");
		ChangeBackgroundImage(newStr, textBlock);
	}

	private void Item_MouseEnter(object sender, MouseEventArgs _)
	{
		var textBlock = (TextBlock)sender;
		if (textBlock.Background is null)
			return;

		var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
		var newStr = str.Replace("0.png", "1.png");
		ChangeBackgroundImage(newStr, textBlock);
	}
	private void updatePreviousFocusedTextBlock(bool bo)
	{
		if (!bo)
		{
			return;
		}
		var previousFocusedTextBlocks = textBlocks.Where(r => r.Background != null && r.Tag != null && ((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png"));
		if (previousFocusedTextBlocks is null)
			return;
		foreach (var item in previousFocusedTextBlocks)
		{
            var otherStr = ((ImageBrush)item.Background).ImageSource.ToString();
            if (otherStr.Contains("1.png"))
            {
                var newOtherStr = otherStr.Replace("1.png", "0.png");
                ChangeBackgroundImage(newOtherStr, item);
            }
        }
	}
	private void UpdateSwitchButtonState(string name)
	{
		Dispatcher.Invoke(() => {
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = txtWWWL
            };
            txtWWWL.RaiseEvent(args);
        });
        UpdateSwitchButtonHighLight(name);	
    }
	private void SwitchSelfAndHasFocusBackgroundImage(object sender, MouseButtonEventArgs e)
	{
		var textBlock = (TextBlock)sender;
		//var tag = textBlock.Tag;
		//if (tag is null)
		//    return;
		if (ConstName2D.stateButtonsNameList.Contains(textBlock.Name) || ConstName2D.functionalButtonsNameList.Contains(textBlock.Name))
		{
			return;
		}
		var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
		if (str.Contains("0.png"))
		{
			if (textBlock.Name == "txtSynchronization")
			{
				return;
			}
			//将自己未被点击状态改为点击状态
			var newStr = str.Replace("0.png", "1.png");
			ChangeBackgroundImage(newStr, textBlock);

			//之前的被选中的 textBlock，去掉属于这个tag组里面的选中状态
			var previousFocusedTextBlock = textBlocks.FirstOrDefault(r => r.Background != null && r.Tag != null &&
			((ImageBrush)r.Background).ImageSource.ToString().Contains("1.png") && r != textBlock 
			&& !ConstName2D.stateButtonsNameList.Contains(r.Name) && !ConstName2D.functionalButtonsNameList.Contains(r.Name));
			//if (previousFocusedTextBlock is null || ConstName2D.stateButtonsNameList.Contains(previousFocusedTextBlock.Name) || ConstName2D.functionalButtonsNameList.Contains(previousFocusedTextBlock.Name))
			//    return;
			if (previousFocusedTextBlock is null)
			{
				return;
			}
            var otherStr = ((ImageBrush)previousFocusedTextBlock.Background).ImageSource.ToString();
            if (otherStr.Contains("1.png"))
            {
                var newOtherStr = otherStr.Replace("1.png", "0.png");
                ChangeBackgroundImage(newOtherStr, previousFocusedTextBlock);
            }
        }
		else
		{
			////需要toggle的控件
			//var toggleBackgroundList = new List<string>()
			//{
			//	"txtSynchronization"
			//};
			//if (toggleBackgroundList.Contains(textBlock.Name))
			//{
			//	ToggleBackground(textBlock);
			//}
		}
	}

	private void SwitchSelfBackgroundImage(object sender, MouseButtonEventArgs _)
	{
		var textBlock = (TextBlock)sender;
		ToggleBackground(textBlock);
	}

	private void ToggleBackground(TextBlock textBlock)
	{
		var str = ((ImageBrush)textBlock.Background).ImageSource.ToString();
		string newStr;
		if (str.Contains("0.png"))
		{
			newStr = str.Replace("0.png", "1.png");
		}
		else
		{
			newStr = str.Replace("1.png", "0.png");
		}

		ChangeBackgroundImage(newStr, textBlock);
	}

	private void ChangeBackgroundImage(string str, TextBlock textBlock)
	{
		var imageSourceConverter = new ImageSourceConverter();
		var imgSource = imageSourceConverter.ConvertFromString(str);
		if (imgSource != null)
		{
			((ImageBrush)textBlock.Background).ImageSource = (ImageSource)imgSource;
		}
	}

	private void LabelLayout_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuLayout.IsOpen = true;
	}

	private void LabelWwWl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuWwWl.IsOpen = true;
	}

	private void LabelRotate_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuRotate.IsOpen = true;
	}

	private void LabelKernel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuKernel.IsOpen = true;
	}

	private void LabelGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuGrid.IsOpen = true;
	}

	private void LabelSynchronization_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuSynchronization.IsOpen = true;
	}


	/// <summary>
	/// WWWL设置事件
	/// </summary>
	private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		var tag = (sender as TextBlock)?.Tag;
		if (tag is null)
			return;

		var windowType = tag as WindowingInfo;
		if (windowType is null)
			return;

		try
		{
			if (windowType.BodyPart == "Custom")
			{
				//Custom情况下，自定义输入值
				CommonMethod.ShowCustomWWWLWindow(ViewScene.View2D);
			}
			else
			{
				_vm.CurrentImageViewer.SetWWWL(windowType.Width.Value, windowType.Level.Value);
			}		
            updatePreviousFocusedTextBlock(true);
            UpdateSwitchButtonHighLight(ConstName2D.txtWWWL);
        }
		catch (Exception ex)
		{
			_logger?.LogError($"parse ww/wl error {ex.Message}");
		}
	}


	public void ShowCustomWWWLWindow()
	{
		_customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

		if (_customWwwlWindow is null)
		{
			_customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
		}

		if (_customWwwlWindow != null)
		{
			_customWwwlWindow.SetScene(ViewScene.View2D);
			WindowDialogShow.Show(_customWwwlWindow);
		}
	}
}