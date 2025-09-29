using NV.CT.ConfigManagement.ViewModel;
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
using System.Windows.Shapes;

namespace NV.CT.ConfigManagement.View
{
    /// <summary>
    /// Interaction logic for BreatheVoiceWindow.xaml
    /// </summary>
    public partial class BreatheVoiceWindow : Window
    {
        public BreatheVoiceWindow()
        {
            InitializeComponent();
            DataContext = DataContext = CTS.Global.ServiceProvider?.GetRequiredService<BreatheVoiceViewModel>();
        }
    }
}
