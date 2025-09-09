using NV.CT.Service.UI.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// DailyCaliUC.xaml 的交互逻辑
    /// </summary>
    public partial class DailyCaliUC : UserControl
    {
        public DailyCaliUC()
        {
            InitializeComponent();
        }

        private void LogToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton.IsChecked == true)
            {
                this.logTextBox.Height *= 2;
            }
            else
            {
                this.logTextBox.Height /= 2;
            }
        }

        private static readonly string ClassName = nameof(DailyCaliUC);
    }
}
