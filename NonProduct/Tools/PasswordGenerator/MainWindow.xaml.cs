using NV.CT.CTS.Encryptions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NV.CT.NP.Tools.PasswordGenerator;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void BtnEncrypt_OnClick(object sender, RoutedEventArgs e)
	{
		TxtMessage.Text = "";

		EncryptedPassword.Text = MD5Helper.Encrypt(PlainPassword.Text.Trim());
	}

	private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
	{
		try
		{
			Clipboard.Clear();
			Clipboard.SetText(EncryptedPassword.Text.Trim());

			TxtMessage.Text = "Copy success";
		}
		catch (Exception ex)
		{
			TxtMessage.Text = "Copy failed";
			MessageBox.Show($"copy to clipboard error:{ex.Message}");
		}
	}
}