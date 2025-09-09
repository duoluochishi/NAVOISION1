//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.View.Control;

public partial class SelfCheckWindow
{
	public SelfCheckWindow()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<TableMoveWindowViewModel>();
	}
}