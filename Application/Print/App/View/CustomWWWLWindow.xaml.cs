//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using NV.CT.Print.ViewModel;
using System.Windows.Input;

namespace NV.CT.Print.View
{
    /// <summary>
    /// CustomWWWLWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CustomWWWLWindow : Window
    {
        private readonly ImageOperationViewModel? _imageOperationViewModel;

        public CustomWWWLWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            txtWW.Text = "1";
            txtWL.Text = "0";
            _imageOperationViewModel = CTS.Global.ServiceProvider.GetService<ImageOperationViewModel>();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// 自定义 WW/WL 
        /// </summary>
        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWW.Text.Trim()) || string.IsNullOrEmpty(txtWL.Text.Trim()))
                return;

            if (double.TryParse(txtWW.Text, out var ww) && double.TryParse(txtWL.Text, out var wl))
            {
                var imageViewer = Global.Instance.ImageViewer;
                imageViewer.SetWWWL(ww, wl);
            }
            Hide();
        }

    }
}
