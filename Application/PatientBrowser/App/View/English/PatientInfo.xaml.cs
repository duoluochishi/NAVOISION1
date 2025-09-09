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
using System;
using System.Windows.Controls;

namespace NV.CT.PatientBrowser.View.English;

/// <summary>
/// PatientInfo.xaml 的交互逻辑
/// </summary>
public partial class PatientInfo : UserControl
{
    public PatientInfo()
    {
        InitializeComponent();
        dtpDateofBirth.DisplayDateStart = DateTime.Now.AddYears(-200);
        dtpDateofBirth.DisplayDateEnd = DateTime.Now;
    }
}