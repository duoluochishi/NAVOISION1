//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/23 15:03:51           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.InterventionalScan.ViewModel;

namespace NV.CT.InterventionalScan.View;
/// <summary>
/// LoadStaticDicomControl.xaml 的交互逻辑
/// </summary>
public partial class LoadStaticDicomControl : UserControl
{
    public LoadStaticDicomControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<DicomImageViewModel>();
    }
}