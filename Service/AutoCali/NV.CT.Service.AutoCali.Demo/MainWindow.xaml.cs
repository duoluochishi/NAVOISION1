using NV.CT.Service.AutoCali.Logic;
using System.Windows;

namespace NV.CT.Service.AutoCali.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestAlgCalculator_Click(object sender, RoutedEventArgs e)
        {
            HelicScanCalcutorHelper.CalcTableControl();
        }
    }
}
