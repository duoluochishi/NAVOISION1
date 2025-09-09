using System.Diagnostics;
using System.Windows;

namespace NV.CT.Service.Hardware.StartUp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            /** Shutdown **/
            Application.Current.Shutdown();
            /** Kill Process **/
            Process.GetCurrentProcess().Kill();
        }
    }
}
