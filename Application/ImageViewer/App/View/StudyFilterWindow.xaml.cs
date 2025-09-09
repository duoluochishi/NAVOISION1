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
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.ImageViewer.Model;
using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

/// <summary>
/// StudyFilterWindow.xaml 的交互逻辑
/// </summary>
public partial class StudyFilterWindow : Window
{
	private readonly IViewerService _viewerService=CTS.Global.ServiceProvider.GetRequiredService<IViewerService>();
	private readonly ILogger<StudyFilterWindow> _logger=CTS.Global.ServiceProvider.GetRequiredService<ILogger<StudyFilterWindow>>();
	public StudyFilterWindow()
	{
		InitializeComponent();

		MouseDown += (_, _) =>
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		};


		DataContext = CTS.Global.ServiceProvider.GetService<StudyFilterViewModel>();
	}

}