using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English
{
    /// <summary>
    /// RawDataImportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RawDataImportWindow : Window
    {
        public RawDataImportWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            DataContext = Global.Instance.ServiceProvider.GetRequiredService<RawDataImportViewModel>();
        }
    }
}
