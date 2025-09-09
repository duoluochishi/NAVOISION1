using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.ComponentHistory.Models;
using NV.CT.Service.ComponentHistory.ViewModels;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.ComponentHistory.Views
{
    /// <summary>
    /// ComponentHistoryView.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentHistoryView
    {
        private bool _isFirstLoad = true;
        private ComponentHistoryViewModel _vm = null!;

        public ComponentHistoryView()
        {
            InitializeComponent();
        }

        private async void Root_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstLoad)
            {
                _vm = Global.Instance.ServiceProvider.GetRequiredService<ComponentHistoryViewModel>();
                DataContext = _vm;
                _isFirstLoad = false;
            }

            await _vm.OnLoaded();
        }

        private void ListBoxItemGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject dependency)
            {
                var listBoxItem = dependency.FindParent<ListBoxItem>();

                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }
            }
        }

        private void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
            {
                return;
            }

            var win = btn.DataContext switch
            {
                ComponentEntryItemModel entryItem => new HistoryWindow(entryItem) { Owner = Window.GetWindow(this) },
                ComponentCategoryModel categoryItem => new HistoryWindow(categoryItem) { Owner = Window.GetWindow(this) },
                _ => null
            };
            win?.ShowDialog();
        }
    }
}