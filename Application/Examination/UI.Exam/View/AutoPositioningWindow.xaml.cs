//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.UI.Exam.View;

public partial class AutoPositioningWindow : Window
{
    public AutoPositioningWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = 350;
        Top = 40;
        var wih = new WindowInteropHelper(this);
        wih.Owner = ConsoleSystemHelper.WindowHwnd;
        DataContext = Global.ServiceProvider?.GetRequiredService<AutoPositioningViewModel>();
    }
}