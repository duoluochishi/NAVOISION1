using System;
using System.Windows;
using NV.CT.Service.QualityTest.Models;
using NV.MPS.UI.Dialog;

namespace NV.CT.Service.QualityTest.Views
{
    /// <summary>
    /// ReportTitleInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ReportHeadInfoWindow : BaseCustomWindow
    {
        public ReportHeadInfoWindow(ReportHeadInfoModel model)
        {
            InitializeComponent();
            model.GenerationDate = DateTime.Now;
            DataContext = model;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}