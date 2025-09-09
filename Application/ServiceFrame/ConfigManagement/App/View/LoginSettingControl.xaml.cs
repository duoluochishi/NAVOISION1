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
using NV.CT.ConfigManagement.ViewModel;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.ConfigManagement.View;

public partial class LoginSettingControl : UserControl,IServiceControl
{
	public LoginSettingControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider?.GetRequiredService<LoginSettingViewModel>();
	}

	public string GetServiceAppName()
	{
		return string.Empty;
	}

	public string GetServiceAppID()
	{
		return string.Empty;
	}

	public string GetTipOnClosing()
	{
		return string.Empty;
	}
}