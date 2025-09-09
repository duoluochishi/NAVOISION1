//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.UI.Exam.View;

public partial class SelectProtocolParameterControl
{
    public SelectProtocolParameterControl()
    {
        InitializeComponent();
        ScamGrid.DataContext = Global.ServiceProvider.GetRequiredService<ScanParameterViewModel>();
        ReconGrid.DataContext = Global.ServiceProvider.GetRequiredService<ReconParameterViewModel>();
    }
}