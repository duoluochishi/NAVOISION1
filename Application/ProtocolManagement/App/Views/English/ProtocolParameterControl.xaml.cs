using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static System.Formats.Asn1.AsnWriter;

namespace NV.CT.ProtocolManagement.Views.English;

/// <summary>
/// ProtocolControl.xaml 的交互逻辑
/// </summary>
public partial class ProtocolParameterControl : UserControl
{
    public ProtocolParameterControl()
    {
        InitializeComponent();
        this.DataContext = Global.Instance.ServiceProvider.GetRequiredService<ProtocolParameterControlViewModel>();
    }
}
