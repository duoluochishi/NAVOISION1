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
using System.Windows.Threading;

namespace NV.CT.NP.Tools.DataTransfer.View
{
    /// <summary>
    /// LoadingControl.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingControl : System.Windows.Controls.UserControl
    {
        public static DependencyProperty TextDependencyProperty = DependencyProperty.Register("Text", typeof(string), typeof(LoadingControl), new PropertyMetadata(""));
        public string Text
        {
            get => (string)GetValue(TextDependencyProperty);
            set => SetValue(TextDependencyProperty, value);
        }

        public LoadingControl()
        {
            InitializeComponent();
            this.Loaded += LoadingControl_Loaded;
        }

        private void LoadingControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(Text))
            {
                txtMessage.Text = Text;
            }
        }

    }
}
