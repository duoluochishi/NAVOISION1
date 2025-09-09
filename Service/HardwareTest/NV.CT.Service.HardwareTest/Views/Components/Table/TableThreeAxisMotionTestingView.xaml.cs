using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.Table;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views.Components.Table
{
    public partial class TableThreeAxisMotionTestingView : Grid
    {
        public TableThreeAxisMotionTestingView()
        {
            InitializeComponent();
            /** DataContext **/
            this.DataContext = Ioc.Default.GetService<TableThreeAxisMotionTestingViewModel>();
        }

        private void Console_TextChanged(object sender, TextChangedEventArgs e)
        {
            consoleTextBox.ScrollToEnd();
        }
    }
}
