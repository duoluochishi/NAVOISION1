//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.View.English;

public partial class CopyrightControl : UserControl
{
    public CopyrightControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider.GetRequiredService<CopyrightViewModel>();
    }

    private void nanoLogo_MouseLeave(object sender, MouseEventArgs e)
    {
        p_SystemInfo.IsOpen = false;
    }

    private void nanoLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        p_SystemInfo.IsOpen = true;
    }

    private void p_SystemInfo_MouseLeave(object sender, MouseEventArgs e)
    {
        p_SystemInfo.IsOpen = false;
    }
}