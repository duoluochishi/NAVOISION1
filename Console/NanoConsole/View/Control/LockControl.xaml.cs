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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NV.CT.NanoConsole.View.Control;

public partial class LockControl
{
	public LockControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<LockViewModel>();

		var layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
		if (layoutManager!=null)
		{
			layoutManager.LayoutChanged += LayoutChanged;
		}
	}

	private void LayoutChanged(object? sender, Screens e)
	{
		if(e!=Screens.LockScreen)
			return;

		Dispatcher.BeginInvoke(DispatcherPriority.Render,
			new Action(() =>
			{
				txtPassword.Focus();
			}));
	}
}