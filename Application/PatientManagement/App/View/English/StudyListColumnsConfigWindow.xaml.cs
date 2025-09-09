//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/30 11:01:27     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English
{
    /// <summary>
    /// StudyListColumnsConfig.xaml 的交互逻辑
    /// </summary>
    public partial class StudyListColumnsConfigWindow : Window
    {
        public StudyListColumnsConfigWindow()
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
                DataContext = scope.ServiceProvider.GetRequiredService<StudyListColumnsConfigViewModel>();
            }
        }
    }
}
