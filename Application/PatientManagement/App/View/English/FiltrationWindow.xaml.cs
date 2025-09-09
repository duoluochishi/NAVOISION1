//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English;

public partial class FiltrationWindow
{
    public FiltrationWindow()
    {
        InitializeComponent();
       
        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };

        using (var scope = Global.Instance.ServiceProvider.CreateScope())
        {
            DataContext = scope.ServiceProvider.GetRequiredService<FiltrationViewModel>();
        }
    }
}