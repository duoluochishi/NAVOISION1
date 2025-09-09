using NV.CT.ServiceFrame.ApplicationService.Contract.Interfaces;
using NV.CT.ServiceFrame.ApplicationService.Contract.Models;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.ServiceFrame.ViewModel;

public class ItemsViewModel : UI.ViewModel.BaseViewModel
{
	private List<ChildrenItem> _items;
	private string _title;
	private ChildrenItem _selectedItem;
	private readonly IServiceAppControlManager _serviceAppControlManager;
	private readonly IDialogService _dialogService;

	public List<ChildrenItem> Items
	{
		get => _items;
		set => SetProperty(ref _items, value);
	}
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}
	public ChildrenItem SelectedItem
	{
		get => _selectedItem;
		set => SetProperty(ref _selectedItem, value);
	}

	public ItemsViewModel(IServiceAppControlManager serviceAppControlManager, IDialogService dialogService)
	{
		_serviceAppControlManager = serviceAppControlManager;
		_dialogService = dialogService;

		Commands.Add("SwitchControlCommand", new DelegateCommand<object>(SwitchControl));
		
		Initialize();
	}

	private void Initialize()
	{
		Title = Global.Instance.ModelName;
		Items = _serviceAppControlManager.GetAppItems(Global.Instance.ModelName);
		if (Items.Count > 0)
		{
			SelectedItem = Items[0];
			SwitchControl(SelectedItem);
		}
	}

	private void SwitchControl(object obj)
	{
		if (obj is not ChildrenItem)
			return;

		if (SelectedItem is null && Items.Any())
		{
			SelectedItem = Items[0];
		}

		if (SelectedItem is null)
			return;

		_serviceAppControlManager.SetChildren(SelectedItem.AppControl);
	}
}