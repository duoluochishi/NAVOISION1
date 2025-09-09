//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="CorrectWindow.cs" company="纳米维景">
// 版权所有 (C)2024,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

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
    /// CorrectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CorrectWindow : Window
    {
        private readonly CorrectViewModel? _viewModel;

        public CorrectWindow()
        {          
            InitializeComponent();

            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            _viewModel = Global.Instance.ServiceProvider.GetRequiredService<CorrectViewModel>();
            DataContext = _viewModel;

        }
    }
}
