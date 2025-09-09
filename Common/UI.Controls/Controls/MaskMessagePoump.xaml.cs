using System.Windows;

namespace NV.CT.UI.Controls.Controls;
/// <summary>
/// MaskMessagePoump.xaml 的交互逻辑
/// </summary>
public partial class MaskMessagePoump : Window
{
	public MaskMessagePoump()
	{
		this.Width = SystemParameters.PrimaryScreenWidth + 50;
		this.Height = SystemParameters.PrimaryScreenHeight + 50;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
		InitializeComponent();
	}
}