//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.RGT.View.Body;

public partial class ChildNormalControl
{
    public ChildNormalControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<ProtocolSelectMainViewModel>();
    }
}