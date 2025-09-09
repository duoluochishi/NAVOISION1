using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;
using System.Windows.Controls;

namespace NV.CT.ProtocolManagement.Views.English;

/// <summary>
/// MeasurementParameterPage.xaml 的交互逻辑
/// </summary>
public partial class MeasurementParameterControl : UserControl
{
    public MeasurementParameterControl()
    {
        InitializeComponent(); 
        using (var scope = Global.Instance.ServiceProvider.CreateScope())
        {
            this.DataContext = scope.ServiceProvider.GetRequiredService<MeasurementParameterControlViewModel>();
        }
    }
}
