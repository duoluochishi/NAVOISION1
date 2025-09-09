//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.UI.Controls.Controls;

public partial class CustomWWWLWindow
{
	//private readonly ReconControlViewModel? _reconControlViewModel;
	private Action<double, double> _okCallbackAction;
	public CustomWWWLWindow()
	{
		InitializeComponent();

		WindowStartupLocation = WindowStartupLocation.CenterScreen;
		MouseDown += (_, _) =>
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		};

		//_reconControlViewModel = CTS.Global.ServiceProvider.GetService<ReconControlViewModel>();
	}

	public void SetDefaultValue(double ww,double wl)
	{
		//double ww = 0, wl = 0;

		//_reconControlViewModel?.AdvancedReconControl?.GetWWWL(ref ww, ref wl);
		txtWW.Text = ww.ToString("F0");
		txtWL.Text = wl.ToString("F0");
	}

	public void SetOkCallback(Action<double, double> action)
	{
		_okCallbackAction=action;
	}

	private void BtnClose_OnClick(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	/// <summary>
	/// 自定义 WW/WL 
	/// </summary>
	private void BtnOk_OnClick(object sender, RoutedEventArgs e)
	{
		if (string.IsNullOrEmpty(txtWW.Text.Trim()) || string.IsNullOrEmpty(txtWL.Text.Trim()))
			return;

		if (double.TryParse(txtWW.Text, out var ww) && double.TryParse(txtWL.Text, out var wl) && ww > 0)
		{
			//_reconControlViewModel?.AdvancedReconControl?.SetWWWL(ww, wl);
			_okCallbackAction(ww,wl);
		}

		Hide();
	}
}