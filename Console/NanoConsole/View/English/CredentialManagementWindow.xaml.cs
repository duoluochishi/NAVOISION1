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

namespace NV.CT.NanoConsole.View.English;

public partial class CredentialManagementWindow : Window
{
	public CredentialManagementWindow()
	{
		InitializeComponent();
		SetAtPrimaryScreenCenter();

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<CredentialManagementViewModel>();
	}

	private void SetAtPrimaryScreenCenter()
	{
		Left = (SystemParameters.PrimaryScreenWidth - this.Width)/2 ;
		Top = (SystemParameters.PrimaryScreenHeight - this.Height)/2;
	}
}