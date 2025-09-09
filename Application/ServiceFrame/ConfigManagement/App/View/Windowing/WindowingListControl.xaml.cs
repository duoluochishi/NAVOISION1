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
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.ConfigManagement.View;

public partial class WindowingListControl : UserControl, IServiceControl
{
    public WindowingListControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<WindowingListViewModel>();     
    }
    public string GetServiceAppID()
    {
        return string.Empty;
    }

    public string GetServiceAppName()
    {
        return string.Empty;
    }

    public string GetTipOnClosing()
    {
        return string.Empty;
    }
}