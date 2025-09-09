using NV.CT.CTS.Helpers;
using NV.CT.UI.Controls.Controls;
using System;
using System.Windows;
using System.Windows.Interop;

namespace NV.CT.UI.Controls;
public static class WindowDialogExtension
{
	private static MaskMessagePoump topWindow;
	public static void ShowWindowDialog(this Window window, IntPtr owner, bool isManualLocation = false)
	{
		if (!isManualLocation)
		{
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}
		var wih = new WindowInteropHelper(window);
		if (owner != IntPtr.Zero)
		{
			if (wih.Owner == IntPtr.Zero)
			{
				wih.Owner = owner;
			}
			if (!window.IsVisible)
			{
				window.Activate();
				//隐藏底部状态栏
				window.Topmost = true;
				window.ShowInTaskbar = false;
				window.ShowDialog();
			}
		}
		else
		{
			if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
			{
				wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			}
			if (!window.IsVisible)
			{
				window.ShowInTaskbar = false;
				//必须加这个，不加就会导致状态栏出来
				window.Topmost = true;
				window.Activate();
				window.Show();
			}
		}
	}

	public static void ShowPopWindowDialog(this Window window, IntPtr owner, bool isManualLocation = false)
	{
		if (topWindow is null)
		{
			topWindow = new MaskMessagePoump();
		}
		if (!isManualLocation)
		{
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}
		window.IsVisibleChanged += Window_IsVisibleChanged;
		var wih = new WindowInteropHelper(window);
		if (owner != IntPtr.Zero)
		{
			if (wih.Owner == IntPtr.Zero)
			{
				wih.Owner = new WindowInteropHelper(topWindow).Handle;
			}
			if (!window.IsVisible)
			{
				window.Activate();
				//隐藏底部状态栏
				window.Topmost = true;
				window.ShowInTaskbar = false;
				topWindow.Show();
				window.ShowDialog();
			}
		}
		else
		{
			if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
			{
				wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			}
			if (!window.IsVisible)
			{
				window.ShowInTaskbar = false;
				//必须加这个，不加就会导致状态栏出来
				window.Topmost = true;
				window.Activate();
				window.Show();
			}
		}
	}

	private static void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is false && topWindow is not null)
		{
			topWindow.Hide();
		}
	}

	public static void ShowWindowDialog(this Window window, bool isManualLocation = false)
	{
		window.ShowWindowDialog(ConsoleSystemHelper.WindowHwnd, isManualLocation);
	}

	public static void ShowPopWindowDialog(this Window window, bool isManualLocation = false)
	{
		window.ShowPopWindowDialog(ConsoleSystemHelper.WindowHwnd, isManualLocation);
	}
}