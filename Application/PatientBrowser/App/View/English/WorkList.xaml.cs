//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:54:59    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using NV.CT.PatientBrowser.Models;
using NV.CT.PatientBrowser.ViewModel;
using NV.CT.SyncService.Contract;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.PatientBrowser.View.English;

/// <summary>
/// WorkList.xaml 的交互逻辑
/// </summary>
public partial class WorkList : UserControl
{
    public WorkList()
    {
        InitializeComponent();
    }
}