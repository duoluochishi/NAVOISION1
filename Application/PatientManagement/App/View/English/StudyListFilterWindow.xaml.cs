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

namespace NV.CT.PatientManagement.View.English
{
    /// <summary>
    /// StudyListFilterWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StudyListFilterWindow : Window
    {
        public StudyListFilterWindow()
        {
            InitializeComponent();

            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            using (var scope = Global.Instance.ServiceProvider.CreateScope())
            {
                DataContext = scope.ServiceProvider.GetRequiredService<StudyListFilterViewModel>();
            }

        }
    }
}
