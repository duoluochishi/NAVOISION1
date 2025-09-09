using System.Windows;
using NV.CT.Service.QualityTest.Enums;
using NV.MPS.UI.Dialog;

namespace NV.CT.Service.QualityTest.Views.MTF
{
    /// <summary>
    /// SelectImage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectImageWindow : BaseCustomWindow
    {
        public UCImageSelectType Select { get; private set; }

        public SelectImageWindow()
        {
            InitializeComponent();
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            Select = UCImageSelectType.Left;
            DialogResult = true;
        }

        private void ButtonCenter_Click(object sender, RoutedEventArgs e)
        {
            Select = UCImageSelectType.Center;
            DialogResult = true;
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            Select = UCImageSelectType.Right;
            DialogResult = true;
        }
    }
}