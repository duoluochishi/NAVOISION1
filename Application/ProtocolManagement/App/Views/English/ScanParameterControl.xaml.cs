using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;

namespace NV.CT.ProtocolManagement.Views.English;

/// <summary>
/// ScanParameterControl.xaml 的交互逻辑
/// </summary>
public partial class ScanParameterControl : UserControl
{
    public ScanParameterControl()
    {
        InitializeComponent();
        this.DataContext = Global.Instance.ServiceProvider.GetRequiredService<ScanParameterControlViewModel>();
    }

}
