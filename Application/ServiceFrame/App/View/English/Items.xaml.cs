using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS.Helpers;
using NV.CT.ServiceFrame.ApplicationService.Contract.Models;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.ServiceFrame.View.English;

public partial class Items
{
    private readonly IDialogService _dialogService;

    public Items()
    {
        InitializeComponent();

		_dialogService= ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<IDialogService>();
		lsItems.PreviewMouseLeftButtonDown += LsItems_PreviewMouseLeftButtonDown;
    }

	private void LsItems_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		var lastItem = lsItems.SelectedItem as ChildrenItem;
		if (lastItem is null)
			return;

		var token = ServiceToken.GetCurrentServiceToken();
		if (!string.IsNullOrEmpty(token))
		{
			IServiceControl serviceControl = lastItem.AppControl;
			_dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", serviceControl.GetTipOnClosing(), null, ConsoleSystemHelper.WindowHwnd);

			e.Handled = true;
		}
	}
}
