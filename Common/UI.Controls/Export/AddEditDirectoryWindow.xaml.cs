//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/9/11 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS;
using System;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.UI.Controls.Export
{
    /// <summary>
    /// AddEditDirectoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddEditDirectoryWindow : Window
    {
        private readonly AddEditDirectoryViewModel? _viewModel;

        public AddEditDirectoryWindow()
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

            _viewModel = Global.ServiceProvider.GetRequiredService<AddEditDirectoryViewModel>();
            DataContext = _viewModel;
        }
        public void SetDefaultInputFocus()
        {
            txtFolderName.Focus();
        }
    }
}
