using System.Collections.Specialized;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.TubeCali.ViewModels;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.TubeCali.Views
{
    /// <summary>
    /// TubeCaliView.xaml 的交互逻辑
    /// </summary>
    public partial class TubeCaliView
    {
        private readonly TubeCaliViewModel _vm;

        public TubeCaliView()
        {
            InitializeComponent();
            _vm = Global.Instance.ServiceProvider.GetRequiredService<TubeCaliViewModel>();
            DataContext = _vm;
        }

        private void Root_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.Histories.CollectionChanged += Histories_CollectionChanged;
            _vm.Loaded();
        }

        private void Root_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.Histories.CollectionChanged -= Histories_CollectionChanged;
            _vm.Unloaded();
        }

        private void Histories_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ConsoleControl.FindVisualChild<ScrollViewer>()?.ScrollToEnd();
        }
    }
}