//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/11/6 16:35:59    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.NanoConsole.View.English;

public partial class MessagesWindow : Window
{
    public MessagesWindow()
    {
        InitializeComponent();

        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };

        using (var scope = CTS.Global.ServiceProvider?.CreateScope())
        {
            DataContext = scope?.ServiceProvider.GetRequiredService<MessagesViewModel>();
        }
    }
}