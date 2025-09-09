//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.View.English;

public partial class TableMoveWindow : Window
{
	public TableMoveWindow()
	{
		InitializeComponent();
		Left = SystemParameters.PrimaryScreenWidth - Width - 8;
		Top = SystemParameters.PrimaryScreenHeight - Height - 40;

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<TableMoveWindowViewModel>();
	}
}