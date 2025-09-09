using NV.CT.Service.TubeWarmUp.ViewModels;
using NV.CT.ServiceFramework.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NV.CT.Service.TubeWarmUp
{
    /// <summary>
    /// TubeWarmUpWarpper.xaml 的交互逻辑
    /// </summary>
    public partial class TubeWarmUpWarpper : UserControl, IServiceControl
    {
        public TubeWarmUpWarpper()
        {
            InitializeComponent();

            var vm = warmupView.DataContext as WarmUpViewModel;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        #region 适应MCS服务框架接口要求

        public static readonly string Service_App_Name = "TubeWarmUp";

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WarmUpViewModel.WarmUpDoing))
            {
                var warmUpViewModel = sender as WarmUpViewModel;
                if (warmUpViewModel != null && warmUpViewModel.WarmUpDoing)
                {
                    ServiceToken.Take(Service_App_Name);
                }
                else if (warmUpViewModel != null && !warmUpViewModel.WarmUpDoing)
                {
                    ServiceToken.Release();
                }
            }
        }

        public string GetServiceAppName()
        {
            return Service_App_Name;
        }

        public string GetServiceAppID()
        {
            return Service_App_Name;
        }

        public string GetTipOnClosing()
        {
            var vm = this.warmupView.DataContext as WarmUpViewModel;
            if (vm != null)
            {
                return vm.WarmUpDoing ? "Application is warming up" : string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion 适应MCS服务框架接口要求
    }
}