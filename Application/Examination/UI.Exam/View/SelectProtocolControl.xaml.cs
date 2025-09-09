//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Windows.Input;

namespace NV.CT.UI.Exam.View;

public partial class SelectProtocolControl
{
    private readonly ProtocolSelectMainViewModel _protocolSelectMainViewModel;
    public SelectProtocolControl()
    {
        InitializeComponent();
        _protocolSelectMainViewModel = Global.ServiceProvider.GetRequiredService<ProtocolSelectMainViewModel>();

        DataContext = _protocolSelectMainViewModel;
    }

    /// <summary>
    /// 外侧列表双击事件
    /// </summary>
    private void ListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _protocolSelectMainViewModel.ReplaceProtocolToTaskList();
    }
}