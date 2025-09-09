using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Upgrade.Initializer;
using NV.CT.Service.Upgrade.Models;
using NV.CT.Service.Upgrade.ViewModels;

namespace NV.CT.Service.Upgrade.Views
{
    /// <summary>
    /// UpgradeView.xaml 的交互逻辑
    /// </summary>
    public partial class UpgradeView
    {
        private bool _isFirstLoad = true;
        private UpgradeViewModel? _vm;

        public UpgradeView()
        {
            InitializeComponent();
        }

        private async void Root_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstLoad)
            {
                ProgramInit.Init();
                _vm = Global.ServiceProvider.GetRequiredService<UpgradeViewModel>();
                DataContext = _vm;
                _isFirstLoad = false;
                _vm.OnLoaded();
                await Task.Yield();
                _vm.OnContentRendered();
            }
            else
            {
                _vm!.OnLoaded();
            }
        }

        private void Root_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm?.OnUnLoaded();
        }

        private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            _vm!.UpgradeFolder = openFileDialog.SelectedPath;
            _vm.AnalyzeUpgradeFolder(openFileDialog.SelectedPath);
        }

        private void TypeFilter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            TypeFilterPopup.IsOpen = true;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not FirmwareModel item)
            {
                e.Accepted = true;
                return;
            }

            var cur = _vm?.FilterFirmwareTypes.FirstOrDefault(i => i.FirmwareType == item.FirmwareType);

            if (cur == null)
            {
                e.Accepted = true;
                return;
            }

            e.Accepted = !cur.IsChecked.HasValue || cur.IsChecked.Value;
        }

        private void CheckBoxFilter_Click(object sender, RoutedEventArgs e)
        {
            DataGridItems.CommitEdit();
            var cvs = CollectionViewSource.GetDefaultView(DataGridItems.ItemsSource);
            cvs?.Refresh();
        }
    }
}