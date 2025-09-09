using NV.CT.ServiceFramework.Contract;
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

namespace NV.CT.Cloud;

public partial class MainControl : UserControl,IServiceControl
{
	public MainControl()
	{
		InitializeComponent();
	}

	public string GetServiceAppID()
	{
		return string.Empty;
	}

	public string GetServiceAppName()
	{
		return string.Empty;
	}

	public string GetTipOnClosing()
	{
		return string.Empty;
	}
}