//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.ConfigManagement.ViewModel;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace NV.CT.ConfigManagement.View;

public partial class RoleWindow
{
    public RoleWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<RoleViewModel>();
    }
}