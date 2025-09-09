using Microsoft.Extensions.DependencyInjection;
using NV.CT.ServiceFrame.ViewModel;

namespace NV.CT.ServiceFrame.View.English;

public partial class MainControl
{
    //public ItemsViewModel ItemsViewModel { get; set; }

    public MainControl()
    {
        InitializeComponent();

        DataContext = CTS.Global.ServiceProvider.GetRequiredService<MainViewModel>();       
        itemsControl.DataContext = CTS.Global.ServiceProvider.GetRequiredService<ItemsViewModel>();
    }
}