using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using System.Windows;
using NV.CT.ProtocolManagement.ViewModels;

namespace NV.CT.ProtocolManagement.Views.English;

/// <summary>
/// SearchBox.xaml 的交互逻辑
/// </summary>
public partial class SearchBoxControl : UserControl
{
    public SearchBoxControl()
    {
        InitializeComponent();
        using (var scope = Global.Instance.ServiceProvider.CreateScope())
        {
            this.DataContext = scope.ServiceProvider.GetRequiredService<SearchBoxViewModel>();
        }
        btn_Search.Command = ((SearchBoxViewModel)DataContext).Commands["SearchCommand"];

        //KeyGesture keyGesture = new KeyGesture(Key.Enter);
        //InputBinding inputBinding = new InputBinding(((SearchBoxViewModel)DataContext).Commands["SearchNotParaCommand"], keyGesture);
        //inputBinding.CommandParameter = txt_Search.Text;
        //btn_Search.InputBindings.Add(inputBinding);

        //btn_Search.InputBindings.Add(new System.Windows.Input.KeyBinding(((SearchBoxViewModel)DataContext).Commands["SearchCommand"], new KeyGesture(Key.Enter)));
    }
}
