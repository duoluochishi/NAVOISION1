using NV.CT.Service.TubeWarmUp.DependencyInject;
using NV.CT.Service.TubeWarmUp.ViewModels;
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

namespace NV.CT.Service.TubeWarmUp.Views
{
    /// <summary>
    /// WarmUpView.xaml 的交互逻辑
    /// </summary>
    public partial class WarmUpView : UserControl
    {
        public WarmUpViewModel VM { get; set; }

        public WarmUpView()
        {
            InitializeComponent();
            VM = ServiceLocator.Instance.GetService<WarmUpViewModel>();
            this.DataContext = VM;
        }
    }
}