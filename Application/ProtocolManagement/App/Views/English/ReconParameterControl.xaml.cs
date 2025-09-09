using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;

namespace NV.CT.ProtocolManagement.Views.English;

/// <summary>
/// ReconParameterControl.xaml 的交互逻辑
/// </summary>
public partial class ReconParameterControl : UserControl
{
    public ReconParameterControl()
    {
        InitializeComponent();
        this.DataContext = Global.Instance.ServiceProvider.GetRequiredService<ReconParameterControlViewModel>();
    }
}
