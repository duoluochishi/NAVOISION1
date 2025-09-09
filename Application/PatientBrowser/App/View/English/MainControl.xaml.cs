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
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.PatientBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.PatientBrowser.View.English;
/// <summary>
/// MainControl.xaml 的交互逻辑
/// </summary>
public partial class MainControl : UserControl
{
    public PatientInfoViewModel PatientInfoViewModel { get; set; }
    public WorkListViewModel WorkListViewModel { get; set; }
    public MainControl(List<ResourceDictionary> list)
    {
        InitializeComponent();
        if (list != null && list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Resources.MergedDictionaries.Add(list[i]);
            }
        }
        this.DataContext = this;
        using (var scope = Global.Instance.ServiceProvider.CreateScope())
        {
            WorkListViewModel = scope.ServiceProvider.GetRequiredService<WorkListViewModel>();
            PatientInfoViewModel = scope.ServiceProvider.GetRequiredService<PatientInfoViewModel>();
        }
    }
}