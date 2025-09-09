//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/5 11:01:27      V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.UI.Controls.Archive
{
    /// <summary>
    /// ArchiveWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ArchiveWindow : Window
    {
        public ArchiveWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            };

            DataContext = Global.ServiceProvider.GetRequiredService<ArchiveWindowViewModel>();
        }
    }
}
