namespace NV.CT.RGT.Layout;

public partial class ProtocolSelection
{
    private readonly ProtocolSelectMainViewModel _vm;
    public ProtocolSelection()
    {
        InitializeComponent();

        _vm = CTS.Global.ServiceProvider.GetRequiredService<ProtocolSelectMainViewModel>();
        DataContext = _vm;
    }

    private void ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
        var listView = sender as ListView;
        if(listView?.SelectedItem is ProtocolViewModel selectedItem)
			_vm.SendSelectedProtocolToMcs(selectedItem.Id);
    }
}