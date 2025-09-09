//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.ImageViewer.View;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void RegisterScreenShotKeys()
	{
		//注册快捷键
		//Screenshot.Screenshot.RegisterHotKey(this, ModifierKeys.Control, Keys.C);
	}
}