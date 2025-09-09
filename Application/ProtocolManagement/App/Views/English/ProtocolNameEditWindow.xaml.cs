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
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;

namespace NV.CT.ProtocolManagement.Views.English
{
    /// <summary>
    /// ProtocolNameEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolNameEditWindow : Window
    {
        public ProtocolNameEditWindow()
        {
            InitializeComponent();
            var protocolViewModel = Global.Instance.ServiceProvider.GetRequiredService<ProtocolNameEditViewModel>();
            protocolViewModel.NameFocusSetting += OnNameFocusSetting;
            protocolViewModel.WindowClosing += (sender, b) => this.Close();

            this.DataContext = protocolViewModel;
        }

        private void OnNameFocusSetting(object? sender, bool e)
        {
            txtName.Focus();
            txtName.SelectionStart = txtName.Text.Length;
        }
    }
}
