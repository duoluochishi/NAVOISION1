//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

/// <summary>
/// 列表控件是之前的需求, 现在的需求是使用卡片展示
/// </summary>
public partial class StudyListControl
{
    public StudyListControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider.GetRequiredService<StudyViewModel>();
    }
}