using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.View;

public partial class TextInputWindow : Window
{
	private readonly Image2DViewModel _viewModel;
	private TextInputAction? CurrentAction;
	public TextInputWindow()
	{
		InitializeComponent();

		_viewModel = CTS.Global.ServiceProvider.GetRequiredService<Image2DViewModel>();
		DataContext = _viewModel;

		EventAggregator.Instance.GetEvent<TextInputEvent>().Subscribe(data =>
		{
			CurrentAction = data.Action;

			Application.Current?.Dispatcher?.Invoke(() =>
			{
				txtContent.Focus();
			});

			if (data.Action == TextInputAction.InputText)
			{
				txtContent.Text = string.Empty;
			}
			else if (data.Action == TextInputAction.ChangeText)
			{
				txtContent.Text = data.Text;
			}
		});
	}

	private void BtnOk_OnClick(object sender, RoutedEventArgs e)
	{
		var realContent = txtContent.Text.Trim();
		if (string.IsNullOrEmpty(realContent))
			return;

		if (CurrentAction == TextInputAction.InputText)
		{
			_viewModel.CurrentImageViewer.WriteTextInput(realContent);
		}
		else if (CurrentAction == TextInputAction.ChangeText)
		{
			_viewModel.CurrentImageViewer.WriteChangedText(realContent);
		}

		Hide();
	}

	private void BtnClose_OnClick(object sender, RoutedEventArgs e)
	{
		Hide();
	}
}