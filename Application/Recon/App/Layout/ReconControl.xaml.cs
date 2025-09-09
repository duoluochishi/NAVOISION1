//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Recon.Layout;

public partial class ReconControl
{
	public ReconControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<ReconControlViewModel>();
	}
}