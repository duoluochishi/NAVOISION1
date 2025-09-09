using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.TubeHistory.ViewModels;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.TubeHistory.Views
{
    /// <summary>
    /// TubeHistoryView.xaml 的交互逻辑
    /// </summary>
    public partial class TubeHistoryView
    {
        private bool _isFirstLoad = true;
        private TubeHistoryViewModel _vm = null!;

        public TubeHistoryView()
        {
            InitializeComponent();
        }

        private async void Root_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstLoad)
            {
                _vm = Global.Instance.ServiceProvider.GetRequiredService<TubeHistoryViewModel>();
                DataContext = _vm;
                _isFirstLoad = false;
            }

            await _vm.OnLoaded();
        }
    }
}